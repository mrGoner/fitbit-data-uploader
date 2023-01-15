namespace FitbitApi.DataModels.Authorization;

public record AuthorizeData(string AuthorizeUrl, string CodeVerifier, string CodeChallenge);