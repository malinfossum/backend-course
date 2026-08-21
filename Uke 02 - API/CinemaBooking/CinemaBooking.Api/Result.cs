namespace CinemaBooking.Api;

// Our own Result<T> from the lectures. It knows nothing about HTTP.
// Results.Ok(...) is ASP.NET's own - that one belongs in the endpoint.
//
// A failure now also carries an ErrorKind, so the endpoint can tell the
// difference between "does not exist" and "conflicts with what is there".
// Without it every failure had to become 400, which is why GET /screenings/999
// answered 404 while POST to the same id answered 400.
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
    // is exactly the bug this challenge is about, so the compiler asks for it.
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
