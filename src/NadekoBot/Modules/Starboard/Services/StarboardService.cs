using Discord;
using Discord.WebSocket;
using NadekoBot.Extensions;
using NadekoBot.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NadekoBot.Modules.Starboard.Services
{
    public class StarboardService : INService
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public StarboardService(DiscordSocketClient client, DbService db)
        {
            _client = client;
            _db = db;

            _client.ReactionAdded += (m, c, r) => { Task.Run(() => ReactionAdded(m, c, r)); return Task.CompletedTask; };
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var ch = channel as ITextChannel;
            if (ch == null)
                return;
            
            var msg = await message.GetOrDownloadAsync();
            // Ignore the bot's own messages
            if (msg.Author.Id == _client.CurrentUser.Id)
                return;

            // TODO: Ignore message's author

            var metadata = msg.Reactions;
            ReactionMetadata data;
            if (!metadata.TryGetValue(new Emoji("⭐"), out data))
                return;
            
            using (var uow = _db.UnitOfWork)
            {
                var config = uow.StarGuilds.GetOrCreate(ch.GuildId);
                if (data.ReactionCount < config.Required)
                    return;
                if (config.ChannelId == null)
                    return;
                var starChannel = await ch.Guild.GetTextChannelAsync(config.ChannelId.Value).ConfigureAwait(false);
                await PostMessage(starChannel, msg).ConfigureAwait(false);
            }
        }

        public async Task PostMessage(ITextChannel channel, IUserMessage message)
        {
            // don't post if it already has been posted
            using (var uow = _db.UnitOfWork)
            {
                var messages = uow.StarGuilds.GetMessages(channel.GuildId);
                if (messages == null)
                {
                    Console.WriteLine("message null");
                    return;
                }
                if (messages.Any(m => m.MessageId == message.Id))
                    return;
            }

            var eb = new EmbedBuilder();
            var efb = new EmbedFooterBuilder();

            EmbedBuilderExtensions.WithAuthor(eb, message.Author);
            eb.WithDescription(message.Content);

            if (message.Attachments.Count > 0)
                eb.WithImageUrl(message.Attachments.First().Url);

            efb.WithIconUrl(_client.CurrentUser.GetAvatarUrl());
            efb.WithText(message.Timestamp.ToString());

            eb.WithFooter(efb).WithOkColor();

            using (var uow = _db.UnitOfWork)
            {
                _db.UnitOfWork.StarGuilds.AddMessage(channel.GuildId, message.Id);
                _db.UnitOfWork.Complete();
            }

            await channel.EmbedAsync(eb).ConfigureAwait(false);
        }
    }
}
