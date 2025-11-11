using Microsoft.Web.WebView2.Core;
using Next_Future_ERP.Features.PrintManagement.Models;
using Next_Future_ERP.Features.PrintManagement.ViewModels;
using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Next_Future_ERP.Features.PrintManagement.Views
{
    /// <summary>
    /// Interaction logic for TemplateWorkspaceView.xaml
    /// </summary>
    public partial class TemplateWorkspaceView : Page
    {
        public TemplateWorkspaceView(TemplateWorkspaceViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            Loaded += TemplateWorkspaceView_Loaded;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        /// <summary>
        /// تحميل قالب معين في مساحة العمل
        /// </summary>
        public async void LoadTemplate(int templateId)
        {
            if (DataContext is TemplateWorkspaceViewModel viewModel)
            {
                await viewModel.LoadTemplateCommand.ExecuteAsync(templateId);
            }
        }

        /// <summary>
        /// إنشاء قالب جديد في مساحة العمل
        /// </summary>
        public async void CreateNewTemplate(TemplateInfo templateInfo)
        {
            if (DataContext is TemplateWorkspaceViewModel viewModel)
            {
                await viewModel.CreateNewTemplateCommand.ExecuteAsync(templateInfo);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private async void TemplateWorkspaceView_Loaded(object sender, RoutedEventArgs e)
        {
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Core", "Resources", "Editors", "grapes", "index.html");

            if (System.IO.File.Exists(path))
            {
                EditorWebView.Source = new Uri(path);
            }
        }

        private async void EditorWebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await SyncEditorAsync();
        }

        private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TemplateWorkspaceViewModel.HtmlContent) ||
                e.PropertyName == nameof(TemplateWorkspaceViewModel.CssContent))
            {
                await SyncEditorAsync();
            }
        }

        private async Task SyncEditorAsync()
        {
            if (EditorWebView?.CoreWebView2 == null) return;

            if (DataContext is TemplateWorkspaceViewModel vm)
            {
                var payload = JsonSerializer.Serialize(new { html = vm.HtmlContent, css = vm.CssContent });
                await EditorWebView.CoreWebView2.ExecuteScriptAsync($"window.__designer.setHtmlCss({payload})");
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditorWebView?.CoreWebView2 == null) return;

            if (DataContext is TemplateWorkspaceViewModel vm)
            {
                var json = await EditorWebView.CoreWebView2.ExecuteScriptAsync("window.__designer.getHtmlCss()");
                var inner = JsonSerializer.Deserialize<string>(json);
                if (!string.IsNullOrEmpty(inner))
                {
                    var data = JsonSerializer.Deserialize<HtmlCssData>(inner);
                    if (data != null)
                    {
                        vm.HtmlContent = data.html ?? string.Empty;
                        vm.CssContent = data.css ?? string.Empty;

                        await vm.SaveHtmlContentCommand.ExecuteAsync(null);
                        await vm.SaveCssContentCommand.ExecuteAsync(null);
                    }
                }
            }
        }

        private record HtmlCssData(string html, string css);

        private void AddDataSourceButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is TemplateWorkspaceViewModel vm)
            {
                vm.SelectedTabIndex = 2; // Data sources tab
            }
        }

        private void TestDataSourceButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("اختبار مصدر البيانات غير متوفر حالياً.",
                "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteDataSourceButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("حذف مصدر البيانات غير متوفر حالياً.",
                "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}