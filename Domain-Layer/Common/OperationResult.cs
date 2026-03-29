namespace Domain_Layer.Common;

public class OperationResult
{
    protected OperationResult(bool successful, string? error = null)
    {
        Successful = successful;
        Error = error;
    }

    public bool Successful { get; }
    public string? Error { get; }

    public static OperationResult Success() => new(true);
    public static OperationResult Failure(string error) => new(false, error);
}

public sealed class OperationResult<T> : OperationResult
{
    private OperationResult(bool successful, T? data, string? error = null)
        : base(successful, error)
    {
        Data = data;
    }

    public T? Data { get; }

    public static OperationResult<T> Success(T data) => new(true, data);
    public new static OperationResult<T> Failure(string error) => new(false, default, error);
}
