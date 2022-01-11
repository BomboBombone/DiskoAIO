using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;

namespace DiskoAIO.MVVM.View
{
    /// <summary>
    /// Interaction logic for AccountsView.xaml
    /// </summary>
    public partial class AccountsView : UserControl
    {
        public int currentTokens { get; set; } = 0;
        public AccountGroup _currentGroup { get; set; } = null;
        public AccountsView()
        {
            InitializeComponent();
            var source = new string[] { };
            foreach (var group in App.accountsGroups)
            {
                source = source.Append(group._name).ToArray();
            }
            GroupComboBox.ItemsSource = source;
            if (_currentGroup != null)
                GroupComboBox.SelectedItem = _currentGroup._name;
            else
                 if (App.accountsGroups.Count > 0)
                    _currentGroup = App.accountsGroups.First();
            if (_currentGroup != null)
            {
                GroupComboBox.SelectedItem = _currentGroup._name;

                ListTokens.ItemsSource = _currentGroup._accounts;
            }
            UpdateAccountCount();
        }
        private void ListTokens_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            if (currentTokens > ListTokens.Items.Count)
                currentTokens = ListTokens.Items.Count;
            foreach (object item in ListTokens.Items)
            {
                currentTokens += 1;
            }
            TokenCounter.Content = "Accounts: " + currentTokens.ToString();

        }
        private void Add_Group_Click(object sender, RoutedEventArgs e)
        {
            var inputName = new InputPopupView("Choose a name for the account group", 16);
            inputName.ShowDialog();
            var accountGroup = new AccountGroup(null, inputName.answer);
            if (accountGroup == null || accountGroup._name == null)
            {
                App.mainWindow.ShowNotification("Invalid group name selected");
                return;
            }
            App.accountsGroups.Add(accountGroup);
            _currentGroup = accountGroup;
            var source = new string[] { };
            foreach (var group in App.accountsGroups)
            {
                source = source.Append(group._name).ToArray();
            }
            GroupComboBox.ItemsSource = source;
            GroupComboBox.SelectedIndex = source.Length - 1;
            UpdateAccountCount();

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
            if (App.accountsGroups.Count > 0)
                _currentGroup = App.accountsGroups.First();
            else
                _currentGroup = null;

            ListTokens.ItemsSource = new List<DiscordToken>();
            ListTokens.Items.Refresh();

            var source = new string[] { };
            foreach (var group in App.accountsGroups)
            {
                source = source.Append(group._name).ToArray();
            }
            GroupComboBox.ItemsSource = source;
            if(_currentGroup != null)
                GroupComboBox.SelectedItem = _currentGroup._name == null ? "" : _currentGroup._name;
            GroupComboBox.Items.Refresh();
            UpdateAccountCount();
        }
        private void UpdateAccountCount()
        {
            if (_currentGroup == null)
                TokenCounter.Content = "Accounts: 0";
            else
            {
                TokenCounter.Content = "Accounts: " + _currentGroup._accounts.Count.ToString();
            }
        }
        private void Load_Tokens_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGroup == null)
            {
                App.mainWindow.ShowNotification("Please select a group before loading your accounts");
                return;
            }
            Task.Run(() => {
                var dialog = new CommonOpenFileDialog();
                dialog.Title = "Select tokens file";
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
                    var tokens = new List<DiscordToken>();

                    path = dialog.FileName;
                    if (path.EndsWith(".txt"))
                    {
                        Dispatcher.Invoke(() => {
                            App.mainWindow.ShowNotification("Adding tokens, please wait...", 1000);
                        });
                        using (var reader = new StreamReader(path))
                        {
                            var line = reader.ReadLine();
                            while (line != null && line != "")
                            {
                                try
                                {
                                    line = line.Trim(new char[] { '\n', '\t', '\r', ' ' });
                                    var token_array = line.Split(':');
                                    var token = DiscordToken.Load(token_array);
                                    line = reader.ReadLine();
                                    tokens.Add(token);
                                    Dispatcher.Invoke(() =>
                                    {
                                        ListTokens.ItemsSource = tokens;
                                        ListTokens.Items.Refresh();
                                        UpdateAccountCount();
                                    });
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
                        _currentGroup.Append(tokens);
                        Dispatcher.Invoke(() =>
                        {
                            ListTokens.ItemsSource = _currentGroup._accounts;
                            ListTokens.Items.Refresh();
                            App.mainWindow.ShowNotification("Tokens added successfully: " + tokens.Count.ToString());
                            UpdateAccountCount();
                        });
                        while (true)
                        {
                            try
                            {
                                using (var writer = new StreamWriter(App.strWorkPath + "\\groups\\" + _currentGroup._name + ".txt"))
                                {
                                    foreach (var token in tokens)
                                    {
                                        writer.WriteLine(token.ToString());
                                    }
                                }
                                break;
                            }
                            catch (Exception ex)
                            {
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }
            });
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
                        using (var writer = new StreamWriter(App.strWorkPath + "\\groups\\" + _currentGroup._name + ".txt"))
                        {
                            foreach (var proxy in _currentGroup._accounts)
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
                    App.mainWindow.ShowNotification("Successfully saved your current accounts");
                });
            });
        }

        private void Note_Double_Click(object sender, MouseButtonEventArgs e)
        {
            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTokens.ItemContainerGenerator.IndexFromContainer(lbItem);
            var input = new InputPopupView("Add your note here", 64, false, _currentGroup._accounts[index].Note);
            input.ShowDialog();
            if (input.answer == null)
            {
                App.mainWindow.ShowNotification("Invalid note, please try again", 1000);
                return;
            }
            if (input.answer == "")
                input.answer = "Double tap to add note...";

            _currentGroup._accounts[index].Note = input.answer;
            App.mainWindow.ShowNotification("Successfully saved note");
            ListTokens.Items.Refresh();
        }

        private void Note_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputPopupView("Add your note here", 64);
            input.ShowDialog();
            if(input.answer == null)
            {
                App.mainWindow.ShowNotification("Invalid note, please try again", 1000);
                return;
            }
            if (input.answer == "")
                input.answer = "Double tap to add note...";
            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTokens.ItemContainerGenerator.IndexFromContainer(lbItem);

            _currentGroup._accounts[index].Note = input.answer;
            App.mainWindow.ShowNotification("Successfully saved note");
            ListTokens.Items.Refresh();
        }
        private void DeleteToken_Click(object sender, RoutedEventArgs e)
        {
            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTokens.ItemContainerGenerator.IndexFromContainer(lbItem);
            _currentGroup._accounts.RemoveAt(index);
            ListTokens.ItemsSource = _currentGroup._accounts;
            ListTokens.Items.Refresh();
            UpdateAccountCount();
        }
    }
}
