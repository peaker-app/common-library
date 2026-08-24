using System.Security.Claims;
using Common.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Common.API.Security;

public sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId => GetUserId();

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) =>
        httpContextAccessor.HttpContext?.User
            .FindAll(PeakerRoles.ClaimType)
            .Any(claim => string.Equals(claim.Value, role, StringComparison.Ordinal)) ?? false;

    private Guid GetUserId()
    {
        ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
        string? subject = user?.FindFirstValue("sub") ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(subject, out Guid userId)
            ? userId
            : throw new InvalidOperationException("The current request has no authenticated user.");
    }
}
