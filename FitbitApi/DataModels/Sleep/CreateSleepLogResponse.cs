using System.Text.Json.Serialization;

namespace FitbitApi.DataModels.Sleep;

//https://dev.fitbit.com/build/reference/web-api/sleep/create-sleep-log
public record CreateSleepLogResponse(
    [property:JsonPropertyName("sleep")] 
    CreateSleepLogResponse.SleepLog Sleep)
{
    public record SleepLog(
        [property: JsonPropertyName("startTime")]
        DateTime StartTime, 
        [property: JsonPropertyName("duration")]
        int Duration, 
        [property: JsonPropertyName("efficiency")]
        int Efficiency, 
        [property: JsonPropertyName("endTime")]
        DateTime EndTime, 
        [property: JsonPropertyName("infoCode")]
        int InfoCode, 
        [property: JsonPropertyName("isMainSleep")]
        bool IsMainSleep,
        [property: JsonPropertyName("logId")] 
        long LogId,
        [property: JsonPropertyName("minutesAfterWakeup")]
        int MinutesAfterWakeup,
        [property: JsonPropertyName("minutesAsleep")]
        int MinutesAsleep,
        [property: JsonPropertyName("minutesAwake")]
        int MinutesAwake,
        [property: JsonPropertyName("minutesToFallAsleep")]
        int MinutesToFallAsleep,
        [property:JsonPropertyName("timeInBed")]
        int TimeInBed,
        [property:JsonPropertyName("type")]
        [property:JsonConverter(typeof(JsonStringEnumConverter))]
        SleepLogType LogType
    );
    
    public enum SleepLogType
    {
        Classic,
        Stages
    }
}