namespace FitbitApi.Exceptions;

public class UnauthorizedFitbitClientException : Exception
{
    public UnauthorizedFitbitClientException(string? message) : base(message)
    {
    }

    public UnauthorizedFitbitClientException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}