using Common.API.Health;
using Common.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Common.API.Security;

public static class JwtExtensions
{
    private const string DiscoveryPath = "/.well-known/openid-configuration";

    public static IServiceCollection AddCommonJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        JwtOptions options = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                bound => bound.IsValid(),
                $"La sección '{JwtOptions.SectionName}' exige autoridad, emisor y al menos una audiencia: sin " +
                "audiencia el servicio arrancaría rechazando todos los tokens con un 401 indistinguible de un " +
                "token inválido.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(bearer => Configure(bearer, options));

        services.AddHealthChecks().AddCheck<JwksHealthCheck>(JwksHealthCheck.Name);
        services.AddCommonAuthorization();

        return services;
    }

    private static void Configure(JwtBearerOptions bearer, JwtOptions options)
    {
        bearer.MapInboundClaims = false;
        bearer.Authority = options.Authority;
        bearer.RequireHttpsMetadata = options.RequireHttpsMetadata;
        bearer.ConfigurationManager = CreateConfigurationManager(options);
        bearer.TokenValidationParameters = CreateValidationParameters(options);
    }

    private static ConfigurationManager<OpenIdConnectConfiguration> CreateConfigurationManager(JwtOptions options) =>
        new(
            string.Concat(options.Authority.TrimEnd('/'), DiscoveryPath),
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever { RequireHttps = options.RequireHttpsMetadata })
        {
            AutomaticRefreshInterval = options.MetadataAutomaticRefreshInterval,
            RefreshInterval = options.MetadataRefreshInterval,
            LastKnownGoodLifetime = options.MetadataLastKnownGoodLifetime
        };

    private static TokenValidationParameters CreateValidationParameters(JwtOptions options) =>
        new()
        {
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudiences = options.Audiences,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidateWithLKG = true,
            NameClaimType = "sub",
            RoleClaimType = PeakerRoles.ClaimType,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
}
