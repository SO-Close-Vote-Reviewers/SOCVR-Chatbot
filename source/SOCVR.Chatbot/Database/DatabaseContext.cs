using Microsoft.Data.Entity;
using SOCVR.Chatbot.Configuration;

namespace SOCVR.Chatbot.Database
{
    internal class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserReviewedItem> ReviewedItems { get; set; }
        public DbSet<PermissionRequest> PermissionRequests { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(ConfigurationAccessor.DatabaseConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //permission request
            modelBuilder.Entity<PermissionRequest>()
                .HasOne(pr => pr.RequestingUser)
                .WithMany(u => u.PermissionsRequested)
                .HasForeignKey(pr => pr.RequestingUserId);

            modelBuilder.Entity<PermissionRequest>()
                .HasOne(pr => pr.ReviewingUser)
                .WithMany(u => u.PermissionsReviewed)
                .HasForeignKey(pr => pr.ReviewingUserId);

            //user reviewed item
            modelBuilder.Entity<UserReviewedItem>()
                .HasOne(i => i.Reviewer)
                .WithMany(u => u.ReviewedItems)
                .HasForeignKey(i => i.ReviewerId)
                .IsRequired();

            modelBuilder.Entity<UserReviewedItem>()
                .Property(i => i.PrimaryTag)
                .IsRequired();

            //user permission
            modelBuilder.Entity<UserPermission>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<UserPermission>()
                .HasOne(p => p.User)
                .WithMany(u => u.Permissions)
                .HasForeignKey(p => p.UserId)
                .IsRequired();

            //user
            modelBuilder.Entity<User>()
                .HasKey(u => u.ProfileId);
        }
    }
}
