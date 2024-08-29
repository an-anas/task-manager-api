using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskManager.Models
{
    public class TaskItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }

        [BsonElement("title")]
        public required string Title { get; set; }

        [BsonElement("description")] public string? Description { get; set; }

        [BsonElement("completed")]
        public bool? Completed { get; set; }

        // Parameterless constructor needed for MongoDB deserialization
        public TaskItem() { }

        // Constructor for easier instantiation
        public TaskItem(string id, string title, string? description = null, bool? completed = null)
        {
            Id = id;
            Title = title;
            Description = description;
            Completed = completed;
        }
    }
}