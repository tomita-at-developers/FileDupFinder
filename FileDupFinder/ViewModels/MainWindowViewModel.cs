using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDupFinder.Models;
using FileDupFinder.ViewModels.Pages;

namespace FileDupFinder.ViewModels
{
    /// <summary>
    /// アプリケーション全体のメインウィンドウに対応する親玉ViewModelです。
    /// 画面全体のナビゲーション（検索画面と設定画面の切り替え）および、
    /// アプリケーション全体で永続して共有される検索設定オブジェクトの一元管理を担当します。
    /// </summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        /// <summary>
        /// アプリケーション全体で一元管理される検索条件の設定オブジェクト。
        /// メイン画面での重複分析時、および設定画面でのユーザー入力時に同じこのインスタンスが参照され、変更がリアルタイムに共有されます。
        /// </summary>
        public SearchSettings Settings { get; } = new();

        /// <summary>
        /// 現在アクティブ（画面に表示中）な子ViewModelを保持します。
        /// .NET 10.0推奨の部分プロパティ(partial property)方式を採用しています。
        /// WPFのMainWindow.xamlにあるContentControlが、このプロパティの変更を検知してViewを自動切り替えします。
        /// </summary>
        [ObservableProperty]
        public partial ObservableObject? CurrentViewModel { get; set; }

        // 各子画面のViewModelインスタンス（無駄なメモリ消費や再生成を防ぐため、必要になるまで生成しない遅延初期化を行います）
        private FinderViewModel? _finderViewModel;
        private SettingsViewModel? _settingsViewModel;

        /// <summary>
        /// コンストラクタ。アプリ起動時の初期画面として、最優先で「検索画面（Finder）」を表示するようにナビゲーションをキックします。
        /// </summary>
        public MainWindowViewModel()
        {
            NavigateToFinder();
        }

        /// <summary>
        /// コマンド：メインの検索画面へ遷移します。
        /// 既に一度インスタンスが生成されている場合はそれを使い回すため、画面の状態（ユーザーが入力を進めていたフォルダパスなど）がそのまま維持されます。
        /// </summary>
        [RelayCommand]
        public void NavigateToFinder()
        {
            _finderViewModel ??= new FinderViewModel(this);
            CurrentViewModel = _finderViewModel; // 💡内部の代入も大文字のプロパティ名に修正
        }

        /// <summary>
        /// コマンド：重複条件を細かくカスタムするための設定画面へ遷移します。
        /// </summary>
        [RelayCommand]
        public void NavigateToSettings()
        {
            _settingsViewModel ??= new SettingsViewModel(this);
            CurrentViewModel = _settingsViewModel; // 💡内部の代入も大文字のプロパティ名に修正
        }
    }
}
