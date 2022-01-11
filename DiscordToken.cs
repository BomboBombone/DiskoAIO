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
        public ulong User_id
        {
            get { return _user_id; }
            set { _user_id = value; }
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
                if (_note.Length > 20)
                    return _note.Substring(0, 17) + "...";
                else
                    return _note;
                }
            set { _note = value; }
        }
        public DiscordToken(int group_id, ulong user_id, string token, bool isPhoneVerified = false, bool isMailVerified = false, string note = null)
        {
            _group_id = group_id;
            _user_id = user_id;
            _token = token;
            _isPhoneVerified = isPhoneVerified;
            _isMailVerified = isMailVerified;
            _note = note;
        }
        public override string ToString()
        {
            return this._token + ':' + this._user_id + ':' + this.IsPhoneVerified + ':' + IsMailVerified.ToString() + ':' + this._note;
        }
    }
}
