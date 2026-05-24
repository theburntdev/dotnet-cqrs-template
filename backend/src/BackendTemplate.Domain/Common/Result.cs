namespace BackendTemplate.Domain.Common;

public sealed class Result
{
    private readonly string? _error;
    private readonly ErrorKind? _kind;

    private Result(bool isSuccess, string? error, ErrorKind? kind)
    {
        IsSuccess = isSuccess;
        _error = error;
        _kind = kind;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public string Error => IsSuccess
        ? throw new InvalidOperationException("Cannot access Error on a successful result.")
        : _error!;

    public ErrorKind Kind => IsSuccess
        ? throw new InvalidOperationException("Cannot access Kind on a successful result.")
        : _kind!.Value;

    public static Result Success() => new(true, null, null);

    public static Result Failure(string error, ErrorKind kind = ErrorKind.Validation) =>
        new(false, error, kind);
}

public sealed class Result<T>
{
    private readonly T? _value;
    private readonly string? _error;
    private readonly ErrorKind? _kind;

    private Result(bool isSuccess, T? value, string? error, ErrorKind? kind)
    {
        IsSuccess = isSuccess;
        _value = value;
        _error = error;
        _kind = kind;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

    public string Error => IsSuccess
        ? throw new InvalidOperationException("Cannot access Error on a successful result.")
        : _error!;

    public ErrorKind Kind => IsSuccess
        ? throw new InvalidOperationException("Cannot access Kind on a successful result.")
        : _kind!.Value;

    public static Result<T> Success(T value) => new(true, value, null, null);

    public static Result<T> Failure(string error, ErrorKind kind = ErrorKind.Validation) =>
        new(false, default, error, kind);
}
