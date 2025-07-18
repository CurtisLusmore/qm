using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace qm.Dtos;

/// <summary>
/// Torrent file priority
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Priority
{
    /// <summary>
    /// Skip
    /// </summary>
    [EnumMember(Value = "Skip")]
    Skip,

    /// <summary>
    /// Normal
    /// </summary>
    [EnumMember(Value = "Normal")]
    Normal,

    /// <summary>
    /// High
    /// </summary>
    [EnumMember(Value = "High")]
    High,
}
