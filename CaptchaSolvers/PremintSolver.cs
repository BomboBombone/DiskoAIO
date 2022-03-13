using DeathByCaptcha;
using DiskoAIO.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO.CaptchaSolvers
{
    class PremintSolver
    {
        public const string site_key = "6Lf9yOodAAAAADyXy9cQncsLqD9Gl4NCBx3JCR_x";
        public const string pageurl = "https://www.premint.xyz/";
        public static string Solve(string project_name)
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
            string tokenParams = "{\"googlekey\": \"" + site_key + "\"," +
                "\"pageurl\": \"" + pageurl + project_name + "/\"}";
            Captcha captcha = client.Decode(Client.DefaultTimeout,
                new Hashtable(){
                        { "type", 4 },
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
