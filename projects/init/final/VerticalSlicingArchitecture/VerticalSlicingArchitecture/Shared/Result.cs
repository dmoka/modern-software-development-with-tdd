using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VerticalSlicingArchitecture.Shared;

public class Result
{
    private Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.Empty)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.Empty);

    public static Result Failure(Error error) => new(false, error);
}