public interface MessageInfo {
    MessageType messageType {get; set;}
    public string toJson();
    public string processMessageInfo();
}