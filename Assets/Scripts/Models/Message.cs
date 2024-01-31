using Newtonsoft.Json;
public abstract class Message {
    private string messageID;
    private string synAckFlag;
    private string type;
    private string sentFrom;
    private string messageInfo{get; set;}
    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    // to change back to void (string for testing)
    public string processMessage() {
        // To add different classes
        if (type.Equals("test")) {
            TestMessageInfo messageInfo = JsonConvert.DeserializeObject<TestMessageInfo>(this.messageInfo);
            return messageInfo.processMessageInfo();
        } else {
            return "not test type, something's wrong";
        }
    }
}