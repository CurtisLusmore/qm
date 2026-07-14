using System.Text.Json.Serialization;

namespace be.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]

public enum TitleType
{
    Movie,
    Series,
    Episode
}
