using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NadekoBot.Common.Attributes;
using NadekoBot.Modules.Starboard.Services;
using NadekoBot.Services;
using System;
using System.Threading.Tasks;

namespace NadekoBot.Modules.Starboard
{
    public class Starboard : NadekoTopLevelModule
    {
        private readonly StarboardService _service;
        private readonly DbService _db;
        private readonly DiscordSocketClient _client;

        public Starboard(StarboardService service, DbService db, DiscordSocketClient client)
        {
            _service = service;
            _db = db;
            _client = client;
        }

        [NadekoCommand, Description, Usage, Aliases]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task StarMessage(IUserMessage message)
        {
            var socketMsg = message as SocketUserMessage;
            if (socketMsg == null)
            {
                Console.WriteLine("Message not a guild text message");
                return;
            }

            using (var uow = _db.UnitOfWork)
            {
                var starChannel = uow.StarGuilds.GetOrCreate(Context.Guild.Id).ChannelId;
                if (starChannel == null)
                {
                    Console.WriteLine("No star channel");
                    return;
                }
                await _service.PostMessage(await Context.Guild.GetTextChannelAsync(starChannel.Value).ConfigureAwait(false), socketMsg).ConfigureAwait(false);
            }
        }

        [NadekoCommand, Description, Usage, Aliases]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        public Task SetStarsRequired(int amount)
        {
            if (amount < 1)
            {
                Console.WriteLine("amount < 1");
                return Task.CompletedTask;
            }

            using (var uow = _db.UnitOfWork)
            {
                var config = uow.StarGuilds.GetOrCreate(Context.Guild.Id);
                config.Required = amount;
                uow.Complete();
            }

            return Task.CompletedTask;
        }

        [NadekoCommand, Description, Usage, Aliases]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        public Task StarChannel(ITextChannel channel)
        {
            using (var uow = _db.UnitOfWork)
            {
                var config = uow.StarGuilds.GetOrCreate(Context.Guild.Id);
                config.ChannelId = channel.Id;
                uow.StarGuilds.Update(config);
                uow.Complete();
            }

            return Task.CompletedTask;
        }
    }
}
