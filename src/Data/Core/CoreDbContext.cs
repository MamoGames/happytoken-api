using HappyTokenApi.Data.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HappyTokenApi.Data.Core
{
    public class CoreDbContext : DbContext
    {
        public DbSet<DbUser> Users { get; set; }

        public DbSet<DbUserProfile> UsersProfiles { get; set; }

        public DbSet<DbUserFriend> UsersFriends { get; set; }

        public DbSet<DbUserWallet> UsersWallets { get; set; }

        public DbSet<DbUserHappiness> UsersHappiness { get; set; }

        public DbSet<DbUserBuilding> UsersBuildings { get; set; }

        public DbSet<DbUserAvatar> UsersAvatars { get; set; }

        public DbSet<DbUserCake> UsersCakes { get; set; }

        public DbSet<DbUserStorePurchaseRecord> UsersStorePurchaseRecords { get; set; }

        public DbSet<DBUserDailyActions> UsersDailyActions { get; set; }

        public DbSet<DbUserMessage> UsersMessages { get; set; }

        public DbSet<DbUserMessagesStatus> UsersMessagesStatus { get; set; }

        public DbSet<DbUserQuest> UsersQuests { get; set; }

        public DbSet<DbUserStat> UsersStats { get; set; }

        public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbUserQuest>()
                .Property(b => b._RequiresValues).HasColumnName("RequiresValues");

            modelBuilder.Entity<DbUserQuest>()
                .Property(b => b._TargetValues).HasColumnName("TargetValues");
        }
    }
}
