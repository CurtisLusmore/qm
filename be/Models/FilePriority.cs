using System.Text.Json.Serialization;

namespace be.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FilePriority
{
    Skip,
    Low,
    Normal,
    High,
}
