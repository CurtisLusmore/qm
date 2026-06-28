using System.Text.Json.Serialization;

namespace be.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FilePriority
{
    Skip,
    Low,
    Normal,
    High,
}
