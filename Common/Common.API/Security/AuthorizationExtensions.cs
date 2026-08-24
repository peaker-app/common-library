using Common.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Common.API.Security;

public static class AuthorizationExtensions
{
    public const string AuthenticatedPolicyName = "authenticated";

    public const string AdminPolicyName = "admin";

    public static IServiceCollection AddCommonAuthorization(this IServiceCollection services) =>
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthenticatedPolicyName, policy => policy.RequireAuthenticatedUser());

            options.AddPolicy(
                AdminPolicyName,
                policy => policy.RequireAuthenticatedUser().RequireClaim(PeakerRoles.ClaimType, PeakerRoles.Admin));
        });
}
