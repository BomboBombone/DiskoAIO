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
    /// Interaction logic for CheckWinnersView.xaml
    /// </summary>
    public partial class CheckWinnersView : Window
    {
        public AccountGroup _currentGroup { get; set; } = null;

        public CheckWinnersView()
        {
            InitializeComponent();
            var source = new string[] { };

            foreach (var group in App.accountsGroups)
            {
                source = source.Append(group._name).ToArray();
            }
            TokenGroup.ItemsSource = source;
            if (_currentGroup != null)
                TokenGroup.SelectedItem = _currentGroup._name;
            else
                if(App.accountsGroups.Count > 0)
                    _currentGroup = App.accountsGroups.First();

            ServerID.Focus();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ulong result;
            if (!ulong.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Start_Task(object sender, RoutedEventArgs e)
        {
            App.mainWindow.ShowNotification("Task started successfully");
        }
        private void StackPanel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox s = e.Source as TextBox;
                if (s != null)
                {
                    s.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }

                e.Handled = true;
            }
        }
    }
}
