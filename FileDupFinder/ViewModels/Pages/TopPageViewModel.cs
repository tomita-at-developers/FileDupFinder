using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileDupFinder.ViewModels.Pages
{
    internal partial class TopPageViewModel : PageViewModelBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TopPageViewModel()
        {
            PageTitle = "ページ1";
        }

        // ソースジェネレーターが「SelectedFolderPath」プロパティを自動生成します
        [ObservableProperty]
        private string _selectedFolderPath = string.Empty;

        // ソースジェネレーターが「SelectFolderCommand」を自動生成します
        [RelayCommand]
        private void SelectFolder()
        {
            // .NET標準のモダンなフォルダ選択ダイアログ
            var dialog = new OpenFolderDialog
            {
                Title = "フォルダを選択してください",
                InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
            };

            // ダイアログを表示し、ユーザーがフォルダを選択した場合
            if (dialog.ShowDialog() == true)
            {
                SelectedFolderPath = dialog.FolderName;
            }
        }
    }
}
