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
        public static List<string> types = new List<string>() { "Connect Discord", "Connect Twitter", "Subscribe" };
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
