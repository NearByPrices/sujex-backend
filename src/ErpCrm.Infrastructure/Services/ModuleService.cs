using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Domain.Entities.Platform;
using ErpCrm.Domain.Enums;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ErpCrm.Infrastructure.Services;

public class ModuleService : IModuleService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public ModuleService(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<bool> IsEnabledAsync(Guid tenantId, ModuleCode module)
    {
        var modules = await GetEnabledModulesAsync(tenantId);
        return modules.Contains(module);
    }

    public async Task EnableModuleAsync(Guid tenantId, ModuleCode module)
    {
        var existing = await _db.TenantModules.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.ModuleCode == module);

        if (existing == null)
            _db.TenantModules.Add(new TenantModule { TenantId = tenantId, ModuleCode = module, IsEnabled = true });
        else
            existing.IsEnabled = true;

        await _db.SaveChangesAsync();
        _cache.Remove($"modules_{tenantId}");
    }

    public async Task DisableModuleAsync(Guid tenantId, ModuleCode module)
    {
        var existing = await _db.TenantModules.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.ModuleCode == module);

        if (existing != null)
        {
            existing.IsEnabled = false;
            await _db.SaveChangesAsync();
        }
        _cache.Remove($"modules_{tenantId}");
    }

    public async Task<IEnumerable<ModuleCode>> GetEnabledModulesAsync(Guid tenantId)
    {
        var cacheKey = $"modules_{tenantId}";
        if (_cache.TryGetValue(cacheKey, out IEnumerable<ModuleCode>? cached) && cached != null)
            return cached;

        var modules = await _db.TenantModules.IgnoreQueryFilters()
            .Where(m => m.TenantId == tenantId && m.IsEnabled)
            .Select(m => m.ModuleCode)
            .ToListAsync();

        _cache.Set(cacheKey, (IEnumerable<ModuleCode>)modules, TimeSpan.FromMinutes(5));
        return modules;
    }
}
