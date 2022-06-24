﻿// Copyright (c) Richasy. All rights reserved.

using System;
using System.ComponentModel;
using System.Linq;
using Bili.App.Pages.Desktop;
using Bili.App.Resources.Extension;
using Bili.Models.App.Args;
using Bili.Models.Data.Local;
using Bili.Models.Enums;
using Bili.ViewModels.Uwp.Account;
using Bili.ViewModels.Uwp.Core;
using ReactiveUI;
using Splat;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace Bili.App.Controls
{
    /// <summary>
    /// 桌面平台的主视图导航框架.
    /// </summary>
    public sealed partial class DesktopNavigationView : DesktopNavigationViewBase
    {
        private readonly AppViewModel _appViewModel;
        private readonly AccountViewModel _accountViewModel;
        private bool _isFirstLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopNavigationView"/> class.
        /// </summary>
        public DesktopNavigationView()
        {
            InitializeComponent();
            ViewModel = Locator.Current.GetService<NavigationViewModel>();
            _appViewModel = Locator.Current.GetService<AppViewModel>();
            _accountViewModel = Locator.Current.GetService<AccountViewModel>();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>
        /// 在刚加载首页时发生.
        /// </summary>
        public event EventHandler FirstLoaded;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Navigating += OnNavigating;
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;

            if (_appViewModel.IsXbox)
            {
                RootNavView.PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Navigating -= OnNavigating;
            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        private void OnNavigating(object sender, AppNavigationEventArgs e)
        {
            if (e.Type == Models.Enums.App.NavigationType.Main)
            {
                NavigateToMainView(e.PageId);
            }

            CheckMenuButtonVisibility();
        }

        private void OnRootNavViewItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ViewModel.NavigateToMainView(PageIds.Settings);
            }
            else
            {
                var pageId = NavigationExtension.GetPageId(args.InvokedItemContainer);
                ViewModel.Navigate(pageId);
            }
        }

        private void NavigateToMainView(PageIds pageId, object parameter = null)
        {
            Type pageType = null;
            switch (pageId)
            {
                case PageIds.None:
                default:
                    break;
                case PageIds.Recommend:
                    pageType = typeof(RecommendPage);
                    break;
                case PageIds.Rank:
                    pageType = typeof(RankPage);
                    break;
                case PageIds.VideoPartition:
                    pageType = typeof(VideoPartitionPage);
                    break;
                case PageIds.Popular:
                    pageType = typeof(PopularPage);
                    break;
                case PageIds.SpecialColumn:
                    pageType = typeof(ArticlePartitionPage);
                    break;
                case PageIds.Bangumi:
                    pageType = typeof(BangumiPage);
                    break;
                case PageIds.DomesticAnime:
                    pageType = typeof(DomesticPage);
                    break;
                case PageIds.Movie:
                    pageType = typeof(MoviePage);
                    break;
                case PageIds.TV:
                    pageType = typeof(TvPage);
                    break;
                case PageIds.Documentary:
                    pageType = typeof(DocumentaryPage);
                    break;
                case PageIds.Live:
                    pageType = typeof(LiveFeedPage);
                    break;
                case PageIds.Dynamic:
                    pageType = typeof(DynamicPage);
                    break;
                case PageIds.Help:
                    pageType = typeof(HelpPage);
                    break;
                case PageIds.Toolbox:
                    pageType = typeof(ToolboxPage);
                    break;
                case PageIds.Settings:
                    pageType = typeof(SettingsPage);
                    break;
            }

            if (pageType != null)
            {
                MainFrame.Navigate(pageType, parameter, new DrillInNavigationTransitionInfo());
            }

            if (RootNavView.SelectedItem == null ||
                (RootNavView.SelectedItem is Microsoft.UI.Xaml.Controls.NavigationViewItem navItem &&
                NavigationExtension.GetPageId(navItem) != pageId))
            {
                var shouldSelectedItem = pageId == PageIds.Settings
                    ? RootNavView.SettingsItem as Microsoft.UI.Xaml.Controls.NavigationViewItem
                    : RootNavView.MenuItems.Concat(RootNavView.FooterMenuItems).OfType<Microsoft.UI.Xaml.Controls.NavigationViewItem>()
                                           .Where(p => NavigationExtension.GetPageId(p) == pageId).FirstOrDefault();

                RootNavView.SelectedItem = shouldSelectedItem;
            }

            if (RootNavView.SelectedItem != null && RootNavView.SelectedItem is Microsoft.UI.Xaml.Controls.NavigationViewItem selectItem)
            {
                _appViewModel.HeaderText = selectItem.Content.ToString();
            }
        }

        private void OnFixedItemClick(object sender, RoutedEventArgs e)
        {
            var context = (sender as FrameworkElement).DataContext as Models.Data.Local.FixedItem;
            PlaySnapshot playRecord = null;
            switch (context.Type)
            {
                case Models.Enums.App.FixedType.Publisher:
                    new UserSpaceView().Show(context.Id);
                    break;
                case Models.Enums.App.FixedType.Pgc:
                    playRecord = new PlaySnapshot(default, context.Id, VideoType.Pgc)
                    {
                        Title = context.Title,
                    };
                    break;
                case Models.Enums.App.FixedType.Video:
                    playRecord = new PlaySnapshot(context.Id, default, VideoType.Video);
                    break;

                case Models.Enums.App.FixedType.Live:
                    playRecord = new PlaySnapshot(context.Id, default, VideoType.Live);
                    break;
                default:
                    break;
            }

            if (playRecord != null)
            {
                ViewModel.NavigateToPlayView(playRecord);
            }
        }

        private void OnFrameLoaded(object sender, RoutedEventArgs e)
        {
            if (!_isFirstLoaded)
            {
                FirstLoaded?.Invoke(this, EventArgs.Empty);
                _isFirstLoaded = true;
            }
        }

        private void OnDisplayModeChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewDisplayModeChangedEventArgs args)
            => CheckMenuButtonVisibility();

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.IsMainViewShown))
            {
                CheckMenuButtonVisibility();
            }
        }

        private void CheckMenuButtonVisibility()
        {
            _appViewModel.IsShowMenuButton = RootNavView.DisplayMode != Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Expanded
                && ViewModel.IsMainViewShown;
        }
    }

    /// <summary>
    /// <see cref="DesktopNavigationView"/> 的基类.
    /// </summary>
    public class DesktopNavigationViewBase : ReactiveUserControl<NavigationViewModel>
    {
    }
}
