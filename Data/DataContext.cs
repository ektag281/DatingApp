using DatingApp.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Api.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> Likes { get; set; }

        //Configuration with user to like table
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //Creating primary key on Like Table
            builder.Entity<UserLike>()
            .HasKey(c => new { c.SourceUserId, c.LikedUserId});

            //Reltionship between user and like table
            builder.Entity<UserLike>()
            .HasOne(s => s.SourceUser)
            .WithMany(l => l.LikedUsers) //Here we are saying source user can like many user
            .HasForeignKey(s => s.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);

            //Reltionship between user and like table
            builder.Entity<UserLike>()
            .HasOne(s => s.LikedUser)
            .WithMany(l => l.LikedByUsers) //Here we are saying source user can like many user
            .HasForeignKey(s => s.LikedUserId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}