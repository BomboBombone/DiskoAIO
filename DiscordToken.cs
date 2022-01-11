using Discord;
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
                    _note = "Double tap to add note...";
                if (_note.Length > 20)
                    return _note.Substring(0, 17) + "...";
                else
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
            DiscordClient client = null;
            if(token_array.First().Length == 59)
            {
                try
                {
                    client = new DiscordClient(token_array[0]);
                }
                catch (InvalidTokenException)
                {
                    return null;
                }
            }
            if (token_array.First().Length == 59)
            {
                return new DiscordToken(client.User.Id, token_array[0], client.User.PhoneNumber == null ? false: true, client.User.EmailVerified);
            }
            else if(token_array.Length < 2)
            {
                return null;
            }
            else if(token_array.Length == 3)
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
        public override string ToString()
        {
            return this.User_id + ':' + this._token + ':' + this.IsPhoneVerified + ':' + IsMailVerified.ToString() + ':' + this.Note;
        }
    }
}
