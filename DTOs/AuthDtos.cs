using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.DTOs;

public record GoogleLoginDto(
    [Required] string IdToken
);

public record AuthResponseDto(
    string AccessToken,
    string Email,
    string Name,
    string? PictureUrl
);
