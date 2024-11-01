using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public abstract class BaseController : Controller
{
    protected int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }
        return null;
    }
}