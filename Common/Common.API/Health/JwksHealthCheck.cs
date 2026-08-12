using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Common.API.Health;

internal sealed class JwksHealthCheck(IOptionsMonitor<JwtBearerOptions> bearerOptions) : IHealthCheck
{
    internal const string Name = "jwks";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        JwtBearerOptions options = bearerOptions.Get(JwtBearerDefaults.AuthenticationScheme);
        if (options.ConfigurationManager is null)
        {
            return HealthCheckResult.Healthy("Token validation does not resolve discovery metadata.");
        }

        OpenIdConnectConfiguration configuration =
            await options.ConfigurationManager.GetConfigurationAsync(cancellationToken);

        return configuration.SigningKeys.Count > 0
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("The issuer published no signing keys.");
    }
}
