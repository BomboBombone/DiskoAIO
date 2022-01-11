using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace DiskoAIO.MVVM.View
{
    /// <summary>
    /// Interaction logic for ProxiesView.xaml
    /// </summary>
    public partial class ProxiesView : UserControl
    {
        public int currentProxies { get; set; } = 0;
        public ProxyGroup _currentGroup { get; set; } = null;
        public ProxiesView()
        {
            InitializeComponent();
            var source = new string[] { };
            foreach(var group in App.proxyGroups)
            {
                source = source.Append(group._name).ToArray();
            }
            GroupComboBox.ItemsSource = source;
            if(_currentGroup != null)
                GroupComboBox.SelectedItem = _currentGroup._name;
            else
                 if (App.proxyGroups.Count > 0)
                    _currentGroup = App.proxyGroups.First();
            
            if(_currentGroup != null)
            {
                GroupComboBox.SelectedItem = _currentGroup._name;

                ListProxies.ItemsSource = _currentGroup._proxies;
            }
        }
        private void ListProxies_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            if (currentProxies > ListProxies.Items.Count)
                currentProxies = ListProxies.Items.Count;
            foreach (object item in ListProxies.Items)
            {
                currentProxies += 1;
            }
            ProxyCounter.Content = "Proxies: " + currentProxies.ToString();
        }

        private void Load_Proxies_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGroup == null)
            {
                App.mainWindow.ShowNotification("Please select a group before loading your proxies");
                return;
            }

            Task.Run(() => {
                var dialog = new CommonOpenFileDialog();
                dialog.Title = "Select proxies file";
                dialog.DefaultExtension = ".txt";
                dialog.AddToMostRecentlyUsedList = true;
                dialog.EnsureFileExists = true;
                dialog.EnsurePathExists = true;
                string path = "";
                List<DiscordProxy> proxies = new List<DiscordProxy>();
                var result = CommonFileDialogResult.Ok;
                Dispatcher.Invoke(() => result = dialog.ShowDialog());
                if (result == CommonFileDialogResult.Ok)
                {
                    path = dialog.FileName;
                    if (path.EndsWith(".txt"))
                    {
                        Dispatcher.Invoke(() => {
                            App.mainWindow.ShowNotification("Adding proxies, please wait...", 1000);
                        });
                        using (var reader = new StreamReader(path))
                        {
                            var line = reader.ReadLine();
                            while (line != null && line != "")
                            {
                                try
                                {
                                    line = line.Trim(new char[] { '\n', '\t', '\r', ' ' });
                                    var proxy_array = line.Split(':');
                                    DiscordProxy proxy = null;
                                    if (proxy_array.Length > 2)
                                    {
                                        if (int.TryParse(proxy_array[1], out var port))
                                            proxy = new DiscordProxy(proxy_array[0], port, proxy_array[2], proxy_array[3]);
                                        else
                                            proxy = new DiscordProxy(proxy_array[2], int.Parse(proxy_array[3]), proxy_array[1], proxy_array[2]);
                                    }
                                    else
                                    {
                                        proxy = new DiscordProxy(proxy_array[0], int.Parse(proxy_array[1]));
                                    }
                                    line = reader.ReadLine();
                                    proxies.Add(proxy);
                                }
                                catch (Exception ex)
                                {
                                    Dispatcher.Invoke(() => { 
                                        App.mainWindow.ShowNotification("Format of selected proxies seems to be wrong.\nHint: {host}:{port}:{username}:{password}");
                                    });
                                    return;
                                }
                            }
                        }
                        _currentGroup.Append(proxies);
                        Dispatcher.Invoke(() =>
                        {
                            ListProxies.ItemsSource = _currentGroup._proxies;
                            ListProxies.Items.Refresh();
                            App.mainWindow.ShowNotification("Proxies added successfully: " + proxies.Count.ToString());
                            UpdateProxyCount();
                        });
                        while (true)
                        {
                            try
                            {
                                using (var writer = new StreamWriter(App.strWorkPath + "\\proxies\\" + _currentGroup._name + ".txt"))
                                {
                                    foreach (var proxy in proxies)
                                    {
                                        writer.WriteLine(proxy.ToString());
                                    }
                                }
                                break;
                            }
                            catch(Exception ex)
                            {
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }
            });
        }

        private void Add_Group_Click(object sender, RoutedEventArgs e)
        {
            var inputName = new InputPopupView("Choose a name for the proxy group", 16);
            inputName.ShowDialog();
            var proxyGroup = new ProxyGroup(null, inputName.answer);
            if (proxyGroup == null && proxyGroup._name != "")
                return;
            App.proxyGroups.Add(proxyGroup);
            _currentGroup = proxyGroup;
            var source = new string[] { };
            foreach (var group in App.proxyGroups)
            {
                source = source.Append(group._name).ToArray();
            }
            GroupComboBox.ItemsSource = source;
            GroupComboBox.SelectedIndex = source.Length - 1;
            UpdateProxyCount();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGroup == null)
            {
                App.mainWindow.ShowNotification("No group is selected for deletion");
                return;
            }
            var popup = new WarningPopupView("You are about to delete group: " + _currentGroup._name + "\nAre you sure you want to continue?");
            popup.ShowDialog();
            if (!popup.hasConfirmed)
                return;
            var result = _currentGroup.Delete();
            if (result > 0)
                return;
            if (App.proxyGroups.Count > 0)
                _currentGroup = App.proxyGroups.First();
            else
                _currentGroup = null;

            var source = new string[] { };
            foreach (var group in App.proxyGroups)
            {
                source = source.Append(group._name).ToArray();
            }
            GroupComboBox.ItemsSource = source;
            if (_currentGroup != null)
                GroupComboBox.SelectedItem = _currentGroup._name == null ? "" : _currentGroup._name;
            GroupComboBox.Items.Refresh();
            UpdateProxyCount();
        }

        private void DeleteProxy_Click(object sender, RoutedEventArgs e)
        {
            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListProxies.ItemContainerGenerator.IndexFromContainer(lbItem);
            _currentGroup._proxies.RemoveAt(index);
            ListProxies.ItemsSource = _currentGroup._proxies;
            ListProxies.Items.Refresh();
            UpdateProxyCount();
        }

        private void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _currentGroup = App.proxyGroups.Where(o => o._name == GroupComboBox.SelectedItem.ToString()).First();
                ListProxies.ItemsSource = _currentGroup._proxies;
            }
            catch (Exception ex)
            {
                _currentGroup = null;
                ListProxies.ItemsSource = new List<DiscordProxy>();
            }
            ListProxies.Items.Refresh();
            UpdateProxyCount();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGroup == null)
                return;
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        using (var writer = new StreamWriter(App.strWorkPath + "\\proxies\\" + _currentGroup._name + ".txt"))
                        {
                            foreach (var proxy in _currentGroup._proxies)
                            {
                                writer.WriteLine(proxy.ToString());
                            }
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        App.mainWindow.ShowNotification("Resource busy... waiting to save", 1000);

                        Thread.Sleep(1000);
                    }
                }
                Dispatcher.Invoke(() =>
                {
                    App.mainWindow.ShowNotification("Successfully saved your current proxies");
                });
            });
        }
        private void UpdateProxyCount()
        {
            if (_currentGroup == null)
                ProxyCounter.Content = "Proxies: 0";
            else
            {
                ProxyCounter.Content = "Proxies: " + _currentGroup._proxies.Count.ToString();
            }
        }
    }
}
