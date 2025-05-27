namespace MyWebTelegram.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public Chat Chat { get; set; } = null!;
        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;
        public string Text { get; set; } = null!;
        public string? AttachmentUrl { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
