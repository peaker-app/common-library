using Common.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Common.API.Security;

public static class JwtExtensions
{
    public static IServiceCollection AddCommonJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        JwtOptions options = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(bearer =>
            {
                bearer.MapInboundClaims = false;
                bearer.Authority = options.Authority;
                bearer.RequireHttpsMetadata = options.RequireHttpsMetadata;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,
                    ValidateAudience = true,
                    ValidAudience = options.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = "sub",
                    RoleClaimType = PeakerRoles.ClaimType,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddCommonAuthorization();

        return services;
    }
}
