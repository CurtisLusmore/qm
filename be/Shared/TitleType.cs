using System.Text.Json.Serialization;

namespace be.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]

public enum TitleType
{
    Movie,
    Series,
    Episode
}
