using ParallelEntityFrameworkConsoleApp.Data;
using ParallelEntityFrameworkConsoleApp.Dtos;
using ParallelEntityFrameworkConsoleApp.Queries;

namespace ParallelEntityFrameworkConsoleApp.Tasks;

public sealed class CustomerStatsTask(NorthwindDbContext db)
{
    public Task<CustomerStatsDto> ExecuteAsync(CancellationToken cancellationToken = default)
        => CustomerStatsQuery.GetRandomCustomerStatsAsync(db, cancellationToken);
}
