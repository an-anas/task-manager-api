using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class TaskItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [SwaggerSchema(ReadOnly = true)]
        [StringLength(24, MinimumLength = 24)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public required string Title { get; set; }

        [BsonElement("completed")]
        public required bool Completed { get; set; }

        [BsonElement("description")] 
        public string? Description { get; set; }

        // Parameterless constructor needed for MongoDB deserialization
        public TaskItem() { }

        // Constructor for easier instantiation
    }
}