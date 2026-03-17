using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ExpenseTracker.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("googleId")]
    public string GoogleId { get; set; } = null!;

    [BsonElement("email")]
    public string Email { get; set; } = null!;

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("pictureUrl")]
    public string? PictureUrl { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("lastLoginAt")]
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
}
