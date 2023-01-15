namespace FitbitApi.Exceptions;

public class FitbitClientParseException : Exception
{
    public FitbitClientParseException(string? message) : base(message)
    {
    }

    public FitbitClientParseException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}