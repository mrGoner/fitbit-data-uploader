namespace FitbitApi.DataModels.Sleep;

//https://dev.fitbit.com/build/reference/web-api/sleep/create-sleep-log
public record CreateSleepLogRequest(string UserId, DateTime StartDateTime, TimeSpan Duration);