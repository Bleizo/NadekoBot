namespace NadekoBot.Services.Database.Models
{
    public class StarMessage : DbEntity
    {
        public ulong MessageId { get; set; }
        public StarGuild Guild { get; set; }
    }
}
