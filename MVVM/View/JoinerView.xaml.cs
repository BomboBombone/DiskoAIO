﻿using DiskoAIO.Properties;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for JoinerView.xaml
    /// </summary>
    public partial class JoinerView : UserControl
    {
        public static string[] captcha_bots { get; set; } = new string[] { "Discord", "Wick", "Captcha.bot" };
        public JoinerView()
        {
            InitializeComponent();
            var source = new string[] { };
            foreach (var group in App.proxyGroups)
            {
                source = source.Append(group._name).ToArray();
            }
            ProxiesGroup.ItemsSource = source;
            var source1 = new string[] { };
            foreach (var group in App.accountsGroups)
            {
                source1 = source1.Append(group._name).ToArray();
            }
            TokenGroup.ItemsSource = source1;
            CaptchaBotGroup.ItemsSource = captcha_bots;
            TokenGroup.SelectedItem = Settings.Default.TokenGroup;
            ProxiesGroup.SelectedItem = Settings.Default.ProxyGroup;
            CaptchaBotGroup.SelectedItem = captcha_bots.First();

            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            AcceptRules.IsChecked = Settings.Default.AcceptRules;
                            BypassReaction.IsChecked = Settings.Default.BypassReaction;
                            UseProxies.IsChecked = Settings.Default.UseProxies;
                            BypassCaptcha.IsChecked = Settings.Default.BypassCaptcha;
                            CaptchaBotGroup.Visibility = BypassCaptcha.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                            CaptchaBotLabel.Visibility = BypassCaptcha.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                            if (captcha_bots.Contains(CaptchaBotGroup.SelectedItem.ToString()))
                                CaptchaChannelBorder.Visibility = BypassCaptcha.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                        });
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(1);
                    }
                }
            });

            VerificationChannelBorder.Visibility = Settings.Default.AcceptRules ? Visibility.Visible : Visibility.Collapsed;
            ProxiesGroup.Visibility = Settings.Default.UseProxies ? Visibility.Visible : Visibility.Collapsed;
            ProxiesLabel.Visibility = Settings.Default.UseProxies ? Visibility.Visible : Visibility.Collapsed;

            MinimumDelay.Text = Settings.Default.Delay.ToString();
        }
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ulong result;
            if (!ulong.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
        }

        private void Join_Task(object sender, RoutedEventArgs e)
        {
            var info = Invite.Text.Split('/');
            if (Invite.Text == "")
            {
                App.mainWindow.ShowNotification("Please insert a valid message link");
                return;
            }
            if (TokenGroup.SelectedItem == null)
            {
                App.mainWindow.ShowNotification("Please select an account group");
                return;
            }
            if (ProxiesGroup.SelectedItem == null && (bool)UseProxies.IsChecked)
            {
                App.mainWindow.ShowNotification("Please select a proxy group group");
                return;
            }
            if(BypassCaptcha.IsChecked == true && Settings.Default.DeathByCaptcha == "")
            {
                App.mainWindow.ShowNotification("Please insert a deeathbycaptcha key to use captcha verification");
                return;
            }
            AccountGroup accounts = null;
            foreach (var group in App.accountsGroups)
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
            ProxyGroup proxies = null;
            if ((bool)UseProxies.IsChecked)
            {
                foreach (var group in App.proxyGroups)
                {
                    if (group._name == ProxiesGroup.SelectedItem.ToString())
                    {
                        proxies = group;
                        break;
                    }
                }
                if (proxies == null)
                {
                    App.mainWindow.ShowNotification("Couldn't get specified proxy group, try again later");
                    return;
                }
            }
            int skip = 0;
            int max = 0;
            int delay = Settings.Default.Delay;
            if (MinimumDelay.Text != "")
                delay = int.Parse(MinimumDelay.Text);
            if (SkipTokens.Text != "")
                skip = int.Parse(SkipTokens.Text);
            if (MaxTokens.Text != "")
                max = int.Parse(MaxTokens.Text);
            if(BypassCaptcha.IsChecked == true && delay < 3)
            {
                App.mainWindow.ShowNotification("Please use a delay of 3 seconds or more when using captcha verification to avoid crash");
                return;
            }
            if (ulong.TryParse(ChannelID.Text, out var channelID))
            {

            }
            if (ulong.TryParse(CaptchaChannelID.Text, out var captchaChannelID))
            {

            }
            var joinerTask = new JoinTask(accounts, Invite.Text, channelID, proxies, delay, max, skip, (bool)AcceptRules.IsChecked, (bool)BypassReaction.IsChecked, (bool)BypassCaptcha.IsChecked, captchaChannelID, CaptchaBotGroup.SelectedItem.ToString());
            joinerTask.Start();
            App.taskManager.AddTask(joinerTask);
            App.mainWindow.ShowNotification("Task started successfully");
        }
        private void Leave_Task(object sender, RoutedEventArgs e)
        {
            var info = Invite.Text.Split('/');
            if (Invite.Text == "")
            {
                App.mainWindow.ShowNotification("Please insert a valid message link");
                return;
            }
            if(TokenGroup.SelectedItem == null)
            {
                App.mainWindow.ShowNotification("Please select an account group");
                return;
            }
            if (ProxiesGroup.SelectedItem == null && (bool)UseProxies.IsChecked)
            {
                App.mainWindow.ShowNotification("Please select a proxy group group");
                return;
            }
            AccountGroup accounts = null;
            foreach (var group in App.accountsGroups)
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
            ProxyGroup proxies = null;
            if ((bool)UseProxies.IsChecked)
            {
                foreach (var group in App.proxyGroups)
                {
                    if (group._name == ProxiesGroup.SelectedItem.ToString())
                    {
                        proxies = group;
                        break;
                    }
                }
                if (proxies == null)
                {
                    App.mainWindow.ShowNotification("Couldn't get specified proxy group, try again later");
                    return;
                }
            }
            int skip = 0;
            int max = 0;
            int delay = Settings.Default.Delay;
            if (MinimumDelay.Text != "")
                delay = int.Parse(MinimumDelay.Text);
            if (SkipTokens.Text != "")
                skip = int.Parse(SkipTokens.Text);
            if (MaxTokens.Text != "")
                max = int.Parse(MaxTokens.Text);
            ulong channelID = 0;
            if (ulong.TryParse(ChannelID.Text, out channelID))
            {

            }
            var leaveTask = new LeaveTask(accounts, Invite.Text, proxies, delay, max, skip);
            leaveTask.Start();
            App.taskManager.AddTask(leaveTask);
            App.mainWindow.ShowNotification("Task started successfully");
        }

        private void BypassReaction_Click(object sender, RoutedEventArgs e)
        {
            VerificationChannelBorder.Visibility = (bool)BypassReaction.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }
        private void UseProxies_Click(object sender, RoutedEventArgs e)
        {
            ProxiesLabel.Visibility = (bool)UseProxies.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            ProxiesGroup.Visibility = (bool)UseProxies.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BypassCaptcha_Click(object sender, RoutedEventArgs e)
        {
            CaptchaBotGroup.Visibility = BypassCaptcha.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            CaptchaBotLabel.Visibility = BypassCaptcha.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            if(captcha_bots.Contains(CaptchaBotGroup.SelectedItem.ToString()))
                CaptchaChannelBorder.Visibility = BypassCaptcha.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Captcha_Change(object sender, ContextMenuEventArgs e)
        {
            CaptchaChannelBorder.Visibility = (bool)UseProxies.IsChecked && captcha_bots.Contains(CaptchaBotGroup.SelectedItem.ToString()) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
