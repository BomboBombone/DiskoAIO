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
    /// Interaction logic for DiscordBrowserWindow.xaml
    /// </summary>
    public partial class DiscordBrowserWindow : Window
    {
        public DiscordBrowserWindow(string token)
        {
            InitializeComponent();
            string script =
                @"  <script>
                        window.location.href = 'https://discord.com/login';
                        function login(token) {
                            setInterval(() => {
                                document.body.appendChild(document.createElement `iframe`).contentWindow.localStorage.token = `'${ token}'`}, 50);
                            setTimeout(() => {
                                        location.reload();
                                    }, 500);
                            }

                        login('" + token + "'); </script>";
        }
    }
}
