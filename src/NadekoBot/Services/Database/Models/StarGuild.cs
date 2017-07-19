using System.Collections.Generic;

namespace NadekoBot.Services.Database.Models
{
    public class StarGuild : DbEntity
    {
        public ulong GuildId { get; set; }
        public ulong? ChannelId { get; set; }
        public int Required { get; set; } = 5;
        public HashSet<StarMessage> StarMessages { get; set; }
    }
}
