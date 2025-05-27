namespace MyWebTelegram.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string? ChatName { get; set; } // null = приватний чат
        public bool IsGroup { get; set; }
        public List<ChatUser> ChatUsers { get; set; } = new();
        public List<Message> Messages { get; set; } = new();
    }
}
