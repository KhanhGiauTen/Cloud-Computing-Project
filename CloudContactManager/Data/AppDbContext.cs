using CloudContactManager.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudContactManager.Data
{
    /// <summary>
    /// Application database context for Entity Framework Core.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TODO: Configure entity relationships and constraints
        }
    }
}
