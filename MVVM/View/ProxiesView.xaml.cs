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
                 if (App.accountsGroups.Count > 0)
                    _currentGroup = App.proxyGroups.First();
        }
        private void ListProxies_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            if (currentProxies > ListTokens.Items.Count)
                currentProxies = ListTokens.Items.Count;
            foreach (object item in ListTokens.Items)
            {
                currentProxies += 1;
            }
            ProxyCounter.Content = "Proxies: " + currentProxies.ToString();
        }

        private void Load_Proxies_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGroup == null)
                return;

            var dialog = new CommonOpenFileDialog();
            dialog.Title = "Select proxies file";
            dialog.DefaultExtension = ".txt";
            dialog.AddToMostRecentlyUsedList = true;
            dialog.EnsureFileExists = true;
            dialog.EnsurePathExists = true;
            string path = "";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                path = dialog.FileName;
                if (path.EndsWith(".txt"))
                {
                    using (var reader = new StreamReader(path))
                    {
                        var line = reader.ReadLine();
                        while(line != null && line != "")
                        {
                            line = line.Trim(new char[] { '\n', '\t', '\r', ' ' });
                            var proxy_array = line.Split(':');
                            DiscordProxy proxy = null;
                            if(proxy_array.Length > 2)
                            {
                                proxy = new DiscordProxy(proxy_array[0], int.Parse(proxy_array[1]), proxy_array[2], proxy_array[3]);
                            }
                            else
                            {
                                proxy = new DiscordProxy(proxy_array[0], int.Parse(proxy_array[1]));
                            }

                        }
                    }
                }
            }
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
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (_currentGroup == null)
                        return;
                    _currentGroup.Delete();
                    var source = new string[] { };
                    foreach (var group in App.proxyGroups)
                    {
                        source = source.Append(group._name).ToArray();
                    }
                    GroupComboBox.ItemsSource = source;
                    break;
                }
                catch(Exception ex)
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
