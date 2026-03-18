using CloudContactManager.Models;
using CloudContactManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudContactManager.Data
{
    /// <summary>
    /// Application database context for Entity Framework Core.
    /// </summary>
    public class AppDbContext : DbContext
    {
        private readonly int _currentTenantId;

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            _currentTenantId = 0;
        }

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
            : base(options)
        {
            _currentTenantId = tenantProvider.GetTenantId();
        }

        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Campaign> Campaigns => Set<Campaign>();
        public DbSet<CommunicationLog> CommunicationLogs => Set<CommunicationLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tenant>()
                .HasOne(t => t.Plan)
                .WithMany(p => p.Tenants)
                .HasForeignKey(t => t.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Tenant)
                .WithMany(t => t.Customers)
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Campaign>()
                .HasOne(c => c.Tenant)
                .WithMany(t => t.Campaigns)
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommunicationLog>()
                .HasOne(cl => cl.Campaign)
                .WithMany(c => c.CommunicationLogs)
                .HasForeignKey(cl => cl.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommunicationLog>()
                .HasOne(cl => cl.Customer)
                .WithMany(c => c.CommunicationLogs)
                .HasForeignKey(cl => cl.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasQueryFilter(e => e.TenantId == _currentTenantId);

            modelBuilder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan
                {
                    Id = 1,
                    PlanName = "Free",
                    MaxCustomers = 100,
                    Price = 0m
                });
        }
    }
}
