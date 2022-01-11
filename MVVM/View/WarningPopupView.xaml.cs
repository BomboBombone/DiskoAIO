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
    /// Interaction logic for WarningPopupView.xaml
    /// </summary>
    public partial class WarningPopupView : Window
    {
        public bool hasConfirmed { get; set; } = false;

        public WarningPopupView(string input, bool canCancel = true)
        {
            InitializeComponent();
            TextField.Text = input;
            if (!canCancel)
                CancelButton.Visibility = Visibility.Hidden;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            hasConfirmed = true;
            this.Close();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
