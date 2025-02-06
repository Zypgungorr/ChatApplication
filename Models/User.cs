namespace ChatApplication.Models
{
    public class User
    {
        public int Id { get; set; } // Primary Key
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }

        // Bir kullanıcının mesajları
        public ICollection<Message>? Messages { get; set; }

        // Bir kullanıcının grup üyelikleri
        public ICollection<GroupMember>? GroupMembers { get; set; }
    }
}
