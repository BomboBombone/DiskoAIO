using System;
using System.Threading;
using System.Windows;
using Anticaptcha_example.Api;
using Anticaptcha_example.Helper;
using DiskoAIO.Properties;
using Newtonsoft.Json.Linq;

namespace DiskoAIO.CaptchaSolvers
{
    public class WickSolver
    {
        public static string Solve(string url)
        {
            if(Settings.Default.Anti_Captcha == "")
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.mainWindow.ShowNotification("You haven't inserted your anti-captcha key yet");
                });
                return null;
            }
            DebugHelper.VerboseMode = false;

            var api = new ImageToText
            {
                ClientKey = Settings.Default.Anti_Captcha,
                FilePath = url
            };

            while (!api.CreateTask())
            {
                Debug.Log(api.ErrorMessage);
                Thread.Sleep(1000);
                if(!api.ErrorMessage.Contains("idle workers"))
                    return null;
            }
            if (!api.WaitForResult())
            {
                Debug.Log("Couldn't get captcha result...");
                return null;
            }
            else
            {
                return api.TaskInfo.Solution.Text;
            }
        }
    }
}