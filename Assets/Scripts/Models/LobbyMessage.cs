using System;
using System.Collections.Generic;
using Newtonsoft.Json;
public enum LobbyMessageType {
    JOIN,
    LEADER,
    START
}

public class LobbyMessage : MessageInfo {
    public LobbyMessageType Type {get; set;}
    public string Id {get;}
    public string Message {get;}

    public Dictionary<string, string> Players {get;}
    public bool IsLeader {get;}

    // Unused
    public string type {get; set;}

    public LobbyMessage(LobbyMessageType type, bool isLeader, string id, string message) {
        Type = type;
        IsLeader = isLeader;
        Id = id;
        Message = message;
    }

    public LobbyMessage(bool isLeader, string id, Dictionary<string, string> players) {
        if (!isLeader)
            throw new Exception("Only leader can send this message.");
        Type = LobbyMessageType.LEADER;
        IsLeader = isLeader;
        Id = id;
        Message = JsonConvert.SerializeObject(players);
        Players = players;
    }

    public string toJson() {
        return JsonConvert.SerializeObject(this);
    }

    public static LobbyMessage fromJson(string json) {
        return JsonConvert.DeserializeObject<LobbyMessage>(json);
    }

    public string processMessageInfo() {
        return Id + ": " + Message;
    }

    
}