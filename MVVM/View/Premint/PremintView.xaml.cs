using DiskoAIO.DiskoTasks;
using DiskoAIO.Premint;
using DiskoAIO.Twitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for PremintView.xaml
    /// </summary>
    public partial class PremintView : UserControl
    {
        public static List<string> types = new List<string>() { "Subscribe" , "Connect Discord", "Connect Twitter"};
        public PremintView()
        {
            InitializeComponent();
            Type.ItemsSource = types;
            Type.SelectedItem = types.First();
        }

        private void UseProxies_Click(object sender, RoutedEventArgs e)
        {
            ProxiesGroup.Visibility = (bool)UseProxies.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            ProxiesLabel.Visibility = (bool)UseProxies.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Start_Task(object sender, RoutedEventArgs e)
        {
            PremintAccountGroup accounts = null;
            foreach (var group in App.premintGroups)
            {
                if (group._name == TokenGroup.SelectedItem.ToString())
                {
                    accounts = group;
                    break;
                }
            }
            if (accounts == null)
            {
                App.mainWindow.ShowNotification("Couldn't get specified account group, try again later");
                return;
            }
            if (Type.SelectedItem.ToString() != "Subscribe")
            {
                if(BindGroup.SelectedItem == null)
                {
                    App.mainWindow.ShowNotification("Please use a valid group to bind");
                    return;
                }
                if(Type.SelectedItem.ToString() == "Connect Discord")
                {
                    AccountGroup accounts_discord = null;
                    foreach (var group in App.accountsGroups)
                    {
                        if (group._name == BindGroup.SelectedItem.ToString())
                        {
                            accounts_discord = group;
                            break;
                        }
                    }
                    if (accounts_discord == null)
                    {
                        App.mainWindow.ShowNotification("Couldn't get specified account group to bind, try again later");
                        return;
                    }

                    var task = new PremintBindDiscordTask(accounts, accounts_discord);
                    App.taskManager.AddTask(task);
                    task.Start();
                    App.mainWindow.ShowNotification("Task started successfully");

                }
                else
                {
                    TwitterAccountGroup accounts_twitter = null;
                    foreach (var group in App.twitterGroups)
                    {
                        if (group._name == BindGroup.SelectedItem.ToString())
                        {
                            accounts_twitter = group;
                            break;
                        }
                    }
                    if (accounts_twitter == null)
                    {
                        App.mainWindow.ShowNotification("Couldn't get specified account group to bind, try again later");
                        return;
                    }
                    var task = new PremintBindTwitterTask(accounts, accounts_twitter);
                    App.taskManager.AddTask(task);
                    task.Start();
                    App.mainWindow.ShowNotification("Task started successfully");
                }
            }
            else
            {
                if(ProjectLink.Text == "")
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        App.mainWindow.ShowNotification("Please use a valid project name");
                    });
                    return;
                }

                var task = new PremintTask(accounts, ProjectLink.Text.StartsWith("https://www.premint.xyz") ? ProjectLink.Text.Trim('/').Split('/').Last() : ProjectLink.Text);
                App.taskManager.AddTask(task);
                task.Start();
                App.mainWindow.ShowNotification("Task started successfully");

            }
        }

        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ulong result;
            if (!ulong.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
        }

        private void Type_DropDownClosed(object sender, EventArgs e)
        {
            if(Type.SelectedItem.ToString() != "Subscribe")
            {
                PremintLinkBorder.Visibility = Visibility.Collapsed;
                BindGroupLabel.Visibility = Visibility.Visible;
                BindGroup.Visibility = Visibility.Visible;
                var source = new List<string>();
                if(Type.SelectedItem.ToString() == "Connect Discord")
                {
                    foreach(var group in App.accountsGroups)
                    {
                        source.Add(group._name);
                    }
                    BindGroup.ItemsSource = source;
                }
                else
                {
                    foreach (var group in App.twitterGroups)
                    {
                        source.Add(group._name);
                    }
                    BindGroup.ItemsSource = source;
                }
            }
            else
            {
                PremintLinkBorder.Visibility = Visibility.Visible;
                BindGroupLabel.Visibility = Visibility.Collapsed;
                BindGroup.Visibility = Visibility.Collapsed;
            }
        }
    }
}
