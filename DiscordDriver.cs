using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace DiskoAIO
{
    class DiscordDriver
    {
        ChromeDriver driver;
        public static List<ChromeDriver> driver_list = new List<ChromeDriver>();
        public DiscordDriver(string token)
        {
            Task.Run(() =>
            {
                driver = (ChromeDriver)GetSeleniumDriver();
                driver.ExecuteScript("window.location.href = \"https://discord.com/login\"");
                driver.ExecuteScript($"let token = \"{token}\";" +
                    "function login(token) {" +
                    "    setInterval(() => {" +
                    "      document.body.appendChild(document.createElement `iframe`).contentWindow.localStorage.token = `\"${ token}\"`" +
                    "    }, 50);" +
                    "    setTimeout(() => {" +
                    "      location.reload();" +
                    "    }, 2500);" +
                    "   }" +
                    "login(token);");
                driver_list.Add(driver);
                while (true)
                {
                    try
                    {
                        driver.GetDevToolsSession();
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        driver.Dispose();
                    }
                }
            });
            App.mainWindow.ShowNotification("Discord started, loggin in. Please wait...");
        }
        public static void CleanUp()
        {
            foreach(var driver in driver_list)
            {
                driver.Dispose();
            }
        }
        private IWebDriver GetSeleniumDriver()
        {
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;
            return new ChromeDriver(chromeDriverService, new ChromeOptions());
        }
    }
}
