using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDupFinder.ViewModels.Pages;

namespace FileDupFinder.ViewModels
{
    internal class MainWindowViewModel : ObservableObject
    {
        /// <summary>
        /// 現在ページフィールド
        /// </summary>
        private PageViewModelBase _currentPage = default!;  //警告抑止のためだけにdefault!を使用。実際にはコンストラクタで必ず初期化される。

        /// <summary>
        /// 現在ページ
        /// </summary>
        public PageViewModelBase CurrentPage
        {
            get => _currentPage;
            set
            {
                SetProperty(ref _currentPage, value);
            }
        }

        /// <summary>
        /// 最初のページVM
        /// </summary>
        public TopPageViewModel TopPage { get; }

        /// <summary>
        /// ページ管理リスト
        /// </summary>
        private List<PageViewModelBase> _pages;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            // ページVM初期化（ページの並び順と同じ順番で追加すること）
            _pages = new List<PageViewModelBase>();
            _pages.Add(TopPage = new TopPageViewModel());
            // 現在ページ設定
            //CurrentPage = StartPage;
            CurrentPage = _pages.First();

        }

    }
}
