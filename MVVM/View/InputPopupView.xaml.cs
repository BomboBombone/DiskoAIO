using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for InputPopupView.xaml
    /// </summary>
    public partial class InputPopupView : Window
    {
        public bool hasConfirmed { get; set; } = false;
        public string answer { get; set; } = null;
        public int current_chars { get; set; } = 0;
        public int limit { get; set; }
        private bool canUserNumbers { get; set; } = false;

        public InputPopupView(string prompt, int char_limit = 32, bool useNumbers = false, string preview_text = "")
        {
            InitializeComponent();
            PromptText.Text = prompt;
            limit = char_limit;
            LimitText.Text = "0/" + char_limit.ToString();
            canUserNumbers = useNumbers;
            if (preview_text == "Double click to add note...")
                preview_text = "";
            InputText.Text = preview_text;
            InputText.TextChanged += InputText_TextChanged;
            InputText.Focus();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            hasConfirmed = true;
            answer = InputText.Text;
            this.Close();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InputText_TextChanged(object sender, TextChangedEventArgs e)
        {
            var change = e.Changes.First();
            if(change.AddedLength > 0)
            {
                current_chars += change.AddedLength;
            }
            else
            {
                current_chars -= change.RemovedLength;
            }
            LimitText.Text = current_chars.ToString() + '/' + limit;
            if(current_chars > limit / 2 && current_chars < limit)
            {
                LimitText.Foreground = Brushes.Orange;
            }
            else if (current_chars >= limit - limit/10)
            {
                LimitText.Foreground = Brushes.Red;
            }
            else
            {
                LimitText.Foreground = Brushes.Gray;
            }
        }

        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ulong result;
            if (!ulong.TryParse(e.Text, out result) || canUserNumbers)
            {
                var replaced = Regex.Replace(e.Text, "[^A-Za-z0-9]", "");
                if (current_chars >= limit || replaced != e.Text)
                    e.Handled = true;
            }
        }
    }
}
