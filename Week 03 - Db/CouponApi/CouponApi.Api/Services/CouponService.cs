using CouponApi.Api.Data;
using CouponApi.Api.Dtos;
using CouponApi.Api.Models;

namespace CouponApi.Api.Services;

// Where the rules live. The repository knows SQL and nothing else; this class
// knows what a coupon is allowed to do and turns "0 rows affected" back into
// something a human can read.
public class CouponService
{
    // The column widths from the table. Checking them here means an over-long
    // code comes back as a readable 400 instead of a truncation error from the
    // database - the constraint is still there, this just gets in first.
    private const int MaxCodeLength = 50;
    private const int MaxDescriptionLength = 200;

    private readonly ICouponRepository _couponRepository;

    // The service is handed an ICouponRepository and does not know which one
    // it got. Swapping SQL Server for anything else is one line in Program.cs.
    public CouponService(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<IEnumerable<Coupon>> GetAllAsync()
    {
        return await _couponRepository.GetAllAsync();
    }

    public async Task<Result<Coupon>> GetByIdAsync(int id)
    {
        var coupon = await _couponRepository.FindAsync(id);

        if (coupon == null)
        {
            return Result<Coupon>.NotFound($"No coupon with id {id}.");
        }

        return Result<Coupon>.Success(coupon);
    }

    public async Task<Result<Coupon>> CreateCouponAsync(CreateCouponDto dto)
    {
        // Codes are compared and printed in upper case, so they are stored that
        // way too. SQL Server's default collation ignores case anyway, which
        // means "summer26" would collide with SUMMER26 in the UNIQUE index -
        // normalising here keeps what is stored and what is compared identical.
        var code = dto.Code.Trim().ToUpperInvariant();
        var description = dto.Description.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<Coupon>.Validation("The coupon must have a code.");
        }

        if (code.Length > MaxCodeLength)
        {
            return Result<Coupon>.Validation(
                $"The code can be at most {MaxCodeLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result<Coupon>.Validation("The coupon must have a description.");
        }

        if (description.Length > MaxDescriptionLength)
        {
            return Result<Coupon>.Validation(
                $"The description can be at most {MaxDescriptionLength} characters.");
        }

        if (dto.RemainingUses <= 0)
        {
            return Result<Coupon>.Validation(
                "A new coupon must have at least one use left.");
        }

        // Pure validation first, then the lookup: there is no reason to read
        // storage to reject a request we already know is invalid.
        var existing = await _couponRepository.FindByCodeAsync(code);

        if (existing != null)
        {
            // This check is what gives the user a sentence instead of a crash.
            // The UNIQUE index is what actually guarantees it, including for
            // writes that never pass through this service at all.
            return Result<Coupon>.Conflict($"The code {code} is already in use.");
        }

        var coupon = new Coupon
        {
            Code = code,
            Description = description,
            RemainingUses = dto.RemainingUses,
            IsActive = true
        };

        coupon.Id = await _couponRepository.CreateAsync(coupon);

        return Result<Coupon>.Success(coupon);
    }

    public async Task<Result<Coupon>> UseCouponAsync(int id)
    {
        // Ask the database to spend a use, conditions and all, before asking it
        // anything about the coupon. If the conditions hold this is the only
        // round trip that matters, and no other request can slip in between.
        var wasUsed = await _couponRepository.TryUseAsync(id);

        var coupon = await _couponRepository.FindAsync(id);

        if (coupon == null)
        {
            return Result<Coupon>.NotFound($"No coupon with id {id}.");
        }

        if (wasUsed)
        {
            return Result<Coupon>.Success(coupon);
        }

        // The UPDATE matched no row, and only now do we look at why. The
        // repository could not tell us: all three of these look identical
        // from where it stands - 0 rows affected.
        if (!coupon.IsActive)
        {
            return Result<Coupon>.Conflict($"The coupon {coupon.Code} is deactivated.");
        }

        if (coupon.RemainingUses == 0)
        {
            return Result<Coupon>.Conflict($"The coupon {coupon.Code} has no uses left.");
        }

        // Nothing above explains it, so the state changed between the UPDATE
        // and the read. Saying so is better than inventing a reason.
        return Result<Coupon>.Conflict(
            $"The coupon {coupon.Code} could not be used. Try again.");
    }

    public async Task<Result<Coupon>> DeactivateCouponAsync(int id)
    {
        var wasDeactivated = await _couponRepository.DeactivateAsync(id);

        if (!wasDeactivated)
        {
            // This UPDATE has no condition except the id, so 0 rows affected
            // has only one possible meaning here.
            return Result<Coupon>.NotFound($"No coupon with id {id}.");
        }

        return await GetByIdAsync(id);
    }

    // Challenge 2. IsActive and RemainingUses are two separate rules, so
    // reactivating a used-up coupon is allowed - it just still cannot be used.
    public async Task<Result<Coupon>> ActivateCouponAsync(int id)
    {
        var wasActivated = await _couponRepository.ActivateAsync(id);

        if (!wasActivated)
        {
            return Result<Coupon>.NotFound($"No coupon with id {id}.");
        }

        return await GetByIdAsync(id);
    }

    // Challenge 3.
    public async Task<Result<Coupon>> AddUsesAsync(int id, int amount)
    {
        if (amount <= 0)
        {
            return Result<Coupon>.Validation("The amount must be greater than zero.");
        }

        var wasUpdated = await _couponRepository.AddUsesAsync(id, amount);

        if (!wasUpdated)
        {
            return Result<Coupon>.NotFound($"No coupon with id {id}.");
        }

        return await GetByIdAsync(id);
    }

    public async Task<Result<Coupon>> DeleteCouponAsync(int id)
    {
        // Read first, so the answer can say which coupon was deleted and so a
        // missing id is a plain 404. After that, 0 rows affected can only mean
        // somebody else deleted the same row first.
        var coupon = await _couponRepository.FindAsync(id);

        if (coupon == null)
        {
            return Result<Coupon>.NotFound($"No coupon with id {id}.");
        }

        var wasDeleted = await _couponRepository.DeleteAsync(id);

        if (!wasDeleted)
        {
            return Result<Coupon>.NotFound($"No coupon with id {id}.");
        }

        return Result<Coupon>.Success(coupon);
    }
}
