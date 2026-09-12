using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDupFinder.Models;

namespace FileDupFinder.ViewModels.Pages
{
    /// <summary>
    /// 重複検索の条件設定を行う画面に対応するViewModelです。
    /// 親ウィンドウが管理する共通の SearchSettings インスタンスの値をUIへ直接公開・バインドし、
    /// 直接設定値をアップデートさせる構造を採用しています。
    /// </summary>
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _mainParent;

        public SearchSettings Settings => _mainParent.Settings;
        public IRelayCommand BackToFinderCommand => _mainParent.NavigateToFinderCommand;

        public bool IsExtensionFilterEnabled
        {
            get => Settings.IsExtensionFilterEnabled;
            set { Settings.IsExtensionFilterEnabled = value; OnPropertyChanged(); }
        }

        public string TargetExtensions
        {
            get => Settings.TargetExtensions;
            set { Settings.TargetExtensions = value; OnPropertyChanged(); }
        }

        public bool CompareFileName
        {
            get => Settings.CompareFileName;
            set { Settings.CompareFileName = value; OnPropertyChanged(); }
        }

        public bool CompareTimestamp
        {
            get => Settings.CompareTimestamp;
            set { Settings.CompareTimestamp = value; OnPropertyChanged(); }
        }

        public bool CompareHash
        {
            get => Settings.CompareHash;
            set { Settings.CompareHash = value; OnPropertyChanged(); }
        }

        public SettingsViewModel(MainWindowViewModel mainParent)
        {
            _mainParent = mainParent;
        }
    }
}
