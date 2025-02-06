using Microsoft.EntityFrameworkCore;

namespace ChatApplication.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }

        // Fluent API kullanarak ilişkiler
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User ve Message arasındaki ilişki (Bir User bir çok mesaj gönderebilir)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)  // Message ve Sender(User) arasındaki ilişki
                .WithMany(u => u.Messages)  // User ve Messages arasındaki ilişki
                .HasForeignKey(m => m.SenderId)  // Message tablosunda ForeignKey olarak SenderId
                .OnDelete(DeleteBehavior.Restrict);  // Eğer kullanıcı silinirse, mesaj silinmesin.

            // Message ve Group arasındaki ilişki (Bir grup bir çok mesaj alabilir)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Group)  // Message ve Group arasındaki ilişki
                .WithMany(g => g.Messages)  // Group ve Messages arasındaki ilişki
                .HasForeignKey(m => m.GroupId)  // Message tablosunda ForeignKey olarak GroupId
                .OnDelete(DeleteBehavior.SetNull); // Eğer grup silinirse, mesajın grubu null olur.

            // Group ve GroupMember arasındaki ilişki (Bir grup bir çok üyeye sahip olabilir)
            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)  // GroupMember ve User arasındaki ilişki
                .WithMany(u => u.GroupMembers)  // User ve GroupMembers arasındaki ilişki
                .HasForeignKey(gm => gm.UserId);  // GroupMember tablosunda ForeignKey olarak UserId

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)  // GroupMember ve Group arasındaki ilişki
                .WithMany(g => g.Members)  // Group ve Members arasındaki ilişki
                .HasForeignKey(gm => gm.GroupId);  // GroupMember tablosunda ForeignKey olarak GroupId
        }
    }
}
