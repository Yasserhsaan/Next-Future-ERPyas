using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace Next_Future_ERP.Features.Dashboard.Models
{
    public partial class TabItem : ObservableObject
    {
        [ObservableProperty] private string title;
        [ObservableProperty] private bool isSelected;
        [ObservableProperty] private bool canClose = true;

        public Type PageType { get; }
        public System.Windows.FrameworkElement Content { get; set; }

        public TabItem(string title, Type pageType, System.Windows.FrameworkElement content, bool canClose = true)
        {
            this.title = title;
            PageType = pageType;
            Content = content;
            this.canClose = canClose;
        }
    }
}
