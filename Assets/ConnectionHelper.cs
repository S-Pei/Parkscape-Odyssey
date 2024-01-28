using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class ConnectedEndpoint
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("endpointID")]
    public string EndpointID { get; set; }

    [JsonProperty("endpointName")]
    public string EndpointName { get; set; }

    [JsonProperty("payloads")]
    public Payload[] Payloads { get; set; }
}

public class DiscoveredEndpoint
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("endpointID")]
    public string EndpointID { get; set; }

    [JsonProperty("endpointName")]
    public string EndpointName { get; set; }
}

public class ConnectionRequest
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("endpointID")]
    public string EndpointID { get; set; }

    [JsonProperty("endpointName")]
    public string EndpointName { get; set; }

    [JsonProperty("pin")]
    public string Pin { get; set; }
}

public class Payload
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("type")]
    public PayloadType Type { get; set; }

    [JsonProperty("status")]
    public Status Status { get; set; }

    [JsonProperty("isIncoming")]
    public bool IsIncoming { get; set; }

    [JsonProperty("data")]
    public string Data { get; set; }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum PayloadType
{
    Bytes,
    Stream,
    File
}

public class Status
{
    // You need to handle the associated value in 'InProgress' status
    [JsonProperty("inProgress")]
    public Progress InProgress { get; set; }
    [JsonProperty("success")]
    public bool Success { get; set; }
    [JsonProperty("failure")]
    public bool Failure { get; set; }
    [JsonProperty("canceled")]
    public bool Canceled { get; set; }

    [JsonConstructor]
    public Status(Progress inProgress = null, bool success = false, bool failure = false, bool canceled = false)
    {
        InProgress = inProgress;
        Success = success;
        Failure = failure;
        Canceled = canceled;
    }
}

public class Progress
{
    [JsonProperty("totalUnitCount")]
    public long totalUnitCount { get; set; }

    [JsonProperty("completedUnitCount")]
    public long completedUnitCount { get; set; }
}