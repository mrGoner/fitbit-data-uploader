namespace FitbitApi.Exceptions;

public class FitbitClientException : Exception
{
    public FitbitClientException(string data) : base($"Failed to parse data: {data}")
    {
    }

    public FitbitClientException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}