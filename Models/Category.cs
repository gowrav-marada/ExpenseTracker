using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ExpenseTracker.Models;

public class Category
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; } = null!;

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("icon")]
    public string Icon { get; set; } = "??";

    [BsonElement("color")]
    public string Color { get; set; } = "#6366f1";

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
