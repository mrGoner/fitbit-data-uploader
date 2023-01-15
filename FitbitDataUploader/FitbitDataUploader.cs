using FitbitApi;
using FitbitApi.DataModels.Activity;
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
    
    public async Task UploadSleepData(UserInfo userInfo, ClientInfo? clientInfo, IEnumerable<SleepData> sleepDataEnumerable, CancellationToken cancellationToken)
    {
        var fitBitClient = new FitbitClient();
        var token = userInfo.UserToken;
        var refreshToken = userInfo.RefreshToken;

        var progressCount = 0;
        
        _logger.LogInformation("Start upload sleep data");

        foreach (var sleepData in sleepDataEnumerable)
        {
            try
            {
                var response = await fitBitClient.CreateSleepLog(
                    new CreateSleepLogRequest(userInfo.UserId, sleepData.SleepStart, sleepData.Duration), token,
                    cancellationToken);

                progressCount++;

                _logger.LogInformation("Processed {Count} sleep data", progressCount);

                if (response.Quota.RemainingCalls > 0)
                    continue;

                _logger.LogWarning("Remaining calls quota is 0. Delay for {Delay}", response.Quota.TimeToLimitReset);
                
                await Task.Delay(response.Quota.TimeToLimitReset, cancellationToken);
            }
            catch (TooManyRequestsFitbitClientException tmrfce)
            {
                _logger.LogWarning("Too many request, waiting for {TimeToReset}",
                    tmrfce.TimeToReset ?? TimeSpan.FromMinutes(2));
                
                if (tmrfce.TimeToReset.HasValue)
                    await Task.Delay(tmrfce.TimeToReset.Value, cancellationToken);
                else
                    await Task.Delay(TimeSpan.FromMinutes(2), cancellationToken);
            }
            catch (UnauthorizedFitbitClientException ufcle)
            {
                if (clientInfo == null || refreshToken == null)
                    throw;
                
                _logger.LogWarning(ufcle, "Failed to upload sleep data. Trying refresh token");

                var refreshedToken = await fitBitClient.RefreshToken(clientInfo.ClientId, clientInfo.ClientSecret,
                    refreshToken, cancellationToken);

                token = refreshedToken.AccessToken;
                refreshToken = refreshedToken.RefreshToken;

                _logger.LogWarning("Obtained new token");
            }
        }

        _logger.LogInformation("Upload sleep data complete. Uploaded {Count}", progressCount);
    }
    
    public async Task UploadActivityData(UserInfo userInfo, ClientInfo? clientInfo, IEnumerable<ActivityData> activityEnumerable, CancellationToken cancellationToken)
    {
        var fitBitClient = new FitbitClient();
        var token = userInfo.UserToken;
        var refreshToken = userInfo.RefreshToken;
        
        var progressCount = 0;
        
        _logger.LogInformation("Start upload activity data");

        foreach (var activityData in activityEnumerable)
        {
            try
            {
                var response = await fitBitClient.CreateActivityLog(
                    new CreateActivityLogRequest(userInfo.UserId, activityData.ActivityCode, activityData.ActivityStart,
                        activityData.Duration, activityData.Distance, ConvertDistanceUnit(activityData.DistanceUnit),
                        activityData.BurnedCalories), token, cancellationToken);

                progressCount++;

                _logger.LogInformation("Processed {Count} activity data", progressCount);

                if (response.Quota.RemainingCalls > 0)
                    continue;

                _logger.LogWarning("Remaining calls quota is 0. Delay for {Delay}", response.Quota.TimeToLimitReset);
                
                await Task.Delay(response.Quota.TimeToLimitReset, cancellationToken);
            }
            catch (TooManyRequestsFitbitClientException tmrfce)
            {
                _logger.LogWarning("Too many request, waiting for {TimeToReset}",
                    tmrfce.TimeToReset ?? TimeSpan.FromMinutes(2));
                
                if (tmrfce.TimeToReset.HasValue)
                    await Task.Delay(tmrfce.TimeToReset.Value, cancellationToken);
                else
                    await Task.Delay(TimeSpan.FromMinutes(2), cancellationToken);
            }
            catch (UnauthorizedFitbitClientException ufcle)
            {
                if (clientInfo == null || refreshToken == null)
                    throw;
                
                _logger.LogWarning(ufcle, "Failed to upload sleep data. Trying refresh token");

                var refreshedToken = await fitBitClient.RefreshToken(clientInfo.ClientId, clientInfo.ClientSecret, refreshToken,
                    cancellationToken);
                
                token = refreshedToken.AccessToken;
                refreshToken = refreshedToken.RefreshToken;
                
                _logger.LogInformation("Obtained new token");
            }
        }

        _logger.LogInformation("Upload sleep data complete. Uploaded {Count}", progressCount);
    }

    private static FitbitApi.DataModels.Activity.DistanceUnit ConvertDistanceUnit(DistanceUnit distanceUnit) =>
        distanceUnit switch
        {
            DistanceUnit.Steps => FitbitApi.DataModels.Activity.DistanceUnit.Steps,
            DistanceUnit.Kilometers => FitbitApi.DataModels.Activity.DistanceUnit.Kilometers,
            _ => throw new ArgumentOutOfRangeException(nameof(distanceUnit), distanceUnit, null)
        };
}