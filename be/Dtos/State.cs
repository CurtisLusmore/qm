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
    /// Completing
    /// </summary>
    [EnumMember(Value = "Completing")]
    Completing,

    /// <summary>
    /// Complete
    /// </summary>
    [EnumMember(Value = "Complete")]
    Complete,

    /// <summary>
    /// Removing
    /// </summary>
    [EnumMember(Value = "Removing")]
    Removing,

    /// <summary>
    /// Removed
    /// </summary>
    [EnumMember(Value = "Removed")]
    Removed,

    /// <summary>
    /// Error
    /// </summary>
    [EnumMember(Value = "Error")]
    Error,
}
