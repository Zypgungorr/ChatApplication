namespace ChatApplication.Models
{
    public class Group
    {
        public int Id { get; set; } // Primary Key
        public string? GroupName { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigasyon özelliği
        public ICollection<Message>? Messages { get; set; } // Bir grup bir çok mesaja sahip olabilir.
        public ICollection<GroupMember>? Members { get; set; } // Bir grup bir çok üye (User) içerebilir.
    }
}
