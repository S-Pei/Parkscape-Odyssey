using Newtonsoft.Json;
public class TestMessageInfo : MessageInfo {
    private string data {get; set;}

    public TestMessageInfo(string data) {
        this.data = data;
    }

    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    // public MessageInfo fromJson(string jsonString) {
    //     return JsonConvert.DeserializeObject<TestMessageInfo>(jsonString);
    // }

    // to change to void (string for testing)
    public string processMessageInfo() {
        return data;
    }
}