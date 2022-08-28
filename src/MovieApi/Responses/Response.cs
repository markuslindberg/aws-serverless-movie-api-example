namespace MovieApi.Responses;

public class Response<TResult>
{
    public int StatusCode { get; init; }
    public TResult? Result { get; init; }
    public string? ErrorMessage { get; init; }

    public Response(int statusCode, string errorMessage)
    {
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
    }

    public Response(TResult result)
    {
        StatusCode = 200;
        Result = result;
    }
}