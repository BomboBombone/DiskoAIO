﻿namespace Discord.Gateway
{
    public class LockedSocketConfig : LockedDiscordConfig
    {
        public bool Cache { get; }
        public bool ParseIncomingRTPData { get; }
        public DiscordGatewayIntent? Intents { get; }
        public uint VoiceChannelConnectTimeout { get; }
        public DiscordShard Shard { get; }

        public LockedSocketConfig(DiscordSocketConfig config) : base(config)
        {
            Cache = config.Cache;
            ParseIncomingRTPData = config.HandleIncomingMediaData;
            Intents = config.Intents;
            VoiceChannelConnectTimeout = config.VoiceChannelConnectTimeout;
            Shard = config.Shard;
        }
    }
}
