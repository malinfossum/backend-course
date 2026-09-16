using CodeFirstOrders.Api.Infrastructure;
using CodeFirstOrders.Api.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeFirstOrders.Api.Services;

// Week 6 Wednesday: one business operation ("ship 2 keyboards") that is two database writes
// (StockCount down, Shipment in). The three methods are the three stages of the exercise,
// kept side by side so the difference is readable. Only ShipAsync is the real one.
//
// simulateCrash stands in for the "throw new Exception("Simulated crash")" the exercise asks
// you to paste between the two writes, so the crash test runs from the .http file instead of
// by editing code between runs.
public class ShippingService
{
    private readonly OrdersDbContext _context;

    public ShippingService(OrdersDbContext context)
    {
        _context = context;
    }

    // Stage 1 (tasks 1–2): two SaveChanges, no transaction. Looks fine until the crash —
    // then StockCount is 8 and there is no Shipment. The first SaveChanges is already permanent.
    public async Task ShipNaiveAsync(int productId, int quantity, bool simulateCrash)
    {
        var product = await _context.Products.SingleAsync(candidate => candidate.Id == productId);

        product.StockCount -= quantity;
        await _context.SaveChangesAsync();

        if (simulateCrash)
        {
            throw new InvalidOperationException("Simulated crash");
        }

        _context.Shipments.Add(new Shipment { ProductId = productId, Quantity = quantity, CreatedUtc = DateTime.UtcNow });
        await _context.SaveChangesAsync();
    }

    // Stage 2 (tasks 3–4): the same two SaveChanges inside one explicit transaction. The first
    // UPDATE is sent to the database but not committed; the crash reaches catch, and Rollback
    // undoes it. StockCount is back at 10.
    public async Task ShipWithTransactionAsync(int productId, int quantity, bool simulateCrash)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var product = await _context.Products.SingleAsync(candidate => candidate.Id == productId);

            product.StockCount -= quantity;
            await _context.SaveChangesAsync();

            if (simulateCrash)
            {
                throw new InvalidOperationException("Simulated crash");
            }

            _context.Shipments.Add(new Shipment { ProductId = productId, Quantity = quantity, CreatedUtc = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Stage 3 (tasks 5, 7, 8): validate first, then both writes in ONE SaveChanges. EF wraps a
    // single SaveChanges in its own transaction, so no BeginTransaction is needed — there is
    // nothing between the two writes for a crash to land in.
    public async Task<Result<Shipment>> ShipAsync(int productId, int quantity)
    {
        if (quantity <= 0)
        {
            return Result<Shipment>.Validation("Quantity must be positive.");
        }

        var product = await _context.Products.SingleOrDefaultAsync(candidate => candidate.Id == productId);

        if (product is null)
        {
            return Result<Shipment>.NotFound($"No product with id {productId}.");
        }

        if (product.StockCount < quantity)
        {
            return Result<Shipment>.Conflict($"Only {product.StockCount} in stock.");
        }

        var shipment = new Shipment { ProductId = productId, Quantity = quantity, CreatedUtc = DateTime.UtcNow };

        product.StockCount -= quantity;
        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync();

        return Result<Shipment>.Success(shipment);
    }
}
