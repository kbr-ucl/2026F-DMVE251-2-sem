using Microsoft.EntityFrameworkCore;
using ParallelEntityFrameworkConsoleApp.Data;
using ParallelEntityFrameworkConsoleApp.Dtos;

namespace ParallelEntityFrameworkConsoleApp.Tasks;

public sealed class CustomerStatsTask(NorthwindDbContext db)
{
    public async Task<CustomerStatsDto> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var customerCount = await db.Customers.AsNoTracking().CountAsync(cancellationToken);
        if (customerCount == 0)
            throw new InvalidOperationException("No customers in database.");

        var skip = Random.Shared.Next(customerCount);

        var customerId = await db.Customers
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Skip(skip)
            .Select(c => c.Id)
            .FirstAsync(cancellationToken);

        var customer = await db.Customers
            .AsNoTracking()
            .Where(c => c.Id == customerId)
            .Select(c => new { c.Id, c.CompanyName, OrderCount = c.Orders.Count })
            .SingleAsync(cancellationToken);

        var totalOrderSum = await db.OrderDetails
            .AsNoTracking()
            .Where(od => od.Order.CustomerId == customerId)
            .SumAsync(d => d.UnitPrice * d.Quantity * (1m - (decimal)d.Discount), cancellationToken);

        return new CustomerStatsDto(
            customer.Id,
            customer.CompanyName ?? string.Empty,
            customer.OrderCount,
            totalOrderSum);
    }
}
