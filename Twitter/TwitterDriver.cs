using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiskoAIO.Twitter
{
    class TwitterDriver
    {
        ChromeDriver driver;
        public static List<ChromeDriver> driver_list = new List<ChromeDriver>();
        public TwitterDriver(string cookies)
        {
            Task.Run(() =>
            {
                try
                {
                    driver = (ChromeDriver)GetSeleniumDriver();
                    driver.ExecuteScript($"window.location.href = \"https://twitter.com\"");
                    foreach(var cookie1 in cookies.Split(';'))
                    {
                        var cookie = cookie1.Trim();
                        var cookie_arr = cookie.Split('=');
                        driver.Manage().Cookies.AddCookie(new Cookie(cookie_arr[0], cookie_arr[1]));
                    }
                    driver.Navigate().Refresh();
                    driver_list.Add(driver);
                    while (true)
                    {
                        try
                        {
                            if (driver.GetDevToolsSession().ActiveSessionId == null)
                                throw new Exception();
                            Thread.Sleep(5000);
                        }
                        catch (Exception ex)
                        {
                            driver.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            });
            App.mainWindow.ShowNotification("Twitter started, logging in. Please wait...");
        }
        public static void CleanUp()
        {
            foreach (var driver in driver_list)
            {
                driver.Dispose();
            }
        }
        private IWebDriver GetSeleniumDriver()
        {
            var chromeDriverService = ChromeDriverService.CreateDefaultService(App.strWorkPath);
            chromeDriverService.HideCommandPromptWindow = true;
            return new ChromeDriver(chromeDriverService, new ChromeOptions());
        }
    }
}
