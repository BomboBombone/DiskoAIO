using System;
using System.IO;
using System.Threading;
using System.Windows;
using Anticaptcha_example.Api;
using Anticaptcha_example.Helper;
using DeathByCaptcha;
using DiskoAIO.Properties;
using Newtonsoft.Json.Linq;

namespace DiskoAIO.CaptchaSolvers
{
    public class WickSolver
    {
        public static string Solve(string url)
        {
            if (Settings.Default.DeathByCaptcha == "")
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.mainWindow.ShowNotification("You haven't inserted your deathbycaptcha key yet");
                });
                return null;
            }
            Client client = new SocketClient("authtoken", Settings.Default.DeathByCaptcha);

            Captcha captcha = client.Decode(GetStreamFromUrl(url), Client.DefaultTimeout);
            if (captcha.Correct && captcha.Solved)
            {
                return captcha.Text;
            }
            else
                return null;
        }
        private static Stream GetStreamFromUrl(string url)
        {
            byte[] imageData = null;

            using (var wc = new System.Net.WebClient())
                imageData = wc.DownloadData(url);

            return new MemoryStream(imageData);
        }
    }
}