using Discord.Gateway;
using DiskoAIO.Properties;
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
        public static string to_search { get; set; } = "";
        public static bool adding_accounts { get; set; } = false;
        public static int seconds_remaining { get; set; } = 0;

        public AccountsView()
        {
            InitializeComponent();
            if (App.accountsView == null)
                App.accountsView = this;
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
            GroupComboBox.SelectedItem = Settings.Default.TokenGroup;

            if (_currentGroup != null)
            {
                GroupComboBox.SelectedItem = _currentGroup._name;

                ListTokens.ItemsSource = _currentGroup._accounts;
            }
            UpdateAccountCount();
            if (seconds_remaining == 0)
                adding_accounts = false;
            if (adding_accounts)
            {
                App.mainWindow.ShowNotification($"Still adding tokens, estimated time:\n{TimeSpan.FromSeconds(seconds_remaining).ToString()}", 2000);
            }
            if (!App.IsConnectedToInternet())
            {
                App.mainWindow.ShowNotification("You are not connected to internet");
            }
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
            App.accountsView.ListTokens.Items.Refresh();
        }
        private void Add_Group_Click(object sender, RoutedEventArgs e)
        {
            if (adding_accounts)
            {
                App.mainWindow.ShowNotification("Cannot create group while adding tokens");
                return;
            }
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
            if (adding_accounts)
            {
                App.mainWindow.ShowNotification("Cannot delete group while adding tokens");
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
            if (adding_accounts)
            {
                App.mainWindow.ShowNotification("Another group is being filled with tokens, wait for it to finish");
                return;
            }
            if (!App.IsConnectedToInternet())
            {
                App.mainWindow.ShowNotification("You are not connected to internet");
                return;
            }
            Task.Run(() => {
                adding_accounts = true;
                var dialog = new CommonOpenFileDialog();
                dialog.Title = "Select tokens file";
                dialog.DefaultExtension = ".txt";
                dialog.AddToMostRecentlyUsedList = true;
                dialog.EnsureFileExists = true;
                dialog.EnsurePathExists = true;
                string path = "";
                var result = CommonFileDialogResult.Ok;
                Dispatcher.Invoke(() => result = dialog.ShowDialog());
                if (result == CommonFileDialogResult.Ok)
                {
                    var tokens = _currentGroup._accounts;
                    int start_count = tokens.Count;
                    path = dialog.FileName;
                    if (path.EndsWith(".txt"))
                    {
                        Dispatcher.Invoke(() => {
                            App.mainWindow.ShowNotification("Adding tokens, please wait...", 1000);
                        });
                        int lines = 0;
                        using (var reader = new StreamReader(path))
                        {
                            var line = reader.ReadLine();
                            while (line != null && line != "")
                            {
                                lines += 1;
                                line = reader.ReadLine();
                            }
                        }
                        var group_id = _currentGroup._id;
                        var group_name = _currentGroup._name;
                        var group_index = App.accountsGroups.IndexOf(_currentGroup);

                        seconds_remaining = (int)(lines / 3);
                        using (var reader = new StreamReader(path))
                        {
                            var line = reader.ReadLine();
                            while (line != null && line != "")
                            {
                                try
                                {
                                    line = line.Trim(new char[] { '\n', '\t', '\r', ' ' });
                                    var token_array = line.Split(':');
                                    DiscordToken token = null;
                                    var task = Task.Run(() =>
                                    {
                                        token = DiscordToken.Load(token_array);
                                    });
                                    if (task.Wait(TimeSpan.FromSeconds(3)))
                                        ;
                                    else
                                    {
                                        lines -= 1;
                                        seconds_remaining = (int)(lines / 3);
                                        line = reader.ReadLine();
                                        continue;
                                    }
                                    if (token == null)
                                    {
                                        {
                                            lines -= 1;
                                            seconds_remaining = (int)(lines / 3);
                                            line = reader.ReadLine();

                                            continue;
                                        }
                                    }
                                    if (tokens.Contains(token))
                                    {
                                        lines -= 1;
                                        seconds_remaining = (int)(lines / 3);

                                        continue;
                                    }
                                    line = reader.ReadLine();
                                    tokens.Add(token);

                                    lines -= 1;
                                    seconds_remaining = (int)(lines / 3);

                                    Dispatcher.Invoke(() =>
                                    {
                                        if(_currentGroup._name == group_name)
                                        {
                                            App.accountsView.ListTokens.ItemsSource = tokens;
                                            App.accountsView.ListTokens.Items.Refresh();
                                        }
                                        UpdateAccountCount();

                                    });
                                }
                                catch (Exception ex)
                                {
                                    Dispatcher.Invoke(() => {
                                        App.mainWindow.ShowNotification("Format of selected tokens seems to be wrong.\nHint: one token per line");
                                    });
                                    return;
                                }
                            }
                        }
                        Dispatcher.Invoke(() =>
                        {
                            ListTokens.ItemsSource = _currentGroup._accounts;
                            ListTokens.Items.Refresh();
                            App.mainWindow.ShowNotification("Tokens added successfully: " + (tokens.Count - start_count).ToString());
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
                                        if (token == null)
                                            continue;
                                        writer.WriteLine(token.ToString());
                                    }
                                }
                                break;
                            }
                            catch (Exception ex)
                            {
                                Thread.Sleep(100);
                            }
                        }
                        App.accountsGroups[group_index]._accounts = tokens;

                    }
                }

                adding_accounts = false;

            });
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGroup == null || adding_accounts)
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
            var source = new List<DiscordToken>();
            foreach (var o in _currentGroup._accounts)
            {
                if (o.Note.ToLower().Contains(to_search.ToLower()) &&
                    o.Note != "Double click to add note..." &&
                    to_search != "")
                {
                    source.Add(o);
                }
                else if (o.User_id.Contains(to_search))
                    source.Add(o);
            }

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

            source[index].Note = input.answer;
            ListTokens.ItemsSource = source;
            App.mainWindow.ShowNotification("Successfully saved note");
            ListTokens.Items.Refresh();
            TokenCounter.Content = ListTokens.Items.Count.ToString() + " accounts";
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var source = new List<DiscordToken>();
            foreach (var o in _currentGroup._accounts)
            {
                if (o.Note.ToLower().Contains(to_search.ToLower()) &&
                    o.Note != "Double click to add note..." &&
                    to_search != "")
                {
                    source.Add(o);
                }
                else if (o.User_id.Contains(to_search))
                    source.Add(o);
            }

            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTokens.ItemContainerGenerator.IndexFromContainer(lbItem);

            var popup = new AccountSettingsPopup(GroupComboBox.SelectedItem.ToString(), source[index], index);
            popup.Show();
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

        private void Open_Browser(object sender, RoutedEventArgs e)
        {
            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTokens.ItemContainerGenerator.IndexFromContainer(lbItem);
            var source = new List<DiscordToken>();

            foreach (var o in _currentGroup._accounts)
            {
                if (o.Note.ToLower().Contains(to_search.ToLower()) &&
                    o.Note != "Double click to add note..." &&
                    to_search != "")
                {
                    source.Add(o);
                }
                else if (o.User_id.Contains(to_search))
                    source.Add(o);
            }
            new DiscordDriver(source[index]._token);
        }

        private void Search_Input(object sender, TextCompositionEventArgs e)
        {
            if(e.Text == "\r")
            {
                var source = new List<DiscordToken>();
                foreach(var o in _currentGroup._accounts)
                {
                    if (o.Note.ToLower().Contains(to_search.ToLower()) &&
                        o.Note != "Double click to add note..." &&
                        to_search != "")
                    {
                        source.Add(o);
                    }
                    else if (o.User_id.Contains(to_search))
                        source.Add(o);
                }

                ListTokens.ItemsSource = source;
                ListTokens.Items.Refresh();
                UpdateAccountCount();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            to_search = ((TextBox)e.OriginalSource).Text;
        }

        private void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(GroupComboBox.SelectedItem == null)
            {
                return;
            }
            _currentGroup = App.accountsGroups.Where(o => o._name == GroupComboBox.SelectedItem.ToString()).ToArray()[0];
            ListTokens.ItemsSource = _currentGroup._accounts;
            ListTokens.Items.Refresh();
            UpdateAccountCount();
        }

        private void Check_Tokens_Click(object sender, RoutedEventArgs e)
        {
            App.mainWindow.ShowNotification("Checking current group, invalid accounts will be deleted soon");
            var checkerTask = new AccountCheckerTask(_currentGroup);
            checkerTask.Start();
            App.taskManager.AddTask(checkerTask);
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (_currentGroup == null)
            {
                App.mainWindow.ShowNotification("Please select a group before loading your accounts");
                return;
            }
            if (adding_accounts)
            {
                App.mainWindow.ShowNotification("Another group is being filled with tokens, wait for it to finish");
                return;
            }
            if (!App.IsConnectedToInternet())
            {
                App.mainWindow.ShowNotification("You are not connected to internet");
                return;
            }
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                Task.Run(() =>
                {
                    foreach (var file in files)
                    {
                        if (!file.EndsWith(".txt"))
                            continue;

                        adding_accounts = true;
                        var result = CommonFileDialogResult.Ok;
                        if (result == CommonFileDialogResult.Ok)
                        {
                            var tokens = _currentGroup._accounts;
                            int start_count = tokens.Count;
                            if (file.EndsWith(".txt"))
                            {
                                Dispatcher.Invoke(() => {
                                    App.mainWindow.ShowNotification("Adding tokens, please wait...", 1000);
                                });
                                int lines = 0;
                                using (var reader = new StreamReader(file))
                                {
                                    var line = reader.ReadLine();
                                    while (line != null && line != "")
                                    {
                                        lines += 1;
                                        line = reader.ReadLine();
                                    }
                                }
                                var group_id = _currentGroup._id;
                                var group_name = _currentGroup._name;
                                var group_index = App.accountsGroups.IndexOf(_currentGroup);

                                seconds_remaining = (int)(lines / 3);
                                using (var reader = new StreamReader(file))
                                {
                                    var line = reader.ReadLine();
                                    while (line != null && line != "")
                                    {
                                        try
                                        {
                                            line = line.Trim(new char[] { '\n', '\t', '\r', ' ' });
                                            var token_array = line.Split(':');
                                            DiscordToken token = null;
                                            var task = Task.Run(() =>
                                            {
                                                token = DiscordToken.Load(token_array);
                                            });
                                            if (task.Wait(TimeSpan.FromSeconds(3)))
                                                ;
                                            else
                                            {
                                                lines -= 1;
                                                seconds_remaining = (int)(lines / 3);
                                                line = reader.ReadLine();
                                                continue;
                                            }
                                            if (token == null)
                                            {
                                                {
                                                    lines -= 1;
                                                    seconds_remaining = (int)(lines / 3);
                                                    line = reader.ReadLine();

                                                    continue;
                                                }
                                            }
                                            if (tokens.Contains(token))
                                            {
                                                lines -= 1;
                                                seconds_remaining = (int)(lines / 3);

                                                continue;
                                            }
                                            line = reader.ReadLine();
                                            tokens.Add(token);

                                            lines -= 1;
                                            seconds_remaining = (int)(lines / 3);

                                            Dispatcher.Invoke(() =>
                                            {
                                                if (_currentGroup._name == group_name)
                                                {
                                                    App.accountsView.ListTokens.ItemsSource = tokens;
                                                    App.accountsView.ListTokens.Items.Refresh();
                                                }
                                                UpdateAccountCount();

                                            });
                                        }
                                        catch (Exception ex)
                                        {
                                            Dispatcher.Invoke(() => {
                                                App.mainWindow.ShowNotification("Format of selected tokens seems to be wrong.\nHint: one token per line");
                                            });
                                            return;
                                        }
                                    }
                                }
                                Dispatcher.Invoke(() =>
                                {
                                    ListTokens.ItemsSource = _currentGroup._accounts;
                                    ListTokens.Items.Refresh();
                                    App.mainWindow.ShowNotification("Tokens added successfully: " + (tokens.Count - start_count).ToString());
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
                                                if (token == null)
                                                    continue;
                                                writer.WriteLine(token.ToString());
                                            }
                                        }
                                        break;
                                    }
                                    catch (Exception ex)
                                    {
                                        Thread.Sleep(100);
                                    }
                                }
                                App.accountsGroups[group_index]._accounts = tokens;

                            }
                        }

                    }
                    adding_accounts = false;
                });

            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        private void Change_Image_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.Title = "Select images folder";
            dialog.IsFolderPicker = true;
            dialog.AddToMostRecentlyUsedList = true;
            dialog.EnsureFileExists = true;
            dialog.EnsurePathExists = true;
            string path = "";
            var result = CommonFileDialogResult.Ok;
            Dispatcher.Invoke(() => result = dialog.ShowDialog());
            if (result == CommonFileDialogResult.Ok)
            {
                path = dialog.FileName;
                var changerTask = new ImageChangerTask(_currentGroup, path);
                changerTask.Start();
                App.taskManager.AddTask(changerTask);
                App.mainWindow.ShowNotification("Image changer task started successfully");
            }
        }
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTokens.ItemContainerGenerator.IndexFromContainer(lbItem);
            var source = new List<DiscordToken>();

            foreach (var o in _currentGroup._accounts)
            {
                if (o.Note.ToLower().Contains(to_search.ToLower()) &&
                    o.Note != "Double click to add note..." &&
                    to_search != "")
                {
                    source.Add(o);
                }
                else if (o.User_id.Contains(to_search))
                    source.Add(o);
            }
            Clipboard.SetDataObject(source[index].User_id);
            App.mainWindow.ShowNotification("Successfully copied Discord ID");
        }

        private void Clean_Click(object sender, RoutedEventArgs e)
        {
            var lbItem = App.FindParent<ListBoxItem>((DependencyObject)e.Source);
            var index = ListTokens.ItemContainerGenerator.IndexFromContainer(lbItem);
            var source = new List<DiscordToken>();

            foreach (var o in _currentGroup._accounts)
            {
                if (o.Note.ToLower().Contains(to_search.ToLower()) &&
                    o.Note != "Double click to add note..." &&
                    to_search != "")
                {
                    source.Add(o);
                }
                else if (o.User_id.Contains(to_search))
                    source.Add(o);
            }
            var popup = new WarningPopupView("Are you sure you want to leave all servers for account " + source[index].User_id + '?');
            popup.ShowDialog();
            if (popup.hasConfirmed)
                Task.Run(() =>
                {
                    try
                    {
                        var client = new DiscordSocketClient();
                        client.Login(source[index]._token);
                        while (!client.LoggedIn)
                            Thread.Sleep(10);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            App.mainWindow.ShowNotification("Started leaving all servers for " + client.User.Username);
                        });
                        var guilds = client.GetCachedGuilds();
                        int tries = 0;
                        while (guilds.Count == 0 && tries < 3)
                        {
                            Thread.Sleep(2000);
                            guilds = client.GetCachedGuilds();
                            tries++;
                        }
                        foreach (var guild in guilds)
                        {
                            try
                            {
                                guild.Leave();
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                        }
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            App.mainWindow.ShowNotification("All servers have been wiped for " + client.User.Username);
                        });
                    }
                    catch (Exception ex1)
                    {
                        Debug.Log(ex1.StackTrace);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            App.mainWindow.ShowNotification("There was an error wiping servers, please check that the account is not locked");
                        });
                    }
                });
        }
    }
}
