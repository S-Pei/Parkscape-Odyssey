using Newtonsoft.Json;
public class TestMessageInfo : MessageInfo {
    public string data {get; set;}
    public MessageType messageType {get; set;} = MessageType.TEST;

    public TestMessageInfo(string data) {
        this.data = data;
    }

    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    // to change to void (string for testing)
    public string processMessageInfo() {
        return data;
    }
}