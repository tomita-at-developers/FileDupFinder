using System;
using System.Collections.Generic;
using System.Diagnostics; // 👈 Stopwatchを使用するために追加
using System.IO;
using System.Linq;
using System.Threading;
using FileDupFinder.Models;

namespace FileDupFinder.ViewModels.Pages
{
    /// <summary>
    /// ファイル重複チェックのバックグラウンド走査および「N秒おき」の進捗通知アルゴリズム部です。
    /// </summary>
    public partial class FinderViewModel
    {
        private List<FileInfoItem> FindDuplicates(string folderPath, SearchSettings settings, IProgress<FinderProgressReport> progress, int totalSteps, CancellationToken token)
        {
            var resultList = new List<FileInfoItem>();
            // 💡 時間を計測するためのストップウォッチを起動
            var stopwatch = Stopwatch.StartNew();
            double lastReportTime = 0;

            // 💡 内部関数：前回の報告から定数（N秒）が経過したかチェックし、経過していれば報告する仕組み
            void ReportIfTimeElapsed(string text, double value, double max, System.Windows.Visibility visibility, bool force = false)
            {
                double currentTime = stopwatch.Elapsed.TotalSeconds;
                // 強制(force)フラグ、または前回の報告からN秒以上経過している場合のみ画面へ送出
                if (force || (currentTime - lastReportTime) >= ProgressUpdateIntervalSeconds)
                {
                    lastReportTime = currentTime;
                    progress.Report(new FinderProgressReport
                    {
                        Text = text,
                        Value = value,
                        Maximum = max,
                        ProgressBarVisibility = visibility
                    });
                }
            }

            try
            {
                var enumerationOptions = new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true };
                var allowedExts = settings.IsExtensionFilterEnabled ? settings.TargetExtensions.Split(',').Select(e => e.Trim().ToLowerInvariant()).ToHashSet() : null;
                var filesList = new List<FileInfo>();
                int scannedCount = 0;

                // --------------------------------------------------------------------------------
                // 【STEP 1/N：フォルダー検索中】
                // --------------------------------------------------------------------------------
                foreach (var path in Directory.EnumerateFiles(folderPath, "*.*", enumerationOptions))
                {
                    token.ThrowIfCancellationRequested();
                    scannedCount++;

                    // 件数ベースではなく、N秒経過したタイミングでテキストを更新
                    ReportIfTimeElapsed($"STEP 1/{totalSteps}：フォルダー検索中... ({scannedCount:N0} 件スキャン済み)", 0, 0, System.Windows.Visibility.Collapsed);

                    try
                    {
                        var f = new FileInfo(path);
                        if (allowedExts != null && !allowedExts.Contains(f.Extension.ToLowerInvariant())) continue;
                        filesList.Add(f);
                    }
                    catch { }
                }

                // --------------------------------------------------------------------------------
                // 【STEP 2/N：重複ファイル検索中 (一次選別)】
                // --------------------------------------------------------------------------------
                int step2Total = filesList.Count;
                int step2Count = 0;
                var initialGroups = new Dictionary<(string Key, long Size), List<FileInfo>>();

                foreach (var f in filesList)
                {
                    token.ThrowIfCancellationRequested();
                    step2Count++;

                    // N秒おきにプログレスバーと進捗テキストを滑らかに更新
                    ReportIfTimeElapsed($"STEP 2/{totalSteps}：重複ファイル検索中... ({step2Count:N0} / {step2Total:N0})", step2Count, step2Total, System.Windows.Visibility.Visible);

                    string key = "";
                    if (settings.CompareFileName) key += $"|Name:{f.Name.ToLowerInvariant()}";
                    if (settings.CompareTimestamp) key += $"|Time:{f.LastWriteTime:yyyyMMddHHmmss}";
                    if (!settings.CompareHash) key += $"|Size:{f.Length}";

                    var groupKey = (key, f.Length);
                    if (!initialGroups.TryGetValue(groupKey, out List<FileInfo>? value))
                    {
                        value = [];
                        initialGroups[groupKey] = value;
                    }
                    value.Add(f);
                }

                var finalGroups = new Dictionary<string, List<FileInfoItem>>();

                // --------------------------------------------------------------------------------
                // 【STEP 3/N：重複ファイル検索中(詳細) (ハッシュ精密計算)】
                // --------------------------------------------------------------------------------
                if (settings.CompareHash)
                {
                    var candidateGroups = initialGroups.Values.Where(g => g.Count > 1).ToList();
                    int step3Total = candidateGroups.Sum(g => g.Count);
                    int step3Count = 0;

                    // ステップ切り替えの瞬間は、force=true で強制的に「0%」リセットを画面に反映
                    ReportIfTimeElapsed($"STEP 3/{totalSteps}：重複ファイル検索中(詳細)... (0 / {step3Total:N0})", 0, step3Total, System.Windows.Visibility.Visible, force: true);

                    foreach (var g in candidateGroups)
                    {
                        foreach (var fileInfo in g)
                        {
                            token.ThrowIfCancellationRequested();
                            step3Count++;

                            // 重いハッシュ計算中も、進捗件数に関わらず「N秒おき」に確実にバーが動く
                            ReportIfTimeElapsed($"STEP 3/{totalSteps}：重複ファイル検索中(詳細)... ({step3Count:N0} / {step3Total:N0})", step3Count, step3Total, System.Windows.Visibility.Visible);

                            string hash = FileInfoItem.CalculateHash(fileInfo.FullName);
                            if (string.IsNullOrEmpty(hash)) continue;

                            string matchGroupKey = $"Size:{fileInfo.Length}|Hash:{hash}";
                            var item = new FileInfoItem
                            {
                                FilePath = fileInfo.FullName,
                                FileSize = fileInfo.Length,
                                Hash = hash,
                                CreationTime = fileInfo.CreationTime,
                                LastWriteTime = fileInfo.LastWriteTime
                            };

                            if (!finalGroups.TryGetValue(matchGroupKey, out List<FileInfoItem>? value))
                            {
                                value = [];
                                finalGroups[matchGroupKey] = value;
                            }
                            value.Add(item);
                        }
                    }
                }
                else
                {
                    foreach (var g in initialGroups.Values.Where(g => g.Count > 1))
                    {
                        foreach (var fileInfo in g)
                        {
                            var item = new FileInfoItem
                            {
                                FilePath = fileInfo.FullName,
                                FileSize = fileInfo.Length,
                                Hash = "N/A",
                                CreationTime = fileInfo.CreationTime,
                                LastWriteTime = fileInfo.LastWriteTime
                            };
                            string matchGroupKey = $"Size:{fileInfo.Length}";
                            if (!finalGroups.TryGetValue(matchGroupKey, out List<FileInfoItem>? value))
                            {
                                value = [];
                                finalGroups[matchGroupKey] = value;
                            }
                            value.Add(item);
                        }
                    }
                }

                foreach (var group in finalGroups.Values.Where(g => g.Count > 1))
                {
                    token.ThrowIfCancellationRequested();
                    resultList.AddRange(group);
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            finally { stopwatch.Stop(); } // 終了時に計測を停止

            return resultList;
        }
    }
}
