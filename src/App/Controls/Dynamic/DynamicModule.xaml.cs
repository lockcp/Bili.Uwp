﻿// Copyright (c) Richasy. All rights reserved.

using Bili.ViewModels.Interfaces;
using Bili.ViewModels.Interfaces.Core;
using Splat;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Bili.App.Controls.Dynamic
{
    /// <summary>
    /// 动态模块.
    /// </summary>
    public sealed partial class DynamicModule : UserControl
    {
        /// <summary>
        /// <see cref="ViewModel"/> 的依赖属性.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(IDynamicModuleViewModel), typeof(DynamicModule), new PropertyMetadata(default));

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicModule"/> class.
        /// </summary>
        public DynamicModule() => InitializeComponent();

        /// <summary>
        /// 核心视图模型.
        /// </summary>
        public IAppViewModel CoreViewModel { get; } = Locator.Current.GetService<IAppViewModel>();

        /// <summary>
        /// 视图模型.
        /// </summary>
        public IDynamicModuleViewModel ViewModel
        {
            get { return (IDynamicModuleViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
