using Discord.Gateway;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.Commands
{
    public abstract class CommandBase
    {
        public DiscordSocketClient Client { get; private set; }
        public DiscordMessage Message { get; private set; }
        public static Dictionary<ulong, bool> isAdminDict { get; set; }
        public List<ulong> admin_roles { get; private set; }

        internal void Prepare(DiscordSocketClient client, DiscordMessage message)
        {
            Client = client;
            Message = message;
            if (admin_roles == null)
                admin_roles = new List<ulong>() { };
            if (isAdminDict == null)
                isAdminDict = new Dictionary<ulong, bool>() { };

            foreach (var role in Client.GetGuild(Message.Guild.Id).Roles)
            {
                if ((role.Permissions & DiscordPermission.Administrator) == DiscordPermission.Administrator)
                {
                    admin_roles.Add(role.Id);
                }
            }
        }
        public bool CanSendEmbed()
        {
            var channel = (TextChannel)Client.GetChannel(Message.Channel.Id);
            
            if (channel.PermissionOverwrites.Count == 0)
                return true;

            if (isAdmin())
            {
                return true;
            }

            foreach (var entry in channel.PermissionOverwrites)
            {
                if (entry.AffectedId == Message.Author.User.Id)
                {
                    var result = entry.GetPermissionState(DiscordPermission.EmbedLinks) == OverwrittenPermissionState.Allow;
                    if (result)
                        return true;
                }
            }
            return false;
        }
        public bool isAdmin()
        {
            try
            {
                if (isAdminDict[Message.Guild.Id] == true)
                    return true;
                else
                    return false;
            }
            catch
            {
                foreach (var role in Client.GetCachedGuild(Message.Guild.Id).GetMember(Client.User.Id).Roles)
                {
                    foreach (var admin in admin_roles)
                    {
                        if (role == admin)
                        {
                            isAdminDict[Message.Guild.Id] = true;
                            return true;
                        }
                    }
                }
                isAdminDict[Message.Guild.Id] = false;
                return false;
            }
        }
        public void SendMessageAsync(string to_send)
        {
            try
            {
                if (CanSendEmbed())
                {
                    var embed = new EmbedMaker() { Title = Client.User.Username, TitleUrl = "https://discord.gg/bXfjwSeBur", Color = System.Drawing.Color.IndianRed, ThumbnailUrl = Client.User.Avatar.Url, Description = to_send };
                    Task.Run(() => Message.Channel.SendMessage(embed));
                    return;
                }
                else
                {
                    Task.Run(() => Message.Channel.SendMessage(to_send));
                }
            }
            catch
            {
                CommandBase.isAdminDict[Message.Guild.Id] = false;

                Task.Run(() => Message.Channel.SendMessage(to_send));
            }
        }
        public abstract void Execute();
        public virtual void HandleError(string parameterName, string providedValue, Exception exception) { }
    }
}
