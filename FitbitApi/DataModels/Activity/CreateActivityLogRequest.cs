namespace FitbitApi.DataModels.Activity;

//https://dev.fitbit.com/build/reference/web-api/activity/create-activity-log
public record CreateActivityLogRequest(string UserId, int ActivityId, DateTime ActivityStart, TimeSpan Duration,
    decimal Distance, DistanceUnit DistanceUnit, int? BurnedCalories);