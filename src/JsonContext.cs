using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AudioToggle
{
    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(List<string>))]
    internal partial class AudioToggleJsonContext : JsonSerializerContext
    {
    }
}
