using Microsoft.Data.Entity;
using SOCVR.Chatbot.Configuration;
using System;
using System.Linq;

namespace SOCVR.Chatbot.Database
{
    internal class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserReviewedItem> ReviewedItems { get; set; }
        public DbSet<PermissionRequest> PermissionRequests { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<DayMissingReviews> DayMissingReviews { get; set; }

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

            //MissingReviews
            modelBuilder.Entity<DayMissingReviews>()
                .HasKey(dmr => new { dmr.ProfileId, dmr.Date });

            modelBuilder.Entity<DayMissingReviews>()
                .HasOne(dmr => dmr.User)
                .WithMany(u => u.MissingReviewRecords)
                .HasForeignKey(dmr => dmr.ProfileId);
        }

        /// <summary>
        /// Checks that there is a user in the database with this profile Id.
        /// If it does not exist, the user entry will be created.
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="setOptIn">If true, the user will be opted in to user tracking.</param>
        public void EnsureUserExists(int profileId, bool setOptIn = false)
        {
            var dbUser = Users.SingleOrDefault(x => x.ProfileId == profileId);

            if (dbUser == null)
            {
                dbUser = new User() { ProfileId = profileId };

                if (setOptIn)
                {
                    dbUser.LastTrackingPreferenceChange = DateTimeOffset.Now;
                    dbUser.OptInToReviewTracking = true;
                }

                Users.Add(dbUser);
                SaveChanges();
            }
        }

        public void EnsureUserIsInAllPermissionGroups(int profileId)
        {
            var dbUser = Users
                .Include(x => x.Permissions)
                .SingleOrDefault(x => x.ProfileId == profileId);

            var permissionsUserCurrentlyHas = dbUser
                .Permissions
                .Select(x => x.PermissionGroup);

            var allPermissionGroups = Enum
                .GetValues(typeof(PermissionGroup))
                .OfType<PermissionGroup>();

            var missingPermissionGroups = allPermissionGroups.Except(permissionsUserCurrentlyHas);

            foreach (var group in missingPermissionGroups)
            {
                dbUser.Permissions.Add(new UserPermission
                {
                    JoinedOn = DateTimeOffset.Now,
                    PermissionGroup = group,
                });
            }

            SaveChanges();
        }
    }
}
