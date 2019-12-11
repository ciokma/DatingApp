using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options){}
        
        public DbSet<Value> Values { get; set; }
        public DbSet<User> Users {get;set;}

        protected override void OnModelCreating(ModelBuilder  modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Value>();
            modelBuilder.Entity<User>();
            //.ToTable("Values");
        }
        
    }

    
}