namespace MyWebTelegram.Models
{
    public class User
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; } = null!; // <- додано
        public string Username { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public List<Message> Messages { get; set; } = new();
    }
}
