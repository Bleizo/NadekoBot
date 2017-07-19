using NadekoBot.Services.Database.Models;
using System.Collections.Generic;

namespace NadekoBot.Services.Database.Repositories
{
    public interface IStarGuildRepository : IRepository<StarGuild>
    {
        StarGuild GetOrCreate(ulong guildId);
        HashSet<StarMessage> GetMessages(ulong guildId);
        bool AddMessage(ulong guildId, ulong messageId);
    }
}
