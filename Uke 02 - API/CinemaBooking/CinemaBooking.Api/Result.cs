namespace CinemaBooking.Api;

// Vår egen Result<T> fra undervisningen. Den vet ingenting om HTTP.
// Results.Ok(...) er ASP.NETs egen - den hører hjemme i endepunktet.
public class Result<T>
{
    public bool IsSuccess { get; }

    public T? Value { get; }

    public string? ErrorMessage { get; }

    private Result(bool isSuccess, T? value, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null);
    }

    public static Result<T> Failure(string errorMessage)
    {
        return new Result<T>(false, default, errorMessage);
    }
}
