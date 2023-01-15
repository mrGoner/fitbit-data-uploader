using System.Text.Json.Serialization;

namespace FitbitApi.DataModels.Activity;

//https://dev.fitbit.com/build/reference/web-api/activity/create-activity-log
public record CreateActivityLogResponse(
    [property: JsonPropertyName("activityLog")]
    CreateActivityLogResponse.ActivityLog Activity)
{
    public record ActivityLog(
        [property: JsonPropertyName("activityId")]
        int ActivityId, 
        [property: JsonPropertyName("activityParentId")]
        int ActivityParentId, 
        [property: JsonPropertyName("activityParentName")]
        string ActivityParentName, 
        [property: JsonPropertyName("calories")]
        int Calories, 
        [property: JsonPropertyName("description")]
        string Description, 
        [property: JsonPropertyName("distance")]
        decimal Distance, 
        [property: JsonPropertyName("duration")]
        int Duration,
        [property: JsonPropertyName("isFavorite")]
        bool IsFavorite, 
        [property: JsonPropertyName("name")] 
        string Name,
        [property: JsonPropertyName("logId")] 
        long LogId);
}