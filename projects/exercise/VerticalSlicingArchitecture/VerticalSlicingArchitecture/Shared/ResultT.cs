namespace VerticalSlicingArchitecture.Shared;

public class Result<T>
{
    private Result(bool isSuccess, T value, Error error)
    {
        if (isSuccess && error != Error.Empty)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T Value { get; }

    public Error Error { get; }

    public static Result<T> Success(T value) => new(true, value, Error.Empty);

    public static Result<T> Failure(Error error) => new(false, default, error);
}
