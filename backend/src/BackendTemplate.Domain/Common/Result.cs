namespace BackendTemplate.Domain.Common;

public sealed class Result<T>
{
    private readonly T? _value;
    private readonly string? _error;

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
    }

    private Result(string error, ErrorKind kind)
    {
        _error = error;
        Kind = kind;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public ErrorKind Kind { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    public string Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(string error, ErrorKind kind = ErrorKind.Validation) =>
        new(error, kind);
}
