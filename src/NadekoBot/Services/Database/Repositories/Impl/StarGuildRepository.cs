using NadekoBot.Services.Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace NadekoBot.Services.Database.Repositories.Impl
{
    public class StarGuildRepository : Repository<StarGuild>, IStarGuildRepository
    {
        public StarGuildRepository(DbContext context) : base(context)
        {
        }

        public StarGuild GetOrCreate(ulong guildId)
        {
            var sg = _set.FirstOrDefault(g => g.GuildId == guildId);
            if (sg == null)
            {
                sg = new StarGuild
                {
                    GuildId = guildId,
                    StarMessages = new HashSet<StarMessage>(new StarMessageEqualityComparer())
                };
                _set.Add(sg);
                _context.SaveChanges();
            }
            else if (sg.StarMessages == null)
            {
                sg.StarMessages = new HashSet<StarMessage>(new StarMessageEqualityComparer());
                var context = _context as NadekoContext;
                var msgs = context.StarMessages.Where(m => m.Guild.GuildId == guildId).ToList();
                foreach (var msg in msgs)
                    sg.StarMessages.Add(new StarMessage { MessageId = msg.MessageId, Guild = sg });
            }
            return sg;
        }

        public HashSet<StarMessage> GetMessages(ulong guildId)
        {
            return GetOrCreate(guildId).StarMessages;
        }

        public bool AddMessage(ulong guildId, ulong messageId)
        {
            var sm = new StarMessage { MessageId = messageId, Guild = GetOrCreate(guildId) };
            var added = GetOrCreate(guildId).StarMessages.Add(sm);
            if (added)
            {
                var context = _context as NadekoContext;
                context.StarMessages.Add(sm);
                _context.SaveChanges();
            }
            return added;
        }

        private class StarMessageEqualityComparer : IEqualityComparer<StarMessage>
        {
            public bool Equals(StarMessage one, StarMessage two)
                => one.MessageId == two.MessageId;
            
            public int GetHashCode(StarMessage message)
                => message.MessageId.GetHashCode();
        }
    }
}
