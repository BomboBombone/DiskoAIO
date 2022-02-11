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
    /// Interaction logic for ChatDeleteWindow.xaml
    /// </summary>
    public partial class ChatDeleteWindow : Window
    {
        public ChatDeleteWindow()
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

        private void Delete_Message(object sender, RoutedEventArgs e)
        {
            var statement = MessageText.Text;
            var serverID = ServerID.Text;
            if(statement == null || statement.Remove(' ') == "" || !ulong.TryParse(serverID, out var server_id) || serverID.Length != 18)
            {
                return;
            }
            Task.Run(() =>
            {
                BobbyAPI.Forget(' ' + statement.Trim(' ') + ' ', serverID);
                Application.Current.Dispatcher.Invoke(() => {
                    App.mainWindow.ShowNotification("Deleted all sentences containing the specified text");
                });
            });
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
