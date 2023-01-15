using System.Text.Json.Serialization;

namespace FitbitApi.DataModels.Activity;

public record GetAllActivitiesResponse(
    [property: JsonPropertyName("categories")]
    GetAllActivitiesResponse.Category[] Categories)
{
    public record Category(
        [property: JsonPropertyName("id")] 
        int Id, 
        [property: JsonPropertyName("name")] 
        string Name,
        [property: JsonPropertyName("activities")]
        Activity[] Activities);
    
    public record Activity(
        [property: JsonPropertyName("id")] 
        int Id,
        [property: JsonPropertyName("mets")] 
        float Mets,
        [property: JsonPropertyName("name")]
        string Name,
        [property: JsonPropertyName("maxSpeedMPH")] 
        int? MaxSpeed,
        [property: JsonPropertyName("minSpeedMPH")] 
        int? MinSpeed,
        [property: JsonPropertyName("hasSpeed")] 
        bool HasSpeed);
}