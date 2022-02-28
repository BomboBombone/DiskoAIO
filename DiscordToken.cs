using Discord;
using DiskoAIO.CaptchaSolvers;
using DiskoAIO.Properties;
using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskoAIO
{
    public class DiscordToken
    {
        public int _group_id { get; set; }
        public ulong _user_id { get; set; } 
        public string User_id
        {
            get { return _user_id.ToString(); }
            set { _user_id = ulong.Parse(value); }
        }
        public string _token { get; set; }
        public bool _isPhoneVerified { get; set; }
        public bool IsPhoneVerified
        {
            get { return _isPhoneVerified; }
            set { _isPhoneVerified = value; }
        }
        public bool _isMailVerified { get; set; }
        public bool IsMailVerified
        {
            get { return _isMailVerified; }
            set { _isMailVerified = value; }
        }
        public string _note { get; set; }
        public string Note
        {
            get {
                if (_note == null)
                    _note = "Double click to add note...";
                return _note;
                }
            set { _note = value; }
        }
        public DiscordToken(ulong user_id, string token, bool isPhoneVerified = false, bool isMailVerified = false, string note = null)
        {
            _user_id = user_id;
            _token = token;
            _isPhoneVerified = isPhoneVerified;
            _isMailVerified = isMailVerified;
            _note = note;
        }
        public static DiscordToken Load(string[] token_array)
        {
            try
            {
                DiscordClient client = null;
                if(!ulong.TryParse(token_array[0], out var id))
                {
                    foreach (var part in token_array)
                    {
                        if (part.Length == 59)
                        {
                            try
                            {
                                if (App.IsConnectedToInternet())
                                {
                                    client = new DiscordClient(part);

                                    client.QueryGuilds(new GuildQueryOptions()
                                    {
                                        Limit = 1
                                    });
                                }
                                else
                                    throw new InvalidTokenException("");
                                return new DiscordToken(client.User.Id, part, client.User.PhoneNumber == null ? false : true, client.User.EmailVerified);
                            }
                            catch (InvalidTokenException)
                            {
                                return null;
                            }
                        }
                    }
                    if(Settings.Default.Anti_Captcha != "")
                        foreach(var part in token_array)
                        {
                            if(part.Contains('@') && part.Contains('.'))
                            {
                                var pw = token_array[token_array.ToList().IndexOf(part) + 1];
                                var token = Login(part, pw);
                                if (App.IsConnectedToInternet())
                                {
                                    client = new DiscordClient(token);

                                    client.QueryGuilds(new GuildQueryOptions()
                                    {
                                        Limit = 1
                                    });
                                }
                                else
                                    throw new InvalidTokenException("");
                                return new DiscordToken(client.User.Id, token, client.User.PhoneNumber == null ? false : true, client.User.EmailVerified);
                            }
                        }
                }

                if (token_array.Length < 2)
                {
                    return null;
                }
                else if (token_array.Length == 3)
                {
                    return new DiscordToken(ulong.Parse(token_array[0]), token_array[1], bool.Parse(token_array[2]));
                }
                else if (token_array.Length == 4)
                {
                    return new DiscordToken(ulong.Parse(token_array[0]), token_array[1], bool.Parse(token_array[2]), bool.Parse(token_array[3]));
                }
                else if (token_array.Length == 5)
                {
                    return new DiscordToken(ulong.Parse(token_array[0]), token_array[1], bool.Parse(token_array[2]), bool.Parse(token_array[3]), token_array[4]);
                }
                else
                {
                    return new DiscordToken(ulong.Parse(token_array[0]), token_array[1]);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static string Login(string email, string password)
        {
            var captcha_key = DiscordSolver.Solve("f5561ba9-8f1e-40ca-9b5b-a0b3f719ef34");
            var request = new HttpRequest() { };
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36");
            request.AddHeader("x-debug-options", "bugReporterEnabled");
            request.AddHeader("x-fingerprint", "945448142466334752.3vANn7dUuaXHZ8eOFy75OP0r9og");
            request.AddHeader("x-super-properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiQ2hyb21lIiwiZGV2aWNlIjoiIiwic3lzdGVtX2xvY2FsZSI6Iml0LUlUIiwiYnJvd3Nlcl91c2VyX2FnZW50IjoiTW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NCkgQXBwbGVXZWJLaXQvNTM3LjM2IChLSFRNTCwgbGlrZSBHZWNrbykgQ2hyb21lLzk4LjAuNDc1OC4xMDIgU2FmYXJpLzUzNy4zNiIsImJyb3dzZXJfdmVyc2lvbiI6Ijk4LjAuNDc1OC4xMDIiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6Imh0dHBzOi8vd2VibWFpbC5yZWdpc3Rlci5pdC8iLCJyZWZlcnJpbmdfZG9tYWluIjoid2VibWFpbC5yZWdpc3Rlci5pdCIsInJlZmVycmVyX2N1cnJlbnQiOiIiLCJyZWZlcnJpbmdfZG9tYWluX2N1cnJlbnQiOiIiLCJyZWxlYXNlX2NoYW5uZWwiOiJzdGFibGUiLCJjbGllbnRfYnVpbGRfbnVtYmVyIjoxMTU2MzMsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");
            var payload = "{\"login\":\""+ email + "\",\"password\":\"" + password + "\",\"undelete\":false,\"captcha_key\":\"" + captcha_key + "\",\"login_source\":null,\"gift_code_sku_id\":null}";
            request.AddHeader("Content-Length", payload.Length.ToString());
            var response = request.Post("https://discord.com/api/v9/auth/login", payload, "application/json");
            var res_array = response.ToString().Split('"');
            return res_array[2];
        }
        public override string ToString()
        {
            return this.User_id + ':' + this._token + ':' + this.IsPhoneVerified + ':' + IsMailVerified.ToString() + ':' + this.Note;
        }
    }
}
