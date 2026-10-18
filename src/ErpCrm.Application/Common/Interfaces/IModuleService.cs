using ErpCrm.Domain.Enums;

namespace ErpCrm.Application.Common.Interfaces;

public interface IModuleService
{
    Task<bool> IsEnabledAsync(Guid tenantId, ModuleCode module);
    Task EnableModuleAsync(Guid tenantId, ModuleCode module);
    Task DisableModuleAsync(Guid tenantId, ModuleCode module);
    Task<IEnumerable<ModuleCode>> GetEnabledModulesAsync(Guid tenantId);
}
