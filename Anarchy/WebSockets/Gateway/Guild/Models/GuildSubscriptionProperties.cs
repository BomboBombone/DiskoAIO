﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Gateway
{
    public class GuildSubscriptionProperties
    {
        [JsonProperty("guild_id")]
        internal ulong GuildId { get; set; }

        private readonly DiscordParameter<bool> _typeParam = new DiscordParameter<bool>();
        [JsonProperty("typing")]
        public bool Typing
        {
            get { return _typeParam; }
            set { _typeParam.Value = value; }
        }

        public bool ShouldSerializeTyping() => _typeParam.Set;

        private readonly DiscordParameter<bool> _threadParam = new DiscordParameter<bool>();
        [JsonProperty("threads")]
        public bool Threads
        {
            get { return _threadParam; }
            set { _threadParam.Value = value; }
        }

        public bool ShouldSerializeThreads() => _threadParam.Set;

        private readonly DiscordParameter<bool> _activityParam = new DiscordParameter<bool>();
        [JsonProperty("activities")]
        public bool Activities
        {
            get { return _activityParam; }
            set { _activityParam.Value = value; }
        }

        public bool ShouldSerializeActivities() => _activityParam.Set;

        [JsonProperty("members")]
        public List<ulong> Members { get; set; } = new List<ulong>();

        [JsonProperty("channels")]
        public Dictionary<ulong, int[][]> Channels { get; set; } = new Dictionary<ulong, int[][]>();

        [JsonProperty("thread_member_lists")]
        private readonly List<object> _threadMemberLists = new List<object>();
    }
}
