namespace ElasticsearchTrial.Utilities.Results;

public class ApiResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
}

public class ApiReturn : ApiResult<object>
{
}
