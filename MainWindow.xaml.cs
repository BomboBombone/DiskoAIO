using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Threading;
using DiskoAIO.MVVM.View;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Reflection;

namespace DiskoAIO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        System.Timers.Timer clock_time { get; set; } = null;
        public NotificationState notificationState { get; set; } = NotificationState.Closed;
        public int notificationAnimationDuration { get; } = 2000;
        public MainWindow()
        {
            InitializeComponent();
            VersionLabel.Content = 'v' + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Dispatcher.InvokeShutdown();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var confirm = new WarningPopupView("All of your current tasks will be stopped and deleted.\nAre you sure you want to quit?");
            confirm.ShowDialog();
            if (confirm.hasConfirmed != true)
                return;
            WindowState = WindowState.Minimized;
            window.ShowInTaskbar = false;
            Science.SendStatistic(ScienceTypes.logout);

            DiscordDriver.CleanUp();
            Application.Current.Dispatcher.InvokeShutdown();

            Environment.Exit(0);
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            var source = (RadioButton)e.Source;
            if ((bool)source.IsChecked)
            {
                source.Foreground = Brushes.White;
                source.FontWeight = FontWeights.Black;
            }
            if (!(bool)JoinerRadio.IsChecked)
            {
                JoinerRadio.Foreground = new SolidColorBrush(Color.FromRgb(162, 167, 188));
                JoinerRadio.FontWeight = FontWeights.Bold;
            }
        }
        private void Radio_LostFocus(object sender, RoutedEventArgs e)
        {
            var source = (RadioButton)e.Source;
            source.Foreground = new SolidColorBrush(Color.FromRgb(162, 167, 188));
            source.FontWeight = FontWeights.Bold;
        }
        private void Close_Notification_Click(object sender, RoutedEventArgs e)
        {
            notificationState = NotificationState.Closed;
            Storyboard sb = Resources["CloseMenu"] as Storyboard;
            sb.Begin(InAppNotification);
        }
        public void ShowNotification(string text, int duration = 2000)
        {
            Task.Run(() =>
            {
                while (notificationState == NotificationState.Visible)
                {
                    Thread.Sleep(1000);
                }
                notificationState = NotificationState.Visible;

                Dispatcher.Invoke(() => {
                    NotificationText.Text = text;

                    Storyboard sb = Resources["OpenMenu"] as Storyboard;
                    sb.Begin(InAppNotification);
                });

                int ticks = 0;
                while (ticks < duration)
                {
                    Thread.Sleep(1);
                    ticks += 1;
                    if (notificationState == NotificationState.Closed)
                        return;
                }

                Dispatcher.Invoke(() =>
                {
                    Storyboard sb = Resources["CloseMenu"] as Storyboard;

                    sb.Begin(InAppNotification);
                    notificationState = NotificationState.Closed;
                });
            });
        }

    }
    public enum NotificationState
    {
        Closed,
        Visible
    }
}
