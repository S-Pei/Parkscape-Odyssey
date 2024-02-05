using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

public class NetworkJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(MessageInfo);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);

        // Determine the type based on the "type" field
        if (jsonObject["type"] == null)
        {
            throw new JsonSerializationException("type field is missing");
        }
        string type = jsonObject.Value<string>("type");
        
        if (type.Equals(MessageType.TEST.ToString()))
        {
            return jsonObject.ToObject<TestMessageInfo>(serializer);
        }

        throw new JsonSerializationException("Unknown type");
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
