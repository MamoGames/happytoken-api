using HappyTokenApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HappyTokenApi.Data
{
    public class ApiDbContext : DbContext
    {
        public DbSet<DbUser> Users { get; set; }
        
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {
        }
    }
}
