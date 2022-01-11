using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskoAIO
{
    public class DiscordProxy
    {
        public string _host { get; set; }
        public string Host
        {
            get { return _host; }
            set { _host = value; }
        }
        public int _port { get; set; }
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }
        public string _username { get; set; }
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }
        public string _password { get; set; }
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        public DiscordProxy(string host, int port, string username = null, string password = null)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
        }
    }
}
