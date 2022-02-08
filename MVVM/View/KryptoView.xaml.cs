using DiskoAIO.DiskoTasks;
using DiskoAIO.Properties;
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
    /// Interaction logic for KryptoView.xaml
    /// </summary>
    public partial class KryptoView : UserControl
    {
        public KryptoView()
        {
            InitializeComponent();
        }
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ulong result;
            if (!ulong.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
        }
        private void Start_Task(object sender, RoutedEventArgs e)
        {
            if (MessageLink.Text.Length != 39 || MessageLink.Text.Split('/').Last().Length != 8 || !MessageLink.Text.StartsWith("https://www.kryptosign.io/sign/"))
            {
                App.mainWindow.ShowNotification("Please insert a valid message link");
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
            int skip = 0;
            int delay = Settings.Default.Delay;
            if (MinimumDelay.Text != "")
                delay = int.Parse(MinimumDelay.Text);
            if (SkipTokens.Text != "")
                skip = int.Parse(SkipTokens.Text);
            var krptosigntask = new KryptoSignTask(accounts, proxies, MessageLink.Text.ToString(), delay, 0, skip);
            App.taskManager.AddTask(krptosigntask);
            krptosigntask.Start();
        }
    }
}
