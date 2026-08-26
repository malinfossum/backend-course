namespace CouponApi.Api;

// The same Result<T> as in CinemaBooking (week 2), carried over unchanged.
// It knows nothing about HTTP - Results.Ok(...) is ASP.NET's own type and
// belongs in the endpoint.
//
// The ErrorKind is what challenge 1 asks for: without it every failure has to
// become 400, so "no coupon with id 999" and "that code already exists" answer
// the same status code even though they are different problems.
public class Result<T>
{
    public bool IsSuccess { get; }

    public T? Value { get; }

    public string? ErrorMessage { get; }

    public ErrorKind Error { get; }

    private Result(bool isSuccess, T? value, string? errorMessage, ErrorKind error)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        Error = error;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null, ErrorKind.None);
    }

    // There is deliberately no Failure(string) overload. Forgetting the kind
    // is exactly the bug challenge 1 is about, so the compiler asks for it.
    public static Result<T> Failure(ErrorKind error, string errorMessage)
    {
        return new Result<T>(false, default, errorMessage, error);
    }

    public static Result<T> NotFound(string errorMessage)
    {
        return Failure(ErrorKind.NotFound, errorMessage);
    }

    public static Result<T> Validation(string errorMessage)
    {
        return Failure(ErrorKind.Validation, errorMessage);
    }

    public static Result<T> Conflict(string errorMessage)
    {
        return Failure(ErrorKind.Conflict, errorMessage);
    }
}
