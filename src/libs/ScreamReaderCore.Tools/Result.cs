namespace ScreamReaderCore.Tools;

public record Result
{
    private static readonly Result _success = new Result(true);
    public static Result Success() => _success;
    public static Result<TResult> Success<TResult>(TResult result) => new Result<TResult>(result);
    public static Result Failure(Exception exception) => new Result(exception);
    public static Result<TResult> Failure<TResult>(Exception exception) => new Result<TResult>(exception);
    public static Result<TResult> Failure<TResult>(string? error) => new Result<TResult>(false, error);
    public static Result Failure(string? error) => new Result(false, error);
    
    internal Result(Exception exception) : this(false, exception.ToString()){}
    
    internal Result(bool isSuccess = true, string? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public string? Error { get; }
    public bool IsSuccess { get; }
}

public record Result<TResult> : Result
{
    internal Result(Exception exception)
        : base(exception)
    {
        Value = default!;
    }

    internal Result(bool isSuccess, string? error = null)
        : base(isSuccess, error)
    {
        Value = default!;
    }

    internal Result(TResult result) 
        : base(true)
    {
        Value = result;
    }

    public TResult Value { get; }

    
    public static implicit operator TResult(Result<TResult> result) => result.Value;
    
    public static implicit operator Result<TResult>(TResult result) => new Result<TResult>(result);
}