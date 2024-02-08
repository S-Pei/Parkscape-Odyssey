using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


public class NetworkManager : MonoBehaviour {
    private static NetworkManager instance;
    
    private NetworkUtils networkUtils;

    private LobbyManager lobbyManager;
    private EncounterController encounterController;

    private readonly float baseFreq = 0.3f; // per second

    private Dictionary<string, (string, int counter)> connectedPlayers = new ();
    private int numConnectedPlayers = 0;

    private readonly float pingFreq = 2f;
    private float pingTimer = 0;


    public static NetworkManager Instance {
        get {
            if (instance == null) {
                // To make sure that script is persistent across scenes
                GameObject go = new GameObject("NetworkManager");
                instance = go.AddComponent<NetworkManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        } else {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        networkUtils = DebugNetwork.Instance;

        #if UNITY_ANDROID
        networkUtils = AndroidNetwork.Instance;
        #endif
        #if UNITY_IOS
        networkUtils = IOSNetwork.Instance;
        #endif
    }
    
    void Start() {
        InvokeRepeating("HandleMessages", 0.0f, baseFreq);
    }

    void Update() {
        if (LobbyManager.selfReference != null) {
            lobbyManager = LobbyManager.selfReference;
        }

        if (EncounterController.selfReference != null) {
            encounterController = EncounterController.selfReference;
        }
    }

    public NetworkUtils NetworkUtils {
        get {
            return networkUtils;
        }
    }

    // Wrapper for sending messages.
    private void HandleMessages() {
        // Set up callback for handling incoming messages.
        Func<Message, CallbackStatus> callback = (Message msg) => {
            return HandleMessage(msg);
        };
        networkUtils.onReceive(callback);

        SendMessages();
    }

    // Handle incoming messages for all managers.
    private CallbackStatus HandleMessage(Message message) {
        switch(message.messageInfo.messageType) {
            case MessageType.PINGMESSAGE:
                // Received a ping message from someone else.
                PingMessageInfo pingMessage = (PingMessageInfo)message.messageInfo;
                if (!connectedPlayers.ContainsKey(pingMessage.playerId)) {
                    connectedPlayers[pingMessage.playerId] = (pingMessage.playerName, 10);
                } else {
                    connectedPlayers[pingMessage.playerId].Item2 = 10;
                    numConnectedPlayers++;
                }
            case MessageType.LOBBYMESSAGE:
                if (lobbyManager != null) {
                    return lobbyManager.HandleMessage(message);
                } else {
                    return CallbackStatus.DORMANT;
                }
            case MessageType.ENCOUNTERMESSAGE:
                if (encounterController != null) {
                    return encounterController.HandleMessage(message);
                } else {
                    return CallbackStatus.DORMANT;
                }
        }
        return CallbackStatus.NOT_PROCESSED;
    }

    // Handle sending messages for all managers.
    private void SendMessages() {
        if (networkUtils == null)
            return;

        // Send ping messages to all connected players every PingFreq.
        if (pingTimer >= pingFreq) {
            PingMessageInfo pingMessage = new PingMessageInfo(PlayerPrefs.GetString("name"));
            networkUtils.broadcast(pingMessage.toJson());
            pingTimer = 0;
        } else {
            pingTimer += baseFreq;
        }
        
        if (lobbyManager != null) {
            lobbyManager.SendMessages();
        }

        if (encounterController != null) {
            encounterController.SendMessages();
        }
    }
}


public class PingMessageInfo : MessageInfo {
        public MessageType messageType { get; set; }
        public string playerId { get; set; }
        public string playerName { get; set; }


        [JsonConstructor]
        public PingMessageInfo(string playerName) {
            this.messageType = MessageType.PINGMESSAGE;
            this.playerId = SystemInfo.deviceUniqueIdentifier;
            this.playerName = playerName;
        }
        
        public string toJson() {
            return JsonConvert.SerializeObject(this);
        }

        public string processMessageInfo() {
            return "";
        }
}