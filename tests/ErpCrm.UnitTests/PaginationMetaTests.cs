using ErpCrm.Application.Common.Models;
using Xunit;

namespace ErpCrm.UnitTests;

public class PaginationMetaTests
{
    [Fact]
    public void Calculates_total_pages_and_navigation_flags()
    {
        var pagination = new PaginationMeta { Page = 2, PageSize = 20, TotalCount = 41 };

        Assert.Equal(3, pagination.TotalPages);
        Assert.True(pagination.HasNext);
        Assert.True(pagination.HasPrevious);
    }
}
