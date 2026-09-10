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
                // ページ戻る/進むコマンドの有効/無効状態変化イベント発行
                PageBackCommand?.NotifyCanExecuteChanged();
                PageNextCommand?.NotifyCanExecuteChanged();
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
        /// ページ戻るコマンド
        /// </summary>
        public IRelayCommand PageBackCommand { get; }

        /// <summary>
        /// ページ進むコマンド
        /// </summary>
        public IRelayCommand PageNextCommand { get; }

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

        // <summary>
        /// ページ戻るコマンドの処理
        /// </summary>
        private void PageBackExecute()
        {
            // ページ管理リストを使用して、現在ページの1つ前を現在ページに設定
            CurrentPage = _pages[_pages.FindIndex(x => x == CurrentPage) - 1];
        }

        /// <summary>
        /// ページ戻るコマンド実行可否を設定
        /// </summary>
        /// <returns></returns>
        private bool PageBackCanExecute()
        {
            // 現在ページが最初のページ以外ならコマンド実行可
            return CurrentPage != _pages.First();
        }

    }
}
