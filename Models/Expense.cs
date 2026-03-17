using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ExpenseTracker.Models;

public class Expense
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("userId")]
    public string UserId { get; set; } = null!;

    [BsonElement("title")]
    public string Title { get; set; } = null!;

    [BsonElement("amount")]
    public decimal Amount { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = "INR";

    [BsonElement("categoryId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CategoryId { get; set; } = null!;

    [BsonElement("note")]
    public string? Note { get; set; }

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
