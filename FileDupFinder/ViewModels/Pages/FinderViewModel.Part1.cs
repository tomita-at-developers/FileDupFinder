using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace FileDupFinder.ViewModels.Pages
{
    /// <summary>
    /// ファイルの重複チェック画面に対応するViewModelです（状態プロパティおよびUI基本コマンド定義）。
    /// Mvvm Toolkitのソースジェネレーターと親和性が高く、コード分析ツールにも最適な .NET 10.0 推奨の「部分プロパティ(partial property)」方式を採用しています。
    /// </summary>
    public partial class FinderViewModel : ObservableObject
    {
        // 画面遷移を要求するために親MainWindowViewModelへの参照を保持
        private readonly MainWindowViewModel _mainParent;

        // 非同期実行中の重い検索タスクを、どのフェーズからでも安全かつ強制的に即時遮断するためのコントロールソース
        private CancellationTokenSource? _cts;

        /// <summary>
        /// ユーザーがテキストボックスに入力、またはダイアログから選択した、調査対象となるフォルダのフルパスです。
        /// パスが書き換わった瞬間に、検索開始ボタン（SearchDuplicatesCommand）の実行可否状態（CanSearch）を自動で再評価させます。
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SearchDuplicatesCommand))]
        public partial string TargetFolderPath { get; set; } = string.Empty;

        /// <summary>
        /// 画面最下部のステータスバーに表示されるメッセージです。
        /// 裏スレッドから進行状況（「15,000 件スキャン済み」など）を受け取り、リアルタイムにユーザーへ進捗を伝えます。
        /// </summary>
        [ObservableProperty]
        public partial string StatusMessage { get; set; } = "フォルダを選択して検索を開始してください。";

        /// <summary>
        /// 現在検索処理を実行中（バックグラウンドタスクが稼働中）であるかどうかを示す状態フラグ。
        /// この値が true の間、検索開始ボタンをグレーアウトさせて二重実行のコンフリクトを完全に防御します。
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SearchDuplicatesCommand))]
        public partial bool IsBusy { get; set; }

        /// <summary>
        /// IsBusy の論理反転プロパティ。
        /// スキャン中でない（＝アプリが待機状態である）時だけフォルダ参照ダイアログや設定画面への遷移ボタンを押せるようにUIを縛るために使用します。
        /// </summary>
        public bool IsNotBusy => !IsBusy;

        /// <summary>
        /// 検索実行中に「キャンセルボタン」をクリック可能にするためのアクティブフラグです。
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CancelSearchCommand))]
        public partial bool CanCancel { get; set; }

        /// <summary>
        /// 発見された重複ファイルの個別ビューモデルを格納するスレッド安全なコレクションです。
        /// ViewのListBoxにデータバインディングされており、要素の増減をWPF側が自動検知して描画を追従させます。
        /// </summary>
        public ObservableCollection<FileItemViewModel> DuplicateFiles { get; } = new();

        /// <summary>
        /// 親のMainWindowViewModelが持つ「設定画面へ遷移するコマンド」へアクセスするためのプロパティラッパー。
        /// </summary>
        public IRelayCommand MoveToSettingsCommand => _mainParent.NavigateToSettingsCommand;

        // 検索ボタンを実行可能にするための内部条件式（フォルダパスが入力されており、かつ現在スキャン中でないこと）
        private bool CanSearch => !string.IsNullOrEmpty(TargetFolderPath) && !IsBusy;

        /// <summary>
        /// コンストラクタ。起動直後の初期状態（設定画面をまだ一度も開いていない状態）であっても、
        /// ボタンの有効・無効状態のロジック判定を強制的に実行させ、グレーアウト不具合を完璧に抑止します。
        /// </summary>
        public FinderViewModel(MainWindowViewModel mainParent)
        {
            _mainParent = mainParent;
            SearchDuplicatesCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// コマンド：Windows標準のフォルダ選択ダイアログを開き、選択されたフォルダパスをテキストボックスへ自動セットします。
        /// アプリが検索待機中（IsNotBusy）の場合のみクリック可能です。
        /// </summary>
        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private void SelectFolder()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "スキャンするフォルダを選択してください",
                InitialDirectory = Directory.Exists(TargetFolderPath) ? TargetFolderPath : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialog.ShowDialog() == true)
            {
                TargetFolderPath = dialog.FolderName;
            }
        }
    }
}
