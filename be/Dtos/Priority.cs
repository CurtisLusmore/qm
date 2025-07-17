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
    /// Do not download
    /// </summary>
    [EnumMember(Value = "Do Not Download")]
    DoNotDownload,

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
