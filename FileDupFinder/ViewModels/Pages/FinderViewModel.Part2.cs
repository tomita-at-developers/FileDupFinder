using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using FileDupFinder.Models;

namespace FileDupFinder.ViewModels.Pages
{
    /// <summary>
    /// ファイルの重複チェック画面に対応するViewModelです（非同期検索タスクおよびコア計算ロジック定義）。
    /// クラスを分割（partial）することで、長大な処理コードを分離し保守性を向上させています。
    /// </summary>
    public partial class FinderViewModel
    {
        /// <summary>
        /// コマンド：重複検索を非同期（マルチスレッド）で実行します。
        /// 数十万件のディスク走査や重い計算中も、WPFのUIスレッド（画面の描画やボタンのクリック応答）が絶対にフリーズしないよう Task.Run へ処理を逃がします。
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSearch))]
        private async Task SearchDuplicatesAsync()
        {
            if (!Directory.Exists(TargetFolderPath))
            {
                StatusMessage = "指定されたフォルダが存在しません。";
                return;
            }

            IsBusy = true;
            CanCancel = true;
            SelectFolderCommand.NotifyCanExecuteChanged(); // 走査中のフォルダ再選択を防ぐため参照ボタンをグレーアウト
            StatusMessage = "ファイルをスキャン中...";
            DuplicateFiles.Clear();

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // ワーカースレッド（Task.Run内）からUIメインスレッドへ、現在の進捗件数を安全に届けるためのコールバック窓口
            var progress = new Progress<int>(count =>
            {
                StatusMessage = $"ファイルをスキャン中... ({count:N0} 件スキャン済み)";
            });

            try
            {
                // 【非同期の核心】FindDuplicatesロジックを別スレッドで安全に完全非同期駆動
                var duplicates = await Task.Run(() => FindDuplicates(TargetFolderPath, _mainParent.Settings, progress, token), token);

                StatusMessage = "重複グループを分析中...";

                // 割り出された物理データのリストから、画面表示・操作用の個別ViewModelへと順次ラップしてリストに格納
                foreach (var file in duplicates)
                {
                    token.ThrowIfCancellationRequested(); // ループの節目でもキャンセル要求を確認

                    var itemVM = new FileItemViewModel(file);

                    // 各ファイルアイテムが「削除ボタン」を押されて自身を消し去った際の、親側のリスト追従処理
                    itemVM.Deleted += (s, e) => {
                        if (s is FileItemViewModel vm)
                            App.Current.Dispatcher.Invoke(() => DuplicateFiles.Remove(vm));
                    };
                    DuplicateFiles.Add(itemVM);
                }

                StatusMessage = $"検索完了: {DuplicateFiles.Count} 個の重複ファイルが見つかりました。";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "検索がユーザーによってキャンセルされました。";
                DuplicateFiles.Clear(); // 中途半端なリストが残らないよう美しくクリア
            }
            catch (Exception ex)
            {
                StatusMessage = $"エラーが発生しました: {ex.Message}";
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
                IsBusy = false;
                CanCancel = false;
                SelectFolderCommand.NotifyCanExecuteChanged(); // フォルダ参照ボタンを再度クリック可能に戻す
            }
        }

        /// <summary>
        /// コマンド：現在実行中の検索バックグラウンド処理へ即座に中断シグナルを送信します。
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanCancel))]
        private void CancelSearch()
        {
            _cts?.Cancel();
            StatusMessage = "キャンセル要求を送信中...";
            CanCancel = false;
        }

        /// <summary>
        /// [バックグラウンド実行専用] ファイルシステム内を再帰走査し、サイズおよびハッシュを2段階で精査して重複を割り出す核心関数。
        /// </summary>
        private List<FileInfoItem> FindDuplicates(string folderPath, SearchSettings settings, IProgress<int> progress, CancellationToken token)
        {
            var resultList = new List<FileInfoItem>();
            try
            {
                // Windowsのシステムフォルダやアクセス権限がない隠し領域に激突しても、強制終了せず安全に「無視してスキップ」する防護オプション
                var enumerationOptions = new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true };
                var allowedExts = settings.IsExtensionFilterEnabled ? settings.TargetExtensions.Split(',').Select(e => e.Trim().ToLowerInvariant()).ToHashSet() : null;
                var filesList = new List<FileInfo>();
                int scannedCount = 0;

                // 【第1段階：超高速ファイル全列挙】
                foreach (var path in Directory.EnumerateFiles(folderPath, "*.*", enumerationOptions))
                {
                    token.ThrowIfCancellationRequested(); // 1ファイル走査するごとに超高速でキャンセルの有無を検知
                    scannedCount++;

                    if (scannedCount % 1000 == 0) progress.Report(scannedCount);

                    try
                    {
                        var f = new FileInfo(path);
                        if (allowedExts != null && !allowedExts.Contains(f.Extension.ToLowerInvariant())) continue;
                        filesList.Add(f);
                    }
                    catch { }
                }
                progress.Report(scannedCount);

                // 【第2段階：一次フィルター（LINQによる高速グループ分類）】
                var groups = filesList.GroupBy(f => {
                    string key = "";
                    if (settings.CompareFileName) key += $"|Name:{f.Name.ToLowerInvariant()}";
                    if (settings.CompareTimestamp) key += $"|Time:{f.LastWriteTime:yyyyMMddHHmmss}";
                    if (!settings.CompareHash) key += $"|Size:{f.Length}";
                    return new { Key = key, Size = f.Length };
                });

                var finalGroups = new Dictionary<string, List<FileInfoItem>>();

                // 【第3段階：二次フィルター（真の重複精査：ハッシュ値の抽出）】
                foreach (var g in groups)
                {
                    foreach (var fileInfo in g)
                    {
                        token.ThrowIfCancellationRequested();
                        string matchGroupKey = g.Key.Key;

                        if (settings.CompareHash)
                        {
                            string hash = FileInfoItem.CalculateHash(fileInfo.FullName);
                            if (string.IsNullOrEmpty(hash)) continue;
                            matchGroupKey += $"|Hash:{hash}";
                        }

                        var item = new FileInfoItem
                        {
                            FilePath = fileInfo.FullName,
                            FileSize = fileInfo.Length,
                            Hash = settings.CompareHash ? matchGroupKey.Split("Hash:").Last() : "N/A",
                            CreationTime = fileInfo.CreationTime,
                            LastWriteTime = fileInfo.LastWriteTime
                        };

                        if (!finalGroups.ContainsKey(matchGroupKey)) finalGroups[matchGroupKey] = new List<FileInfoItem>();
                        finalGroups[matchGroupKey].Add(item);
                    }
                }

                // 【第4段階：最終集計（重複の確定）】
                foreach (var group in finalGroups.Values.Where(g => g.Count > 1))
                {
                    token.ThrowIfCancellationRequested();
                    resultList.AddRange(group);
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            return resultList;
        }
    }
}
