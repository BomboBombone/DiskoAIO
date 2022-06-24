using DiskoAIO.Twitter;
using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiskoAIO
{
    public class Science
    {
        public const string login_action = "login";
        public const string logout_action = "logout";
        public const string win_action = "win";
        public const string giveaway_task_finished = "giveaway";
        public const string join_task_finished = "join";
        public const string endpoint = "https://diskoaio.com/api/v3/science";
        public static void SendStatistic(ScienceTypes type)
        {
            string action = "login";
            switch (type)
            {
                case ScienceTypes.login:
                    break;
                case ScienceTypes.logout:
                    action = logout_action;
                    break;
                case ScienceTypes.join:
                    action = join_task_finished;
                    break;
                case ScienceTypes.giveaway:
                    action = giveaway_task_finished;
                    break;
                case ScienceTypes.win:
                    action = win_action;
                    break;
            }
            HttpRequest request = new HttpRequest()
            {
            };
            request.AddHeader("X-Forwarded-For", App.localIP);
            string json = "{\"action\":\"" + action + "\"}";

            request.AddHeader("Host", "diskoaio.com");
            request.AddHeader("Content-Length", json.Length.ToString());
            request.AddHeader("Authorization", "Bearer " + App.api_key);
            HttpResponse res = null;
            try
            {
                res = request.Post(endpoint, json, "application/json");
            }
            catch (Exception ex)
            {
                //if(ex.Message.Contains("401"))
                //{
                //    DiscordDriver.CleanUp();
                //    TwitterDriver.CleanUp();
                //
                //    Application.Current.Dispatcher.InvokeShutdown();
                //    MessageBox.Show("Your account was not found, shutting down.");
                //    Environment.Exit(1);
                //}
                //else
                //{
                //    Debug.Log("Science error: " + ex.Message);
                //}
            }
        }
    }
    public enum ScienceTypes
    {
        login,
        logout,
        win,
        giveaway,
        join
    }
}
