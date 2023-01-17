namespace FitbitDataUploader;

public record RetryPolicy(int RetryCount, TimeSpan WaitBetweenRetries);