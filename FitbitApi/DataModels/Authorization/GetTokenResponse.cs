using System.Text.Json.Serialization;

namespace FitbitApi.DataModels.Authorization;

public record TokenInfo(
    [property: JsonPropertyName("access_token")]
    string AccessToken,
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn,
    [property: JsonPropertyName("token_type")]
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    TokenType TokenType,
    [property: JsonPropertyName("refresh_token")]
    string RefreshToken,
    [property: JsonPropertyName("user_id")]
    string UserId);