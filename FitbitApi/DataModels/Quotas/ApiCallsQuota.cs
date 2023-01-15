namespace FitbitApi.DataModels.Quotas;

public record ApiCallsQuota(int CallsTotal, int RemainingCalls, TimeSpan TimeToLimitReset);