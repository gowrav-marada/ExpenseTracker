using ExpenseTracker.DTOs;

namespace ExpenseTracker.Services;

public interface IAuthService
{
    Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto);
}
