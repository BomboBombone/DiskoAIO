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

namespace DiskoAIO.CaptchaSolvers
{
    class DiscordSolver
    {
        public static string Solve(string site_key = "4c672d35-0701-42b2-88c3-78380b0db560")
        {
            if (Settings.Default.Anti_Captcha == "")
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.mainWindow.ShowNotification("You haven't inserted your anti-captcha key yet");
                });
                return null;
            }
            DebugHelper.VerboseMode = false;

            var api = new HCaptchaProxyless()
            {
                WebsiteUrl = new Uri("https://discord.com/"),
                WebsiteKey = site_key,
                ClientKey = Settings.Default.Anti_Captcha
            };

            while (!api.CreateTask())
            {
                Debug.Log(api.ErrorMessage);
                Thread.Sleep(1000);
                if (!api.ErrorMessage.Contains("idle workers"))
                    return null;
            }
            if (!api.WaitForResult())
            {
                Debug.Log("Couldn't get captcha result...");
                return null;
            }
            else
            {
                return api.TaskInfo.Solution.GRecaptchaResponse;
            }
        }
    }
}
