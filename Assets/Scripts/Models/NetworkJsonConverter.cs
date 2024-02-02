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
        Debug.Log("jsonObject: " + jsonObject.ToString());

        // Determine the type based on the "type" field
        if (jsonObject["type"] == null)
        {
            throw new JsonSerializationException("type field is missing");
        }
        int type = jsonObject.Value<int>("type");
        Debug.Log("type: " + type);
        if (Enum.IsDefined(typeof(MessageType), type))
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
