using System.Security.Claims;

namespace ExpenseTracker.Services;

public interface IUserContext
{
    string UserId { get; }
}

public class UserContext : IUserContext
{
    public string UserId { get; }

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        UserId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }
}
