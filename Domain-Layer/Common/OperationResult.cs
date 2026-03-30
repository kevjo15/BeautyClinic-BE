namespace Domain_Layer.Common;

public enum OperationFailureType
{
    Validation,
    NotFound,
    Forbidden,
    Unauthorized,
    Conflict
}

public class OperationResult
{
    protected OperationResult(bool successful, string? error = null, OperationFailureType? failureType = null)
    {
        Successful = successful;
        Error = error;
        FailureType = failureType;
    }

    public bool Successful { get; }
    public string? Error { get; }
    public OperationFailureType? FailureType { get; }

    public static OperationResult Success() => new(true);
    public static OperationResult Failure(string error, OperationFailureType failureType = OperationFailureType.Validation)
        => new(false, error, failureType);
}

public sealed class OperationResult<T> : OperationResult
{
    private OperationResult(bool successful, T? data, string? error = null, OperationFailureType? failureType = null)
        : base(successful, error, failureType)
    {
        Data = data;
    }

    public T? Data { get; }

    public static OperationResult<T> Success(T data) => new(true, data);
    public new static OperationResult<T> Failure(string error, OperationFailureType failureType = OperationFailureType.Validation)
        => new(false, default, error, failureType);
}
