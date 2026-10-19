using System.Security.Claims;

namespace ApprovalService.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(value, out var userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("The access token does not contain a valid user identifier.");
    }
}
