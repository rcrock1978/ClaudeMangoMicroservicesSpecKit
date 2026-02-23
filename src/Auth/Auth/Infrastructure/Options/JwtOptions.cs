namespace Mango.Services.Auth.Infrastructure.Options;

public class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "mango-auth";
    public string Audience { get; set; } = "mango-client";
    public int AccessTokenExpiryMinutes { get; set; } = 30;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
