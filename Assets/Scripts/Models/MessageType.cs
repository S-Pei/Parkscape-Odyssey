using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum MessageType {
    TEST,
    BATTLEMESSAGE,
}