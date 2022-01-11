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
    /// Interaction logic for JoinerView.xaml
    /// </summary>
    public partial class JoinerView : UserControl
    {
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
            TokenGroup.SelectedItem = Settings.Default.TokenGroup;
            ProxiesGroup.SelectedItem = Settings.Default.ProxyGroup;

            AcceptRules.IsChecked = Settings.Default.AcceptRules;
            BypassReaction.IsChecked = Settings.Default.BypassReaction;
            VerificationChannelBorder.Visibility = Settings.Default.AcceptRules ? Visibility.Visible : Visibility.Collapsed;
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
            App.mainWindow.ShowNotification("Task started successfully");
        }
        private void Leave_Task(object sender, RoutedEventArgs e)
        {
            App.mainWindow.ShowNotification("Task started successfully");
        }
    }
}
