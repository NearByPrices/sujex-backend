namespace ErpCrm.Application.Common.Models;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public PaginationMeta Pagination { get; set; } = new();
}
