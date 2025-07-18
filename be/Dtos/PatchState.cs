using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace qm.Dtos;

/// <summary>
/// Torrent patch state
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PatchState
{
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
}
