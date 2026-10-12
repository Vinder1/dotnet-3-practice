using System.Security.Claims;

namespace ApprovalService.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        if (int.TryParse(value, out var userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("The access token does not contain a valid user identifier.");
    }

    public static string GetUsername(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Name)
            ?? principal.FindFirstValue("unique_name")
            ?? string.Empty;
    }
}
