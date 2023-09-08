namespace ElasticsearchTrial.Utilities.Results;

public class Result : IResult
{

    public Result(bool success, string message)
        : this(success)
    {
        Message = message;
        return;
    }

    public Result(bool success)
    {
        Success = success;
    }

    protected Result()
    {

    }

    public bool Success { get; set; }
    public string Message { get; set; }
}
