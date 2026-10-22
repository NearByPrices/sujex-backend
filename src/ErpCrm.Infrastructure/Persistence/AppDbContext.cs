using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Domain.Entities.CRM;
using ErpCrm.Domain.Entities.Finance;
using ErpCrm.Domain.Entities.Foundation;
using ErpCrm.Domain.Entities.HR;
using ErpCrm.Domain.Entities.Identity;
using ErpCrm.Domain.Entities.Inventory;
using ErpCrm.Domain.Entities.Projects;
using ErpCrm.Domain.Entities.Purchase;
using ErpCrm.Domain.Entities.Sales;
using ErpCrm.Domain.Entities.Platform;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ErpCrm.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    private readonly ITenantContext _tenantContext;

    // Read the tenant from this DbContext instance so EF does not cache a tenant
    // constant in the model shared by all requests.
    public Guid CurrentTenantId => _tenantContext.TenantId;

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

    // CRM
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Activity> Activities => Set<Activity>();

    // Inventory
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    // Sales
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesInvoiceLine> SalesInvoiceLines => Set<SalesInvoiceLine>();
    public DbSet<Payment> Payments => Set<Payment>();

    // Purchase
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

    // HR
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    // Finance
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();

    // Projects
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ErpCrm.Domain.Entities.Projects.Task> ProjectTasks => Set<ErpCrm.Domain.Entities.Projects.Task>();

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
                var tenantIdValue = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
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
