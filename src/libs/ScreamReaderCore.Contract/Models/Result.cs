namespace ScreamReaderCore.Contract.Models;

public record Result
{
    public Result(bool isSuccess = true, string? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public string? Error { get; }
    public bool IsSuccess { get; }
}

public record Result<TResult> : Result
{
    public Result(Exception error)
        : base(false, error.ToString())
    {
        Value = default!;
    }

    public Result(bool isSuccess, string? error = null)
        : base(isSuccess, error)
    {
        Value = default!;
    }

    public Result(TResult result) 
        : base(true)
    {
        Value = result;
    }

    public TResult Value { get; }

    
    public static implicit operator TResult(Result<TResult> result) => result.Value;
    
    public static implicit operator Result<TResult>(TResult result) => new Result<TResult>(result);
}