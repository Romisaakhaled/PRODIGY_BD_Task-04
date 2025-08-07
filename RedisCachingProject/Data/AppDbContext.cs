using Microsoft.EntityFrameworkCore;
using RedisCachingProject.Models;

namespace RedisCachingProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
