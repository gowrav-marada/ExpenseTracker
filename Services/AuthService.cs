using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExpenseTracker.DTOs;
using ExpenseTracker.Models;
using ExpenseTracker.Settings;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace ExpenseTracker.Services;

public class AuthService : IAuthService
{
    private readonly IMongoCollection<User> _users;
    private readonly GoogleAuthSettings _googleSettings;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IOptions<MongoDbSettings> mongoSettings,
        IOptions<GoogleAuthSettings> googleSettings,
        IOptions<JwtSettings> jwtSettings)
    {
        var client = new MongoClient(mongoSettings.Value.ConnectionString);
        var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
        _users = database.GetCollection<User>("users");
        _googleSettings = googleSettings.Value;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken,
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_googleSettings.ClientId]
            });

        var user = await _users.Find(u => u.GoogleId == payload.Subject).FirstOrDefaultAsync();

        if (user is null)
        {
            user = new User
            {
                GoogleId = payload.Subject,
                Email = payload.Email,
                Name = payload.Name,
                PictureUrl = payload.Picture
            };
            await _users.InsertOneAsync(user);
        }
        else
        {
            await _users.UpdateOneAsync(
                u => u.Id == user.Id,
                Builders<User>.Update
                    .Set(u => u.Name, payload.Name)
                    .Set(u => u.PictureUrl, payload.Picture)
                    .Set(u => u.LastLoginAt, DateTime.UtcNow));
        }

        return new AuthResponseDto(GenerateJwtToken(user), user.Email, user.Name, user.PictureUrl);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
