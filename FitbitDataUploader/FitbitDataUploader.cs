using FitbitApi;
using FitbitApi.DataModels;
using FitbitApi.DataModels.Activity;
using FitbitApi.DataModels.Quotas;
using FitbitApi.DataModels.Sleep;
using FitbitApi.Exceptions;
using FitbitDataUploader.Abstractions.Models;
using Microsoft.Extensions.Logging;
using DistanceUnit = FitbitDataUploader.Abstractions.Models.DistanceUnit;

namespace FitbitDataUploader;

public class FitbitUploader
{
    private readonly ILogger<FitbitUploader> _logger;

    public FitbitUploader(ILogger<FitbitUploader> logger)
    {
        _logger = logger;
    }

    public async Task UploadSleepData(UserInfo userInfo, ClientInfo? clientInfo,
        IEnumerable<SleepData> sleepDataEnumerable, RetryPolicy? retryPolicy, CancellationToken cancellationToken)
    {
        var fitBitClient = new FitbitClient();
        var token = userInfo.UserToken;
        var refreshToken = userInfo.RefreshToken;

        var progressCount = 0;

        _logger.LogInformation("Start upload sleep data");

        foreach (var sleepData in sleepDataEnumerable)
        {
            var response = await UploadWithPolicy(async userToken =>
                {
                    var response = await fitBitClient.CreateSleepLog(
                        new CreateSleepLogRequest(userInfo.UserId, sleepData.SleepStart, sleepData.Duration),
                        userToken,
                        cancellationToken);

                    return response;
                }, fitBitClient, token, refreshToken, clientInfo, retryPolicy,
                cancellationToken);

            progressCount++;
            token = response.Token;
            refreshToken = response.RefreshToken;

            _logger.LogInformation("Processed {Count} sleep data", progressCount);

            if (response.Quota.RemainingCalls > 0)
                continue;

            _logger.LogWarning("Remaining calls quota is 0. Delay for {Delay}", response.Quota.TimeToLimitReset);

            await Task.Delay(response.Quota.TimeToLimitReset, cancellationToken);
        }

        _logger.LogInformation("Upload sleep data complete. Uploaded {Count}", progressCount);
    }

    public async Task UploadActivityData(UserInfo userInfo, ClientInfo? clientInfo,
        IEnumerable<ActivityData> activityEnumerable, RetryPolicy? retryPolicy, CancellationToken cancellationToken)
    {
        var fitBitClient = new FitbitClient();
        var token = userInfo.UserToken;
        var refreshToken = userInfo.RefreshToken;

        var progressCount = 0;

        _logger.LogInformation("Start upload activity data");

        foreach (var activityData in activityEnumerable)
        {
            var response = await UploadWithPolicy(async userToken =>
            {
                var response = await fitBitClient.CreateActivityLog(
                    new CreateActivityLogRequest(userInfo.UserId, activityData.ActivityCode, activityData.ActivityStart,
                        activityData.Duration, activityData.Distance, ConvertDistanceUnit(activityData.DistanceUnit),
                        activityData.BurnedCalories), userToken, cancellationToken);

                return response;
            }, fitBitClient, token, refreshToken, clientInfo, retryPolicy, cancellationToken);

            progressCount++;
            token = response.Token;
            refreshToken = response.RefreshToken;
            
            _logger.LogInformation("Processed {Count} activity data", progressCount);

            if (response.Quota.RemainingCalls > 0)
                continue;

            _logger.LogWarning("Remaining calls quota is 0. Delay for {Delay}", response.Quota.TimeToLimitReset);

            await Task.Delay(response.Quota.TimeToLimitReset, cancellationToken);
        }

        _logger.LogInformation("Upload sleep data complete. Uploaded {Count}", progressCount);
    }

    private async Task<(ApiCallsQuota Quota, string Token, string? RefreshToken)> UploadWithPolicy(Func<string, Task<FitbitResponse>> uploadFunc,
        FitbitClient fitbitClient, string userToken, string? refreshUserToken, ClientInfo? clientInfo,
        RetryPolicy? retryPolicy, CancellationToken cancellationToken)
    {
        var isUploaded = false;
        var retriesLeft = retryPolicy?.RetryCount ?? 0;
        var token = userToken;
        var refreshToken = refreshUserToken;
        ApiCallsQuota? quota = null;

        do
        {
            try
            {
                var response = await uploadFunc(token);
                quota = response.Quota;

                isUploaded = true;
            }
            catch (TooManyRequestsFitbitClientException tmrfce)
            {
                var timeToWait = tmrfce.TimeToReset ?? TimeSpan.FromMinutes(2);
                _logger.LogWarning("Too many request, waiting for {TimeToReset}", timeToWait);

                await Task.Delay(timeToWait, cancellationToken);
            }
            catch (UnauthorizedFitbitClientException ufcle)
            {
                if (clientInfo == null || refreshToken == null)
                    throw;

                _logger.LogWarning(ufcle, "Failed to upload data. Trying refresh token");

                var refreshedToken = await fitbitClient.RefreshToken(clientInfo.ClientId, clientInfo.ClientSecret,
                    refreshToken, cancellationToken);

                token = refreshedToken.AccessToken;
                refreshToken = refreshedToken.RefreshToken;

                _logger.LogWarning("Obtained new token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload exception occured. Retries left: {RetryCount}", retriesLeft);

                if (retriesLeft > 0)
                {
                    retriesLeft--;
                    await Task.Delay(retryPolicy!.WaitBetweenRetries, cancellationToken);
                }
                else
                    throw;
            }
        } while (!isUploaded);

        return (quota!, token, refreshToken);
    }

    private static FitbitApi.DataModels.Activity.DistanceUnit ConvertDistanceUnit(DistanceUnit distanceUnit) =>
        distanceUnit switch
        {
            DistanceUnit.Steps => FitbitApi.DataModels.Activity.DistanceUnit.Steps,
            DistanceUnit.Kilometers => FitbitApi.DataModels.Activity.DistanceUnit.Kilometers,
            _ => throw new ArgumentOutOfRangeException(nameof(distanceUnit), distanceUnit, null)
        };
}