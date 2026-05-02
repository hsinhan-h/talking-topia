using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Web.Models.MongoDB.Entities;

public class Message
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId MessageId { get; set; }

    [BsonElement("ConversationId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId ConversationId { get; set; }

    [BsonElement("SenderId")]
    public string SenderId { get; set; }

    [BsonElement("SenderName")]
    public string SenderName { get; set; }

    [BsonElement("ReceiverId")]
    public string ReceiverId { get; set; }

    [BsonElement("ReceiverName")]
    public string ReceiverName { get; set; }

    [BsonElement("Content")]
    public string Content { get; set; }

    [BsonElement("MessageType")]
    [BsonRepresentation(BsonType.String)]
    public MessageType MessageType { get; set; }

    [BsonElement("Timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonElement("Visibility")]
    public Visibility Visibility { get; set; }
}

public enum MessageType
{
    Sent,
    Read,
    Deleted,
}

public class Visibility
{
    [BsonElement("Sender")]
    public bool Sender { get; set; }

    [BsonElement("Receiver")]
    public bool Receiver { get; set; }
}