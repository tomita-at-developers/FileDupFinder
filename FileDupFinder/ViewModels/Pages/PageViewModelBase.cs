using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileDupFinder.ViewModels.Pages
{
    internal class PageViewModelBase : ObservableObject
    {
        /// <summary>
        /// ページタイトルフィールド
        /// </summary>
        private string _pageTitle = string.Empty;

        /// <summary>
        /// ページタイトルプロパティ
        /// </summary>
        /// <remarks>
        /// CommunityToolkitのSetProperty()を使用してINotifyPropertyChangedの実装を簡素化
        /// </remarks>
        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

    }
}
