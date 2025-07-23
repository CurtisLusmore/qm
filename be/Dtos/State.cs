using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace qm.Dtos;

/// <summary>
/// Torrent state
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum State
{
    /// <summary>
    /// Stopped
    /// </summary>
    [EnumMember(Value = "Stopped")]
    Stopped,

    /// <summary>
    /// Initializing
    /// </summary>
    [EnumMember(Value = "Initializing")]
    Initializing,

    /// <summary>
    /// Downloading
    /// </summary>
    [EnumMember(Value = "Downloading")]
    Downloading,

    /// <summary>
    /// Paused
    /// </summary>
    [EnumMember(Value = "Paused")]
    Paused,

    /// <summary>
    /// Error
    /// </summary>
    [EnumMember(Value = "Error")]
    Error,
}
