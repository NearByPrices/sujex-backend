using ErpCrm.Domain.Entities.Foundation;
using ErpCrm.Domain.Entities.Identity;
using ErpCrm.Domain.Entities.Platform;
using ErpCrm.Domain.Entities.CRM;
using ErpCrm.Domain.Entities.Finance;
using ErpCrm.Domain.Entities.HR;
using ErpCrm.Domain.Entities.Inventory;
using ErpCrm.Domain.Entities.Purchase;
using ErpCrm.Domain.Entities.Sales;
using ErpCrm.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Project = ErpCrm.Domain.Entities.Projects.Project;

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
            logger.LogError(ex, "Database migration failed — API will start but DB may be unavailable.");
            return;
        }

        await SeedRolesAsync(roleManager, logger);
        await SeedPlatformAdminAsync(userManager, logger);
        await SeedCurrenciesAsync(db, logger);
        await SeedSubscriptionPlansAsync(db, logger);
        await SeedDemoTenantAsync(db, userManager, logger);
    }

    private static async Task SeedRolesAsync(RoleManager<AppRole> roleManager, ILogger logger)
    {
        string[] roles = { "PlatformAdmin", "Admin", "Manager", "User" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole
                {
                    Name = roleName,
                    IsSystemRole = true,
                    TenantId = null
                });
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

    private static async Task SeedDemoTenantAsync(
        AppDbContext db,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        const string slug = "demo-technology";
        var existingTenant = await db.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Slug == slug);
        if (existingTenant != null)
        {
            await EnsureDemoTransactionDetailsAsync(db, userManager, existingTenant.Id, logger);
            return;
        }

        var tenantId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a001");
        var companyId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a002");
        var branchId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a003");
        var contactId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a004");
        var secondContactId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a005");
        var categoryId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a006");
        var unitId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a007");
        var productId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a008");
        var secondProductId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a009");
        var warehouseId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a00a");
        var vendorId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a00b");
        var departmentId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a00c");
        var designationId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a00d");
        var employeeId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a00e");
        var leaveTypeId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a00f");
        var accountCashId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a010");
        var accountRevenueId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a011");
        var projectId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a012");
        var leadId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a013");
        var opportunityId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a014");
        var activityId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a015");
        var salesOrderId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a016");
        var invoiceId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a017");
        var purchaseOrderId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a018");

        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "Demo Technology Pvt Ltd",
            Slug = slug,
            ContactEmail = "hello@demo-technology.test",
            ContactPhone = "+91 98765 43210",
            MaxUsers = 25,
            MaxCompanies = 2,
            Status = TenantStatus.Active
        };
        db.Tenants.Add(tenant);

        var plan = await db.SubscriptionPlans.IgnoreQueryFilters()
            .FirstAsync(p => p.Name == "Starter");
        db.TenantSubscriptions.Add(new TenantSubscription
        {
            TenantId = tenantId,
            PlanId = plan.Id,
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow.AddMonths(11),
            Status = SubscriptionStatus.Active,
            BillingAmount = 0,
            Notes = "Demo subscription"
        });
        db.TenantModules.AddRange(Enum.GetValues<ModuleCode>().Select(module => new TenantModule
        {
            TenantId = tenantId,
            ModuleCode = module,
            IsEnabled = true
        }));

        var demoUser = await userManager.FindByEmailAsync("demo@demo-technology.test");
        if (demoUser == null)
        {
            demoUser = new AppUser
            {
                UserName = "demo@demo-technology.test",
                Email = "demo@demo-technology.test",
                EmailConfirmed = true,
                FirstName = "Demo",
                LastName = "Admin",
                TenantId = tenantId,
                IsActive = true
            };
            var userResult = await userManager.CreateAsync(demoUser, "Demo@12345");
            if (!userResult.Succeeded)
            {
                var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create demo user: {errors}");
            }
            await userManager.AddToRoleAsync(demoUser, "Admin");
        }

        var adminRole = await db.Roles.IgnoreQueryFilters().FirstAsync(r => r.Name == "Admin");
        db.UserCompanyRoles.Add(new UserCompanyRole
        {
            UserId = demoUser.Id,
            CompanyId = companyId,
            RoleId = adminRole.Id
        });

        db.Companies.Add(new Company
        {
            Id = companyId,
            TenantId = tenantId,
            Name = "Demo Technology Pvt Ltd",
            TaxId = "GST-DEMO-2025",
            Website = "https://demo-technology.test",
            Phone = "+91 98765 43210",
            Email = "hello@demo-technology.test",
            AddressLine1 = "42 Innovation Park",
            City = "Bengaluru",
            State = "Karnataka",
            Country = "India",
            PostalCode = "560001",
            BaseCurrency = "INR",
            Timezone = "Asia/Kolkata"
        });
        db.Branches.Add(new Branch
        {
            Id = branchId,
            TenantId = tenantId,
            CompanyId = companyId,
            Name = "Bengaluru Head Office",
            Code = "BLR-HQ",
            Phone = "+91 80400 11000",
            Email = "blr@demo-technology.test",
            AddressLine1 = "42 Innovation Park",
            City = "Bengaluru",
            State = "Karnataka",
            Country = "India",
            PostalCode = "560001",
            IsHeadOffice = true
        });

        db.Contacts.AddRange(
            new Contact
            {
                Id = contactId,
                TenantId = tenantId,
                CompanyId = companyId,
                FirstName = "Aarav",
                LastName = "Sharma",
                Email = "aarav.sharma@example.test",
                Phone = "+91 90000 10001",
                Mobile = "+91 98888 10001",
                JobTitle = "IT Director",
                Department = "Technology",
                AccountName = "Northwind Labs",
                Notes = "Primary decision maker for the ERP rollout.",
                Tags = "enterprise,priority",
                IsActive = true
            },
            new Contact
            {
                Id = secondContactId,
                TenantId = tenantId,
                CompanyId = companyId,
                FirstName = "Maya",
                LastName = "Patel",
                Email = "maya.patel@example.test",
                Phone = "+91 90000 10002",
                Mobile = "+91 98888 10002",
                JobTitle = "Operations Manager",
                Department = "Operations",
                AccountName = "Acme Retail",
                Notes = "Interested in inventory and purchase automation.",
                Tags = "retail,inventory",
                IsActive = true
            });
        db.Leads.Add(new Lead
        {
            Id = leadId,
            TenantId = tenantId,
            ContactId = contactId,
            Title = "Northwind ERP rollout",
            FirstName = "Aarav",
            LastName = "Sharma",
            Email = "aarav.sharma@example.test",
            Company = "Northwind Labs",
            Phone = "+91 90000 10001",
            JobTitle = "IT Director",
            Status = LeadStatusEnum.Qualified,
            Source = "Website",
            EstimatedValue = 850000,
            AssignedToUserId = demoUser.Id,
            Notes = "Budget approved; send implementation plan after discovery.",
            Tags = "enterprise,erp",
            FollowUpDate = DateTime.UtcNow.AddDays(5)
        });
        db.Opportunities.Add(new Opportunity
        {
            Id = opportunityId,
            TenantId = tenantId,
            ContactId = contactId,
            LeadId = leadId,
            Title = "Northwind annual subscription",
            Status = OpportunityStatus.Open,
            ExpectedValue = 1200000,
            Probability = 70,
            ExpectedCloseDate = DateTime.UtcNow.AddDays(30),
            Stage = "Proposal",
            AssignedToUserId = demoUser.Id,
            Notes = "Proposal includes finance, inventory, CRM, and reporting modules."
        });
        db.Activities.Add(new Activity
        {
            Id = activityId,
            TenantId = tenantId,
            Type = "Meeting",
            Subject = "Product discovery call",
            Description = "Review finance and inventory workflows with the prospect.",
            DueDate = DateTime.UtcNow.AddDays(2),
            AssignedToUserId = demoUser.Id,
            ContactId = contactId,
            OpportunityId = opportunityId
        });

        db.ProductCategories.Add(new ProductCategory
        {
            Id = categoryId,
            TenantId = tenantId,
            Name = "SaaS Hardware",
            Code = "SAAS-HW",
            Description = "Demo equipment and accessories"
        });
        db.Units.Add(new Unit { Id = unitId, TenantId = tenantId, Name = "Piece", Symbol = "pcs" });
        db.Products.AddRange(
            new Product
            {
                Id = productId,
                TenantId = tenantId,
                Name = "Business Laptop Pro",
                Code = "LAPTOP-PRO",
                Sku = "SUX-LAP-001",
                Barcode = "8901234567001",
                Description = "14-inch business laptop with 16GB RAM, 512GB SSD, and 3-year support.",
                CategoryId = categoryId,
                UnitId = unitId,
                CostPrice = 72000,
                SalePrice = 89900,
                MinStockLevel = 5,
                ReorderPoint = 10,
                MaxStockLevel = 100,
                Tags = "hardware,bestseller"
            },
            new Product
            {
                Id = secondProductId,
                TenantId = tenantId,
                Name = "Cloud Setup Service",
                Code = "CLOUD-SETUP",
                Sku = "SUX-SVC-001",
                Barcode = "8901234567002",
                Description = "Professional cloud setup, migration, and onboarding service.",
                CategoryId = categoryId,
                UnitId = unitId,
                CostPrice = 0,
                SalePrice = 25000,
                IsService = true,
                Tags = "service,cloud"
            });
        db.Warehouses.Add(new Warehouse
        {
            Id = warehouseId,
            TenantId = tenantId,
            Name = "Bengaluru Central Warehouse",
            Code = "BLR-WH",
            BranchId = branchId,
            Address = "42 Innovation Park, Bengaluru",
            IsActive = true
        });
        db.InventoryItems.AddRange(
            new InventoryItem { TenantId = tenantId, ProductId = productId, WarehouseId = warehouseId, QuantityOnHand = 42, QuantityReserved = 8, AverageCost = 72000 },
            new InventoryItem { TenantId = tenantId, ProductId = secondProductId, WarehouseId = warehouseId, QuantityOnHand = 100, QuantityReserved = 12, AverageCost = 0 });
        db.StockMovements.Add(new StockMovement
        {
            TenantId = tenantId,
            ProductId = productId,
            WarehouseId = warehouseId,
            MovementType = "IN",
            Quantity = 50,
            UnitCost = 72000,
            Reference = "GRN-DEMO-001",
            MovementDate = DateTime.UtcNow.AddDays(-7)
        });

        db.Vendors.Add(new Vendor
        {
            Id = vendorId,
            TenantId = tenantId,
            Name = "TechSource India",
            Code = "VND-001",
            Email = "sales@techsource.example.test",
            Phone = "+91 90000 20001",
            Address = "18 Electronics City, Bengaluru, Karnataka 560100",
            TaxNumber = "29AABCT1234A1Z5",
            CurrencyCode = "INR",
            PaymentTermsDays = 30,
            CreditLimit = 500000,
            Notes = "Preferred supplier for laptops and network equipment.",
            IsActive = true
        });
        db.PurchaseOrders.Add(new PurchaseOrder
        {
            Id = purchaseOrderId,
            TenantId = tenantId,
            OrderNumber = "PO-2025-0001",
            VendorId = vendorId,
            Status = PurchaseOrderStatus.Sent,
            OrderDate = DateTime.UtcNow.AddDays(-4),
            ExpectedDeliveryDate = DateTime.UtcNow.AddDays(6),
            CurrencyCode = "INR",
            SubTotal = 360000,
            TaxAmount = 64800,
            TotalAmount = 424800,
            WarehouseId = warehouseId,
            Notes = "Demo purchase order for Q4 replenishment.",
            ApprovalStatus = ApprovalStatus.Approved,
            ApprovedByUserId = demoUser.Id,
            ApprovedAt = DateTime.UtcNow.AddDays(-3)
        });
        db.SalesOrders.Add(new SalesOrder
        {
            Id = salesOrderId,
            TenantId = tenantId,
            OrderNumber = "SO-2025-0001",
            ContactId = contactId,
            CompanyId = companyId,
            Status = SalesOrderStatus.Confirmed,
            OrderDate = DateTime.UtcNow.AddDays(-2),
            DeliveryDate = DateTime.UtcNow.AddDays(5),
            CurrencyCode = "INR",
            SubTotal = 204800,
            TaxAmount = 36864,
            TotalAmount = 241664,
            AssignedToUserId = demoUser.Id,
            BillingAddress = "42 Innovation Park, Bengaluru, Karnataka 560001",
            ShippingAddress = "Bengaluru Central Warehouse, 42 Innovation Park",
            DiscountAmount = 0,
            Notes = "Demo confirmed order for Northwind Labs."
        });
        db.SalesInvoices.Add(new SalesInvoice
        {
            Id = invoiceId,
            TenantId = tenantId,
            InvoiceNumber = "INV-2025-0001",
            SalesOrderId = salesOrderId,
            ContactId = contactId,
            CompanyId = companyId,
            Status = InvoiceStatus.Sent,
            InvoiceDate = DateTime.UtcNow.AddDays(-1),
            DueDate = DateTime.UtcNow.AddDays(29),
            CurrencyCode = "INR",
            SubTotal = 204800,
            TaxAmount = 36864,
            TotalAmount = 241664,
            PaidAmount = 50000,
            BillingAddress = "42 Innovation Park, Bengaluru, Karnataka 560001",
            Notes = "Partially paid by bank transfer; balance due in 29 days."
        });

        db.Departments.Add(new Department { Id = departmentId, TenantId = tenantId, CompanyId = companyId, Name = "Engineering", Code = "ENG" });
        db.Designations.Add(new Designation { Id = designationId, TenantId = tenantId, DepartmentId = departmentId, Name = "Senior Engineer" });
        db.Employees.Add(new Employee
        {
            Id = employeeId,
            TenantId = tenantId,
            EmployeeCode = "EMP-0001",
            FirstName = "Rohan",
            LastName = "Mehta",
            Email = "rohan.mehta@demo-technology.test",
            Phone = "+91 90000 30001",
            DepartmentId = departmentId,
            DesignationId = designationId,
            CompanyId = companyId,
            DateOfBirth = new DateTime(1992, 7, 15),
            Gender = "Male",
            NationalId = "DEMO-NID-0001",
            JoinDate = DateTime.UtcNow.AddYears(-2),
            Status = EmployeeStatus.Active,
            Address = "17 Residency Road, Bengaluru, Karnataka 560025",
            BasicSalary = 125000,
            BankAccount = "DEMO-BANK-0001"
        });
        db.LeaveTypes.Add(new LeaveType { Id = leaveTypeId, TenantId = tenantId, Name = "Annual Leave", Code = "AL", DaysAllowed = 24, IsCarryForward = true, MaxCarryForwardDays = 8, IsPaid = true, IsActive = true });
        db.LeaveRequests.Add(new LeaveRequest
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId,
            StartDate = DateTime.UtcNow.AddDays(14),
            EndDate = DateTime.UtcNow.AddDays(16),
            TotalDays = 3,
            Reason = "Family event",
            Status = ApprovalStatus.Pending
        });

        db.SalesOrderLines.AddRange(
            new SalesOrderLine { TenantId = tenantId, SalesOrderId = salesOrderId, ProductId = productId, Description = "Business Laptop Pro", Quantity = 2, UnitPrice = 89900, DiscountPercent = 0, TaxPercent = 18, SortOrder = 1 },
            new SalesOrderLine { TenantId = tenantId, SalesOrderId = salesOrderId, ProductId = secondProductId, Description = "Cloud Setup Service", Quantity = 1, UnitPrice = 25000, DiscountPercent = 0, TaxPercent = 18, SortOrder = 2 });
        db.SalesInvoiceLines.AddRange(
            new SalesInvoiceLine { TenantId = tenantId, SalesInvoiceId = invoiceId, ProductId = productId, Description = "Business Laptop Pro", Quantity = 2, UnitPrice = 89900, DiscountPercent = 0, TaxPercent = 18, SortOrder = 1 },
            new SalesInvoiceLine { TenantId = tenantId, SalesInvoiceId = invoiceId, ProductId = secondProductId, Description = "Cloud Setup Service", Quantity = 1, UnitPrice = 25000, DiscountPercent = 0, TaxPercent = 18, SortOrder = 2 });
        db.PurchaseOrderLines.AddRange(
            new PurchaseOrderLine { TenantId = tenantId, PurchaseOrderId = purchaseOrderId, ProductId = productId, Description = "Business Laptop Pro", Quantity = 5, ReceivedQuantity = 3, UnitCost = 72000, TaxPercent = 18, SortOrder = 1 },
            new PurchaseOrderLine { TenantId = tenantId, PurchaseOrderId = purchaseOrderId, ProductId = secondProductId, Description = "Cloud Setup Service", Quantity = 2, ReceivedQuantity = 2, UnitCost = 0, TaxPercent = 18, SortOrder = 2 });
        db.Payments.Add(new Payment
        {
            TenantId = tenantId,
            PaymentNumber = "PAY-2025-0001",
            SalesInvoiceId = invoiceId,
            ContactId = contactId,
            Amount = 50000,
            PaymentMode = PaymentMode.BankTransfer,
            PaymentDate = DateTime.UtcNow.AddDays(-1),
            Reference = "UTR-DEMO-0001",
            Notes = "Advance payment received from Northwind Labs."
        });

        db.Accounts.AddRange(
            new Account { Id = accountCashId, TenantId = tenantId, Name = "Cash and Bank", Code = "1000", AccountType = "Asset", Balance = 250000 },
            new Account { Id = accountRevenueId, TenantId = tenantId, Name = "Sales Revenue", Code = "4000", AccountType = "Income", Balance = 950000 });
        var journalId = Guid.Parse("2f3f6f9a-0f1d-4f45-9d51-5f8f9de1a019");
        db.JournalEntries.Add(new JournalEntry
        {
            Id = journalId,
            TenantId = tenantId,
            EntryNumber = "JE-2025-0001",
            EntryDate = DateTime.UtcNow.AddDays(-1),
            Description = "Demo customer payment",
            Reference = "INV-2025-0001",
            IsPosted = true,
            PostedAt = DateTime.UtcNow.AddDays(-1),
            PostedByUserId = demoUser.Id,
            TotalDebit = 50000,
            TotalCredit = 50000,
            Lines =
            {
                new JournalLine { TenantId = tenantId, AccountId = accountCashId, Description = "Payment received", Debit = 50000, SortOrder = 1 },
                new JournalLine { TenantId = tenantId, AccountId = accountRevenueId, Description = "Payment received", Credit = 50000, SortOrder = 2 }
            }
        });

        db.Projects.Add(new Project
        {
            Id = projectId,
            TenantId = tenantId,
            CompanyId = companyId,
            ContactId = contactId,
            Name = "Northwind ERP Implementation",
            Code = "PRJ-001",
            Description = "Demo implementation project for the Northwind account.",
            Status = "Active",
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(80),
            Budget = 850000,
            ManagerId = demoUser.Id,
            Tags = "implementation,priority"
        });
        db.ProjectTasks.AddRange(
            new ErpCrm.Domain.Entities.Projects.Task
            {
                TenantId = tenantId,
                ProjectId = projectId,
                Title = "Discovery workshop",
                Description = "Capture current finance and inventory processes.",
                Status = "Done",
                Priority = "High",
                AssignedToUserId = demoUser.Id,
                CompletedAt = DateTime.UtcNow.AddDays(-3),
                EstimatedHours = 8,
                ActualHours = 7
            },
            new ErpCrm.Domain.Entities.Projects.Task
            {
                TenantId = tenantId,
                ProjectId = projectId,
                Title = "Configure inventory module",
                Description = "Set up warehouses, products, and stock rules.",
                Status = "InProgress",
                Priority = "Medium",
                AssignedToUserId = demoUser.Id,
                DueDate = DateTime.UtcNow.AddDays(12),
                EstimatedHours = 16
            });

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded demo tenant and sample data for {Tenant}.", tenant.Name);
    }

    /// <summary>
    /// Backfills the transactional demo records when the database already has
    /// the original demo tenant. This keeps repeated local starts safe while
    /// making an existing demo database as useful as a fresh one.
    /// </summary>
    private static async Task EnsureDemoTransactionDetailsAsync(
        AppDbContext db,
        UserManager<AppUser> userManager,
        Guid tenantId,
        ILogger logger)
    {
        var user = await userManager.FindByEmailAsync("demo@demo-technology.test");
        var contact = await db.Contacts.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.TenantId == tenantId);
        var product = await db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsService);
        var service = await db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.IsService);
        var warehouse = await db.Warehouses.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.TenantId == tenantId);
        var salesOrder = await db.SalesOrders.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.TenantId == tenantId);
        var invoice = await db.SalesInvoices.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.TenantId == tenantId);
        var purchaseOrder = await db.PurchaseOrders.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.TenantId == tenantId);
        if (user == null || contact == null || product == null || warehouse == null)
        {
            logger.LogWarning("Demo tenant exists but is incomplete; run the seed against a clean database to rebuild it.");
            return;
        }

        if (service != null && salesOrder != null && !await db.SalesOrderLines.IgnoreQueryFilters().AnyAsync(x => x.SalesOrderId == salesOrder.Id))
        {
            db.SalesOrderLines.AddRange(
                new SalesOrderLine { TenantId = tenantId, SalesOrderId = salesOrder.Id, ProductId = product.Id, Description = product.Name, Quantity = 2, UnitPrice = product.SalePrice, TaxPercent = 18, SortOrder = 1 },
                new SalesOrderLine { TenantId = tenantId, SalesOrderId = salesOrder.Id, ProductId = service.Id, Description = service.Name, Quantity = 1, UnitPrice = service.SalePrice, TaxPercent = 18, SortOrder = 2 });
        }

        if (service != null && invoice != null && !await db.SalesInvoiceLines.IgnoreQueryFilters().AnyAsync(x => x.SalesInvoiceId == invoice.Id))
        {
            db.SalesInvoiceLines.AddRange(
                new SalesInvoiceLine { TenantId = tenantId, SalesInvoiceId = invoice.Id, ProductId = product.Id, Description = product.Name, Quantity = 2, UnitPrice = product.SalePrice, TaxPercent = 18, SortOrder = 1 },
                new SalesInvoiceLine { TenantId = tenantId, SalesInvoiceId = invoice.Id, ProductId = service.Id, Description = service.Name, Quantity = 1, UnitPrice = service.SalePrice, TaxPercent = 18, SortOrder = 2 });
        }

        if (purchaseOrder != null && !await db.PurchaseOrderLines.IgnoreQueryFilters().AnyAsync(x => x.PurchaseOrderId == purchaseOrder.Id))
        {
            db.PurchaseOrderLines.Add(new PurchaseOrderLine { TenantId = tenantId, PurchaseOrderId = purchaseOrder.Id, ProductId = product.Id, Description = product.Name, Quantity = 5, ReceivedQuantity = 3, UnitCost = product.CostPrice, TaxPercent = 18, SortOrder = 1 });
        }

        if (salesOrder != null)
        {
            salesOrder.SubTotal = 204800;
            salesOrder.TaxAmount = 36864;
            salesOrder.TotalAmount = 241664;
        }
        if (invoice != null)
        {
            invoice.SubTotal = 204800;
            invoice.TaxAmount = 36864;
            invoice.TotalAmount = 241664;
        }

        if (invoice != null && !await db.Payments.IgnoreQueryFilters().AnyAsync(x => x.SalesInvoiceId == invoice.Id))
        {
            db.Payments.Add(new Payment { TenantId = tenantId, PaymentNumber = "PAY-DEMO-0001", SalesInvoiceId = invoice.Id, ContactId = contact.Id, Amount = invoice.PaidAmount, PaymentMode = PaymentMode.BankTransfer, PaymentDate = DateTime.UtcNow.AddDays(-1), Reference = "UTR-DEMO-0001", Notes = "Advance payment received from the demo customer." });
        }

        if (!await db.Notifications.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId))
        {
            db.Notifications.AddRange(
                new Notification { TenantId = tenantId, UserId = user.Id, Title = "Welcome to Sujex", Message = "Your demo workspace is ready. Review the dashboard to see seeded activity.", Type = "Success", ActionUrl = "/dashboard" },
                new Notification { TenantId = tenantId, UserId = user.Id, Title = "Leave request pending", Message = "A demo leave request is waiting for approval.", Type = "Info", ActionUrl = "/hr/leave-requests" });
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Verified transactional demo details for tenant {TenantId}.", tenantId);
    }
}
