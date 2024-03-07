using Newtonsoft.Json;

public class Message {
    private string messageID;
    public string sentFrom;
    [JsonProperty("messageInfo")]
    public MessageInfo messageInfo{get; set;}
    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    #if UNITY_EDITOR
    public Message(string messageID, string sentFrom, MessageInfo msgInfo) {
        this.sentFrom = sentFrom;
        this.messageID = messageID;
        this.messageInfo = msgInfo;
    }
    public static Message createMessage(MessageInfo msgInfo) {
        Message msg = new Message("", "", msgInfo);
        return msg;
    }
    #endif
}