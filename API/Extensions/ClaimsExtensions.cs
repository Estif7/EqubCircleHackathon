using System.Security.Claims;

namespace EkubCircle.API.Extensions;

public static class ClaimsExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(value, out var id))
            throw new UnauthorizedAccessException("User identity is missing.");
        return id;
    }
}
