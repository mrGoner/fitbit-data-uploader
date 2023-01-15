using FitbitApi.DataModels.Quotas;

namespace FitbitApi.DataModels;

public record FitbitApiResponse<T>(T Response, ApiCallsQuota Quota);