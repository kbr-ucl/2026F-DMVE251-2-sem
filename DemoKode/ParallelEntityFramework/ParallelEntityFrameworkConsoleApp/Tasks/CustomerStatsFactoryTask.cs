using Microsoft.EntityFrameworkCore;
using ParallelEntityFrameworkConsoleApp.Data;
using ParallelEntityFrameworkConsoleApp.Dtos;
using ParallelEntityFrameworkConsoleApp.Queries;

namespace ParallelEntityFrameworkConsoleApp.Tasks;

public sealed class CustomerStatsFactoryTask(IDbContextFactory<NorthwindDbContext> factory)
{
    public async Task<CustomerStatsDto> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        return await CustomerStatsQuery.GetRandomCustomerStatsAsync(db, cancellationToken);
    }
}
