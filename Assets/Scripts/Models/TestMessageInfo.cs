using Newtonsoft.Json;
public class TestMessageInfo : MessageInfo {
    private string data {get; set;}
    public string type {get; set;} = "test";

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