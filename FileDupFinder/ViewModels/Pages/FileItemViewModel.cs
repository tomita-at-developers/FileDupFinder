using System;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDupFinder.Models;

namespace FileDupFinder.ViewModels.Pages
{
    /// <summary>
    /// 重複して見つかったファイルリストの「1行分の要素（UserControl）」に対応する独立したViewModelです。
    /// </summary>
    public partial class FileItemViewModel : ObservableObject
    {
        public event EventHandler? Deleted;
        public FileInfoItem Model { get; }

        public string FileName => Model.FileName;
        public string FolderPath => Model.FolderPath;
        public string DisplaySize => Model.DisplaySize;
        public string Hash => Model.Hash;
        public string CreationTimeDisplay => Model.CreationTime.ToString("yyyy/MM/dd HH:mm:ss");
        public string LastWriteTimeDisplay => Model.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss");

        public FileItemViewModel(FileInfoItem model)
        {
            Model = model;
        }

        /// <summary>
        /// コマンド：Windowsのエクスプローラーを起動し、該当するファイルを「選択した状態」で開きます。
        /// </summary>
        [RelayCommand]
        private void OpenInExplorer()
        {
            if (File.Exists(Model.FilePath))
            {
                Process.Start("explorer.exe", $"/select,\"{Model.FilePath}\"");
            }
        }

        /// <summary>
        /// コマンド：この要素が指し示しているストレージ上のファイルを物理的に完全消去します。
        /// </summary>
        [RelayCommand]
        private void DeleteFile()
        {
            try
            {
                if (File.Exists(Model.FilePath))
                {
                    File.Delete(Model.FilePath);
                    Deleted?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"削除失敗: {ex.Message}");
            }
        }
    }
}
