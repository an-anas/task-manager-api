using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TaskManager.Models
{
    [ExcludeFromCodeCoverage]
    public class TaskItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [SwaggerSchema(ReadOnly = true)]
        [StringLength(24, MinimumLength = 24)]
        public string? Id { get; set; }

        [JsonIgnore]
        [BsonElement("userId")]
        [SwaggerSchema(ReadOnly = true)]
        public string? UserId { get; set; }

        [Required]
        [BsonElement("title")]
        public required string Title { get; set; }

        [BsonElement("completed")]
        [DefaultValueAttribute(false)]
        public required bool Completed { get; set; }

        [BsonElement("description")]
        public string? Description { get; set; }

        // Parameterless constructor needed for MongoDB deserialization
        public TaskItem() { }
    }
}