using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sql;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.ConnectServer.ViewModels
{
    public partial class ServerViewModel : ObservableObject
    {
      
        [ObservableProperty]
        private ObservableCollection<string> serverNames = new();

        [ObservableProperty]
        private string selectedServer;

        private bool _serversLoaded = false;

        [ObservableProperty]
        private bool isLoading = false;

      
        public ObservableCollection<string> ServerNamesLazy
        {
            get
            {
                if (!_serversLoaded)
                {
                    LoadServersCommand.Execute(null);
                    _serversLoaded = true;

                }
                return ServerNames;
            }
        }

      
        [RelayCommand]
        private async Task LoadServersAsync()
        {
            IsLoading = true;
            ServerNames.Clear();

            try
            {
                await Task.Run(() =>
                {
                    SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
                    DataTable servers = instance.GetDataSources();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (DataRow row in servers.Rows)
                        {
                            string? server = row["ServerName"]?.ToString();
                            string? instanceName = row["InstanceName"]?.ToString();

                            if (!string.IsNullOrEmpty(server))
                            {
                                string fullName = string.IsNullOrEmpty(instanceName)
                                    ? server
                                    : $"{server}\\{instanceName}";

                                ServerNames.Add(fullName);
                            }
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل السيرفرات:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
