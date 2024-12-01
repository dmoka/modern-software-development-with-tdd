namespace VerticalSlicingArchitecture.Shared;

public class Result<T> : Result
{
    private Result(bool isSuccess, T value, Error error)
        : base(isSuccess, error)
    {
        Value = value;

        if (isSuccess && value == null)
        {
            throw new ArgumentNullException(nameof(value), "Value cannot be null for a successful result.");
        }
    }

    public T Value { get; }

    public static Result<T> Success(T value) => new(true, value, Error.None);

    public static Result<T> Failure(Error error) => new(false, default!, error);
}