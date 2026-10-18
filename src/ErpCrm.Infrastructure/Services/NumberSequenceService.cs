using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.Infrastructure.Services;

public class NumberSequenceService : INumberSequenceService
{
    private readonly AppDbContext _db;

    public NumberSequenceService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> GetNextAsync(Guid tenantId, Guid companyId, string documentType)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var seq = await _db.NumberSequences
                .FromSqlRaw(
                    "SELECT * FROM number_sequences WHERE tenant_id = {0} AND company_id = {1} AND document_type = {2} FOR UPDATE",
                    tenantId, companyId, documentType)
                .FirstOrDefaultAsync();

            if (seq == null)
                throw new KeyNotFoundException($"Number sequence not found for document type: {documentType}");

            var next = seq.GetNext();
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return next;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
