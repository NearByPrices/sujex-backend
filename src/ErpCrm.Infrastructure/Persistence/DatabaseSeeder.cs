using ErpCrm.Domain.Entities.Foundation;
using ErpCrm.Domain.Entities.Identity;
using ErpCrm.Domain.Entities.Platform;
using ErpCrm.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ErpCrm.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            logger.LogInformation("Applying database migrations...");
            await db.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying migrations.");
            throw;
        }

        await SeedRolesAsync(roleManager, logger);
        await SeedPlatformAdminAsync(userManager, logger);
        await SeedCurrenciesAsync(db, logger);
        await SeedSubscriptionPlansAsync(db, logger);
    }

    private static async Task SeedRolesAsync(RoleManager<AppRole> roleManager, ILogger logger)
    {
        string[] roles = { "PlatformAdmin", "Admin", "Manager", "User" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole { Name = roleName });
                logger.LogInformation("Created role: {Role}", roleName);
            }
        }
    }

    private static async Task SeedPlatformAdminAsync(UserManager<AppUser> userManager, ILogger logger)
    {
        const string email = "admin@sujex.io";
        var existing = await userManager.FindByEmailAsync(email);
        if (existing != null)
        {
            logger.LogInformation("Platform admin already exists, skipping.");
            return;
        }

        var admin = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = "Platform",
            LastName = "Admin",
            IsPlatformAdmin = true,
            IsActive = true,
            TenantId = Guid.Empty
        };

        var result = await userManager.CreateAsync(admin, "Sujex@Admin123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "PlatformAdmin");
            logger.LogInformation("Platform admin created: {Email}", email);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to create platform admin: {Errors}", errors);
        }
    }

    private static async Task SeedCurrenciesAsync(AppDbContext db, ILogger logger)
    {
        if (await db.Currencies.AnyAsync())
        {
            logger.LogInformation("Currencies already seeded, skipping.");
            return;
        }

        var currencies = new List<Currency>
        {
            new() { Code = "USD", Name = "US Dollar",     Symbol = "$",    IsDefault = true },
            new() { Code = "EUR", Name = "Euro",           Symbol = "€" },
            new() { Code = "GBP", Name = "British Pound",  Symbol = "£" },
            new() { Code = "INR", Name = "Indian Rupee",   Symbol = "₹" },
            new() { Code = "AED", Name = "UAE Dirham",     Symbol = "د.إ" }
        };

        db.Currencies.AddRange(currencies);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} currencies.", currencies.Count);
    }

    private static async Task SeedSubscriptionPlansAsync(AppDbContext db, ILogger logger)
    {
        if (await db.SubscriptionPlans.AnyAsync())
        {
            logger.LogInformation("Subscription plans already seeded, skipping.");
            return;
        }

        var allModules = Enum.GetValues<ModuleCode>();

        var starterPlan = new SubscriptionPlan
        {
            Name = "Starter",
            Description = "Full access to all modules at no cost",
            Price = 0,
            BillingCycle = BillingCycle.Monthly,
            MaxUsers = 10,
            MaxCompanies = 1,
            IsActive = true
        };

        db.SubscriptionPlans.Add(starterPlan);
        await db.SaveChangesAsync();

        foreach (var module in allModules)
        {
            db.PlanModules.Add(new PlanModule
            {
                PlanId = starterPlan.Id,
                ModuleCode = module,
                IsIncluded = true
            });
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded Starter plan with {Count} modules.", allModules.Length);
    }
}
