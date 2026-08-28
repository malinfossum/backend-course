using CouponApi.Api.Models;

namespace CouponApi.Api.Data;

// The contract the rest of the application depends on. It says nothing about
// SQL, Dapper or SQL Server - only what the service needs done.
//
// Note what the return types admit to. CreateAsync returns int? because the
// database, not the caller, decided the id - and null because the UNIQUE rule
// can refuse the insert outright. The three write methods return bool because
// all the UPDATE/DELETE can honestly report is whether it hit a row; deciding
// what "it hit nothing" means is the service's job.
public interface ICouponRepository
{
    Task<IEnumerable<Coupon>> GetAllAsync();

    Task<Coupon?> FindAsync(int id);

    Task<Coupon?> FindByCodeAsync(string code);

    // Returns the new id, or null if the code is already taken.
    Task<int?> CreateAsync(Coupon coupon);

    Task<bool> TryUseAsync(int id);

    Task<bool> DeactivateAsync(int id);

    Task<bool> ActivateAsync(int id);

    Task<bool> AddUsesAsync(int id, int amount);

    Task<bool> DeleteAsync(int id);
}
