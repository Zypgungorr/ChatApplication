namespace ChatApplication.Models
{
    public class GroupMember
    {
        public int Id { get; set; } // Primary Key
        public int UserId { get; set; } // Foreign Key, UserId
        public int GroupId { get; set; } // Foreign Key, GroupId
        public DateTime JoinedAt { get; set; }

        // Navigasyon özellikleri
        public User? User { get; set; } // Kullanıcı
        public Group? Group { get; set; } // Grup
    }
}
