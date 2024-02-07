using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


public class EncounterController : MonoBehaviour
{

}

public enum EncounterMessageType {
    FOUND_ENCOUNTER,
}


public class EncounterMessage : MessageInfo
{
    public MessageType messageType {get; set;}
    public EncounterMessageType Type {get; set;}

    [JsonConstructor]
    public EncounterMessage(EncounterMessageType type) {
        messageType = MessageType.ENCOUNTERMESSAGE;
        Type = type;
    }

    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    public string processMessageInfo() {
        return "";
    }
}