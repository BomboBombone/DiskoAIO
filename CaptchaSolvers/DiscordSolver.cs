using Anticaptcha_example.Api;
using Anticaptcha_example.Helper;
using DiskoAIO.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DeathByCaptcha;
using System.Collections;

namespace DiskoAIO.CaptchaSolvers
{
    class DiscordSolver
    {
        public static string Solve(string site_key)
        {
            if (Settings.Default.DeathByCaptcha == "")
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.mainWindow.ShowNotification("You haven't inserted your deathbycaptcha key yet");
                });
                return null;
            }
            Client client = new HttpClient("authtoken", Settings.Default.DeathByCaptcha);
            string tokenParams = "{\"sitekey\": \"" + site_key + "\"," +
                "\"pageurl\": \"" + "https://discord.com/" + "\"}";
            Captcha captcha = client.Decode(Client.DefaultTimeout,
                new Hashtable(){
                        { "type", 7 },
                        {"token_params", tokenParams}
                });
            if (captcha.Correct && captcha.Solved)
            {
                return captcha.Text;
            }
            else
                return null;
        }
    }
}
