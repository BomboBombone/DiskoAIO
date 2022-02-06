using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskoAIO
{
    public class Science
    {
        public const string login_action = "login";
        public const string logout_action = "logout";
        public const string win_action = "win";
        public const string giveaway_task_finished = "giveaway";
        public const string join_task_finished = "join";
        public const string endpoint = "https://diskoaio.com/api/science";
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
            try
            {
                request.Post(endpoint, json, "application/json");
            }
            catch (Exception ex)
            {
                Debug.Log("Science error: " + ex.Message);
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
