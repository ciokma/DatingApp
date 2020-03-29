using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options){}
        
        public DbSet<Value> Values { get; set; }
        public DbSet<User> Users {get;set;}
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder  modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Value>();//.ToTable("Value");
            modelBuilder.Entity<User>();//.ToTable("User");
            modelBuilder.Entity<Photo>();//.ToTable("Photo");
            modelBuilder.Entity<Like>()//.ToTable("Like");
                .HasKey(k => new { k.LikerId, k.LikeeId});
   

            modelBuilder.Entity<Like>()
                .HasOne(u => u.Likee)
                .WithMany(u =>u.Likers)
                .HasForeignKey(u => u.LikeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Like>()
                .HasOne(u => u.Liker)
                .WithMany(u =>u.Likees)
                .HasForeignKey(u => u.LikerId)
                .OnDelete(DeleteBehavior.Restrict);

             modelBuilder.Entity<Message>()
                .HasOne(u => u.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);
                
             modelBuilder.Entity<Message>()
                .HasOne(u => u.Recipient)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);
        }
        
    }

    
}