using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options){}
        
        public DbSet<Value> Values { get; set; }
        public DbSet<User> Users {get;set;}

        public DbSet<Photo> Photos { get; set; }

        protected override void OnModelCreating(ModelBuilder  modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Value>();//.ToTable("Value");
            modelBuilder.Entity<User>();//.ToTable("User");
            modelBuilder.Entity<Photo>();//.ToTable("Photo");
        }
        
    }

    
}