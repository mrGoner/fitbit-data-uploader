using System.Net;
using System.Net.Http.Headers;
using FitbitApi.DataModels.Quotas;
using FitbitApi.Exceptions;

namespace FitbitApi.Extensions;

internal static class HttpResponseExtensions
{
    public static async ValueTask ThrowIfStatusCodeUnsuccessful(this HttpResponseMessage message, CancellationToken cancellationToken)
    {
        if(message.IsSuccessStatusCode)
            return;

        var contentError = await message.Content.ReadAsStringAsync(cancellationToken);

        throw message.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new UnauthorizedFitbitClientException(contentError),
            HttpStatusCode.TooManyRequests => new TooManyRequestsFitbitClientException(message.Headers
                .GetQuotaFromHeadersOrDefault()?.TimeToLimitReset),
            _ => new FitbitClientException(contentError)
        };
    }

    public static ApiCallsQuota GetQuotaFromHeaders(this HttpResponseHeaders responseHeaders)
    {
        var rateLimit = int.Parse(responseHeaders.GetValues("fitbit-rate-limit-limit").Single());
        var remainingLimit = int.Parse(responseHeaders.GetValues("fitbit-rate-limit-remaining").Single());
        var resetTime = TimeSpan.FromSeconds(int.Parse(responseHeaders.GetValues("fitbit-rate-limit-reset").Single()));

        return new ApiCallsQuota(rateLimit, remainingLimit, resetTime);
    }

    public static ApiCallsQuota? GetQuotaFromHeadersOrDefault(this HttpResponseHeaders responseHeaders)
    {
        try
        {
            return responseHeaders.GetQuotaFromHeaders();
        }
        catch
        {
            return default;
        }
    }
}