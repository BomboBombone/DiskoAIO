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
    /// Interaction logic for ChatTaskSettingsPopup.xaml
    /// </summary>
    public partial class ChatTaskSettingsPopup : Window
    {
        public ChatTaskSettingsPopup(ulong serverID, ulong channelID, ulong userID, ulong lvlChannelID, int maxlvl)
        {
            InitializeComponent();

            ServerID.Text = serverID.ToString();
            ChannelID.Text = channelID.ToString();
            UserID.Text = userID.ToString();
            LvlChannelID.Text = lvlChannelID.ToString();
            MaxLvl.Text = maxlvl.ToString();
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
