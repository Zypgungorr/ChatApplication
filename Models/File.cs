namespace ChatApplication.Models
{
    public class File
    {
        public int Id { get; set; } // Primary Key
        public int SenderId { get; set; } // Foreign Key, UserId
        public string? FilePath { get; set; }
        public int? GroupId { get; set; } // Foreign Key, GroupId (nullable)

        public DateTime CreatedAt { get; set; }

        // Navigasyon özellikleri
        public User? Sender { get; set; } // Gönderen kullanıcı
        public Group? Group { get; set; } // İlgili grup (nullable)
    }
}
