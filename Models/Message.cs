namespace ChatApplication.Models
{
    public class Message
    {
        public int Id { get; set; } // Primary Key
        public int SenderId { get; set; } // Foreign Key, UserId
        public int? GroupId { get; set; } // Foreign Key, GroupId (nullable)
         public int? ReceiverId { get; set; } // Özel mesaj için (grup değilse)

        public string? Text { get; set; }
        public DateTime CreatedAt { get; set; }

      
        public User? Sender { get; set; } // Bir Mesajın bir Göndereni (User) vardır.
           public User? Receiver { get; set; } // Eğer birebir mesajsa, alıcı kullanıcı
        public Group? Group { get; set; } // Bir Mesajın ait olduğu Grup olabilir.
    }
}
