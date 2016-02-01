using Microsoft.Data.Entity;
using SOCVR.Chatbot.Configuration;
using System;
using System.Linq;
using System.Linq.Expressions;

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
                .HasKey(i => new
                {
                    i.ReviewId,
                    i.ReviewerId
                });

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

        /// <summary>
        /// Checks that there is a user in the databse with this profile Id.
        /// If it does not exist, the user entry will be created.
        /// </summary>
        /// <param name="profileId"></param>
        public void EnsureUserExists(int profileId)
        {
            var dbUser = Users.SingleOrDefault(x => x.ProfileId == profileId);

            if (dbUser == null)
            {
                dbUser = new User() { ProfileId = profileId };
                Users.Add(dbUser);
                SaveChanges();
            }
        }
    }
}
