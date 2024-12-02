using System.Security.Claims;
using Kartverket.Database;
using Kartverket.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(ApplicationDbContext context, ILogger<UserService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public int GetUserId(int id)
    {
        if (id != 0) return id;

        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
            _logger.LogInformation("User id retrieved: {userId}", userId);
            return userId;
        }

        _logger.LogInformation("Could not retrieve user id from claims or URL.");
        return 0;
    }

    public async Task<Users?> GetUserAsync(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null)
        {
            _logger.LogInformation("User not found for id: {id}", id);
        }
        return user;
    }
    
    public async Task<Users?> GetUserByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            _logger.LogWarning("GetUserByUsernameAsync: Empty or null username provided.");
            return null;
        }

        var lowerUsername = username.ToLower();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == lowerUsername);

        if (user == null)
        { 
            _logger.LogInformation("User not found with username: {username}", username);
        }
        else
        {
            _logger.LogInformation("User found: {username}", username);
        }

        return user;
    }
}
    
    



