using CsvDataLoader;
using FitbitApi;
using FitbitApi.DataModels.Authorization;
using FitbitDataUploader;
using FitbitDataUploader.Abstractions.Models;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace FitbitConsoleApp;

public static class Program
{
    private static readonly ILoggerFactory LoggerFactory;
    private const string ClientIdEnvVariable = "FITBIT_CLIENT_ID";
    private const string ClientSecretEnvVariable = "FITBIT_CLIENT_SECRET";
    private const string UserIdEnvVariable = "FITBIT_USER_ID";
    private const string UserTokenEnvVariable = "FITBIT_USER_TOKEN";
    private const string UserRefreshTokenEnvVariable = "FITBIT_USER_REFRESH_TOKEN";
    private const string ActivityFilePathEnvVariable = "FITBIT_ACTIVITY_FILE_PATH";
    private const string SleepFilePathEnvVariable = "FITBIT_SLEEP_FILE_PATH";
    
    private const string LogsDirEnvVariable = "FITBIT_UPLOADER_LOGS_DIR";
    
    static Program()
    {
        var logsDir = Environment.GetEnvironmentVariable(LogsDirEnvVariable) ?? AppDomain.CurrentDomain.BaseDirectory;
        
        LoggerFactory = new NLogLoggerFactory();
        LogManager.Configuration.Variables["logs_dir"] = logsDir;
    }
    
    public static async Task Main(string[] args)
    {
        var isManualMode = args.Contains("-manual");
        var logger = LoggerFactory.CreateLogger(typeof(Program));
        
        try
        {
            using var cts = new CancellationTokenSource();
            
            Console.CancelKeyPress += (_, _) => cts.Cancel();
            
            if (isManualMode)
            {
                var fitbitClient = new FitbitClient();

                Console.WriteLine("Past clientId");
                var clientId = Console.ReadLine() ?? throw new Exception("client id must be set");

                Console.WriteLine("Past client secret");
                var clientSecret = Console.ReadLine() ?? throw new Exception("client secret must be set");

                var authData = await fitbitClient.GetAuthorizeData(clientId,
                    new[] {Scope.Profile, Scope.Activity, Scope.Sleep},
                    CancellationToken.None);

                Console.WriteLine($"Copy link and past auth code: {authData.AuthorizeUrl}");

                var authCode = Console.ReadLine() ?? throw new Exception("auth code must be set");
                
                var tokenInfo =
                    await fitbitClient.GetToken(clientId, clientSecret, authCode, authData.CodeVerifier, "", cts.Token);

                Console.WriteLine("Past path to activity csv");
                var pathToActivityCsv = Console.ReadLine();

                Console.WriteLine("Past path to sleep csv");
                var pathToSleepCsv = Console.ReadLine();

                await Upload(clientId, clientSecret, tokenInfo.UserId, tokenInfo.AccessToken, tokenInfo.RefreshToken,
                    pathToActivityCsv, pathToSleepCsv, cts.Token);

                Console.WriteLine("All upload completed. Press any key to exit");

                Console.ReadKey();
            }
            else
            {
                var clientId = Environment.GetEnvironmentVariable(ClientIdEnvVariable) ??
                               throw new Exception($"{ClientIdEnvVariable} must be set");
                
                var clientSecret = Environment.GetEnvironmentVariable(ClientSecretEnvVariable) ??
                                   throw new Exception($"{ClientSecretEnvVariable} must be set");
                
                var userId = Environment.GetEnvironmentVariable(UserIdEnvVariable) ??
                             throw new Exception($"{UserIdEnvVariable} must be set");
                
                var userToken = Environment.GetEnvironmentVariable(UserTokenEnvVariable) ??
                             throw new Exception($"{UserTokenEnvVariable} must be set");

                var userRefreshToken = Environment.GetEnvironmentVariable(UserRefreshTokenEnvVariable);

                var activityFilePath = Environment.GetEnvironmentVariable(ActivityFilePathEnvVariable);
                var sleepFilePath = Environment.GetEnvironmentVariable(SleepFilePathEnvVariable);

                await Upload(clientId, clientSecret, userId, userToken, userRefreshToken, activityFilePath,
                    sleepFilePath, cts.Token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            logger.LogError(ex, "Fatal exception occured");

            Environment.Exit(-1);
        }
    }

    private static async Task Upload(string clientId, string clientSecret, string userId, string userToken,
        string? userRefreshToken, string? pathToActivityCsv, string? pathToSleepCsv,
        CancellationToken cancellationToken)
    {
        var csvLoader = new CsvLoader();
        var dataUploader = new FitbitUploader(LoggerFactory.CreateLogger<FitbitUploader>());

        if (pathToActivityCsv != null)
        {
            var activityData =
                csvLoader.LoadCustomData<CustomActivityData>(pathToActivityCsv, new LoadConfiguration(true, ","),
                    CancellationToken.None).Where(data => data.Steps > 0).Select(
                    data => new ActivityData
                    {
                        Distance = data.Steps,
                        DistanceUnit = DistanceUnit.Steps,
                        ActivityStart = data.Date + data.Start,
                        Duration = data.Stop - data.Start,
                        BurnedCalories = data.Calories,
                        ActivityCode = 90013
                    }).ToEnumerable();

            await dataUploader.UploadActivityData(
                new UserInfo(userId, userToken, userRefreshToken),
                new ClientInfo(clientId, clientSecret), activityData, new RetryPolicy(5, TimeSpan.FromSeconds(30)),
                cancellationToken);
        }

        if (pathToSleepCsv != null)
        {
            var sleepData =
                csvLoader.LoadCustomData<CustomSleepData>(pathToSleepCsv, new LoadConfiguration(true, ","),
                    CancellationToken.None).Where(data => data.Stop != data.Start).Select(
                    data => new SleepData(data.Start, data.Stop - data.Start)).ToEnumerable();

            await dataUploader.UploadSleepData(
                new UserInfo(userId, userToken, userRefreshToken),
                new ClientInfo(clientId, clientSecret), sleepData, new RetryPolicy(5, TimeSpan.FromSeconds(30)),
                cancellationToken);
        }
    }
}