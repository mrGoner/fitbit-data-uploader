using System.Security.Cryptography;
using System.Text;
using FitbitApi.DataModels.Authorization;

namespace FitbitApi.Helpers;

internal static class AuthHelper
{

    public static async Task<string> GenerateCodeChallenge(string codeVerifier, CancellationToken cancellationToken)
    {
        using var shaCrypt = SHA256.Create();

        using var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(codeVerifier));

        var hash = await shaCrypt.ComputeHashAsync(memoryStream, cancellationToken);

        return Convert.ToBase64String(hash).Replace("=", "").Replace("+", "-").Replace(@"/", "_");
    }

    public static string GenerateCodeVerifier()
    {
        var random = new Random();
        
        var secretCodeLenght = random.Next(43, 129);

        var builder = new StringBuilder(secretCodeLenght);
        
        for (int i = 0; i < secretCodeLenght; i++)
        {
            builder.Append(random.Next(0, 10));
        }

        return builder.ToString();
    }
}