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
using System.Windows.Shapes;

namespace DiskoAIO.MVVM.View
{
    /// <summary>
    /// Interaction logic for TaskSettingsPopup.xaml
    /// </summary>
    public partial class TaskSettingsPopup : Window
    {
        public TaskSettingsPopup(string agName, string pgName, bool isOtherEnabled = false, string otherName = "", string otherValue = "")
        {
            InitializeComponent();
            AccountGroupBox.Text = agName;
            ProxyGroupBox.Text = pgName;
            OtherPanel.Visibility = isOtherEnabled ? Visibility.Visible : Visibility.Collapsed;
            OtherLabel.Content = otherName;
            OtherTextBox.Text = otherValue;
        }
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ulong result;
            if (!ulong.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
        }
    }
}
