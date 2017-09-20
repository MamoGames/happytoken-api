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

        public DbSet<DbUserCakes> UsersCakes { get; set; }

        public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options) { }
    }
}
