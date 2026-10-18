namespace ErpCrm.Application.Common.Interfaces;

public interface INumberSequenceService
{
    Task<string> GetNextAsync(Guid tenantId, Guid companyId, string documentType);
}
