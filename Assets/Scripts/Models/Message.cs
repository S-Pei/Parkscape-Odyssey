using Newtonsoft.Json;
using UnityEngine;
public class Message {
    private string messageID;
    private string sentFrom;
    [JsonProperty("messageInfo")]
    public MessageInfo messageInfo{get; set;}
    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    // to change back to void (string for testing)
    public string processMessage() {
        return messageInfo.processMessageInfo();
        
    }
}