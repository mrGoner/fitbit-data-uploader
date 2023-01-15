using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FitbitApi.DataModels;
using FitbitApi.DataModels.Activity;
using FitbitApi.DataModels.Authorization;
using FitbitApi.DataModels.Sleep;
using FitbitApi.Exceptions;
using FitbitApi.Extensions;
using FitbitApi.Helpers;

namespace FitbitApi;

public sealed class FitbitClient : IDisposable
{
    private readonly HttpClient _httpClient;

    public FitbitClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.fitbit.com")
        };
    }

    public async Task<AuthorizeData> GetAuthorizeData(string clientId, IReadOnlyCollection<Scope> scopes, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(clientId)) 
            throw new ArgumentNullException(nameof(clientId));
        
        const string authTemplate =
            "https://www.fitbit.com/oauth2/authorize?response_type=code&client_id={0}&scope={1}&code_challenge={2}&code_challenge_method=S256";

        var codeVerifier = AuthHelper.GenerateCodeVerifier();

        var codeChallenge = await AuthHelper.GenerateCodeChallenge(codeVerifier, cancellationToken);

        return new AuthorizeData(string.Format(authTemplate, clientId, ConvertScopes(scopes), codeChallenge),
            codeVerifier, codeChallenge);
    }

    public async Task<TokenInfo> GetToken(string clientId, string clientSecret, string authorizeCode, string codeVerifier, string state, CancellationToken cancellationToken)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/oauth2/token")
        {
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", authorizeCode),
                new KeyValuePair<string, string>("code_verifier", codeVerifier)
            })
        };

        var authString = $"{clientId}:{clientSecret}";

        requestMessage.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(authString)));

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        
        var getTokenResponse = await GetResponseData<TokenInfo>(response, cancellationToken);
        
        return getTokenResponse;
    }
    
    public async Task<TokenInfo> RefreshToken(string clientId, string clientSecret, string refreshToken, CancellationToken cancellationToken)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/oauth2/token")
        {
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            })
        };

        var authString = $"{clientId}:{clientSecret}";

        requestMessage.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(authString)));

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        
        var getTokenResponse = await GetResponseData<TokenInfo>(response, cancellationToken);

        return getTokenResponse;
    }

    public async Task<FitbitApiResponse<CreateActivityLogResponse>> CreateActivityLog(CreateActivityLogRequest createActivityLogRequest, string token, CancellationToken cancellationToken)
    {
        var requestMessage =
            new HttpRequestMessage(HttpMethod.Post, $"/1/user/{createActivityLogRequest.UserId}/activities.json");

        var queryParams = new Dictionary<string, string>
        {
            {"activityId", createActivityLogRequest.ActivityId.ToString()},
            {"date", createActivityLogRequest.ActivityStart.FormatDateToApiCompatible()},
            {"startTime", createActivityLogRequest.ActivityStart.FormatTimeToApiCompatible()},
            {"durationMillis", ((int) createActivityLogRequest.Duration.TotalMilliseconds).ToString()},
            {"distance", createActivityLogRequest.Distance.ToString(CultureInfo.InvariantCulture)},
            {"distanceUnit", createActivityLogRequest.DistanceUnit.ToString().ToLower()}
        };

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.ExecuteRequestWithParamsAsync(requestMessage, queryParams, cancellationToken);
        
        var createActivityLogResponse = await GetResponseData<CreateActivityLogResponse>(response, cancellationToken);

        var quotas = response.Headers.GetQuotaFromHeaders();

        return new FitbitApiResponse<CreateActivityLogResponse>(createActivityLogResponse, quotas);
    }

    public async Task<FitbitApiResponse<GetAllActivitiesResponse>> GetAllActivities(string token, CancellationToken cancellationToken)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/1/activities.json");

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        var getAllActivitiesResponse = await GetResponseData<GetAllActivitiesResponse>(response, cancellationToken);

        var quotas = response.Headers.GetQuotaFromHeaders();

        return new FitbitApiResponse<GetAllActivitiesResponse>(getAllActivitiesResponse, quotas);
    }

    public async Task<FitbitApiResponse<CreateSleepLogResponse>> CreateSleepLog(CreateSleepLogRequest request, string token, CancellationToken cancellationToken)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/1.2/user/{request.UserId}/sleep.json")
        {
            Headers = {Authorization = new AuthenticationHeaderValue("Bearer", token)}
        };

        var requestParams = new Dictionary<string, string>
        {
            {"date", request.StartDateTime.FormatDateToApiCompatible()},
            {"startTime", request.StartDateTime.FormatTimeToApiCompatible()},
            {"duration", ((int) request.Duration.TotalMilliseconds).ToString()}
        };
        
        var response = await _httpClient.ExecuteRequestWithParamsAsync(requestMessage, requestParams, cancellationToken);

        var createSleepLogResponse = await GetResponseData<CreateSleepLogResponse>(response, cancellationToken);

        var quotas = response.Headers.GetQuotaFromHeaders();

        return new FitbitApiResponse<CreateSleepLogResponse>(createSleepLogResponse, quotas);
    }

    private static string ConvertScopes(IEnumerable<Scope> scopes)
    {
        return string.Join("+", scopes.Select(scope => scope.ToString().ToSnakeCase()));
    }

    private static async ValueTask<T> GetResponseData<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await response.ThrowIfStatusCodeUnsuccessful(cancellationToken);

        var responseData = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);

        if (responseData == null)
            throw new FitbitClientParseException(await response.Content.ReadAsStringAsync(cancellationToken));

        return responseData;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}