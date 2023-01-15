using System.Text;

namespace FitbitApi.Extensions;

internal static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> ExecuteRequestWithParamsAsync(this HttpClient client, HttpRequestMessage message,
        Dictionary<string, string> queryParams, CancellationToken cancellationToken)
    {
        message.RequestUri = 
            new Uri(GetUriWithQueryString(message.RequestUri?.OriginalString ?? string.Empty, queryParams), UriKind.RelativeOrAbsolute);

        return await client.SendAsync(message, cancellationToken);
    }

    private static string GetUriWithQueryString(string requestUri,
        Dictionary<string, string> queryStringParams)
    {
        bool startingQuestionMarkAdded = false;
        var sb = new StringBuilder();
        sb.Append(requestUri);
        foreach (var parameter in queryStringParams)
        {
            sb.Append(startingQuestionMarkAdded ? '&' : '?');
            sb.Append(parameter.Key);
            sb.Append('=');
            sb.Append(parameter.Value);
            startingQuestionMarkAdded = true;
        }
        return sb.ToString();
    }
}