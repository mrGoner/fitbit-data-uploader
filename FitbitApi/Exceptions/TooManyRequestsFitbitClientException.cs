namespace FitbitApi.Exceptions;

public class TooManyRequestsFitbitClientException : Exception
{
    public TimeSpan? TimeToReset { get; }

    public TooManyRequestsFitbitClientException(TimeSpan? timeToReset)
    {
        TimeToReset = timeToReset;
    }
}