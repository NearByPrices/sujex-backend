using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Domain.Entities.Foundation;
using ErpCrm.Domain.Entities.Identity;
using ErpCrm.Domain.Entities.Platform;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ErpCrm.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    // Platform
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<PlanModule> PlanModules => Set<PlanModule>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<TenantModule> TenantModules => Set<TenantModule>();
    public DbSet<BillingInvoice> BillingInvoices => Set<BillingInvoice>();

    // Identity
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserCompanyRole> UserCompanyRoles => Set<UserCompanyRole>();

    // Foundation
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<CustomFieldDefinition> CustomFieldDefinitions => Set<CustomFieldDefinition>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Rename Identity tables to snake_case
        builder.Entity<AppUser>().ToTable("users");
        builder.Entity<AppRole>().ToTable("roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>().ToTable("user_roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>().ToTable("role_claims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>().ToTable("user_tokens");

        // Global query filters for multi-tenant isolation
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (typeof(ErpCrm.Domain.Common.TenantEntity).IsAssignableFrom(clrType)
                && !clrType.IsAbstract)
            {
                var parameter = Expression.Parameter(clrType, "e");
                var tenantIdProperty = Expression.Property(parameter, nameof(ErpCrm.Domain.Common.TenantEntity.TenantId));
                var tenantIdValue = Expression.Constant(_tenantContext.TenantId);
                var filter = Expression.Lambda(Expression.Equal(tenantIdProperty, tenantIdValue), parameter);
                entityType.SetQueryFilter(filter);
            }
        }

        // Set numeric(18,4) for all decimal properties
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    property.SetColumnType("numeric(18,4)");
                }
            }
        }

        builder.Entity<Tenant>(e => e.HasIndex(t => t.Slug).IsUnique());
        builder.Entity<TenantModule>(e => e.HasIndex(t => new { t.TenantId, t.ModuleCode }).IsUnique());
        builder.Entity<RolePermission>(e => e.HasIndex(r => new { r.RoleId, r.PermissionId }).IsUnique());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<ErpCrm.Domain.Common.BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    if (_tenantContext.UserId != Guid.Empty)
                        entry.Entity.CreatedBy = _tenantContext.UserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    if (_tenantContext.UserId != Guid.Empty)
                        entry.Entity.UpdatedBy = _tenantContext.UserId;
                    break;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
