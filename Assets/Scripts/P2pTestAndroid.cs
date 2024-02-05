using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class P2pTestAndroid : MonoBehaviour 
{
    [SerializeField]
    private TextMeshProUGUI name;

    [SerializeField]
    private TextMeshProUGUI endpointListStr;

    [SerializeField]
    private TextMeshProUGUI dataStr;
    
    private NetworkUtils networkUtils;

    void Start() {
        networkUtils = NetworkManager.Instance.NetworkUtils;
        networkUtils.setRoomCode("1234");
        name.text = networkUtils.getName();
        InvokeRepeating("OnReceive", 0.0f, 1.0f);
    }

    void Update() {
        // System.Func<MessageInfo, CallbackStatus> callback = (MessageInfo msgInfo) => {
        //     return CallbackStatus.PROCESSED;
        // };
        // dataStr.text = networkUtils.onReceive(callback);
    }

    // to be called from UI
    public void startDiscovering() {
        networkUtils.stopDiscovering();
        networkUtils.startDiscovering();
    }

    // to be called from UI
    public void startAdvertising() {
        networkUtils.startAdvertising();
    }

    // to be called from UI
    public void getDiscoveredEndpointList() {
        Debug.Log("getting endpoints");
        endpointListStr.text = string.Join(",", networkUtils.getDiscoveredDevices());
    }

    // broadcasts "hello world" hardcoded string (called from UI)
    public void broadcastString() {
        string TESTSTRING = "{\"messageID\":123456,\"messageType\":\"Text\",\"sentFrom\":\"94C1\",\"messageInfo\":{\"type\":\"TEST\", \"data\":\"helloworld\"}}";
        networkUtils.broadcast(TESTSTRING);
    }

    public void OnReceive() {
        System.Func<Message, CallbackStatus> callback = (Message msg) => {
            if (msg.messageInfo.messageType == MessageType.TEST) {
                TestMessageInfo testMsgInfo = (TestMessageInfo) msg.messageInfo;
                Debug.Log("ran callback");
                Debug.Log("received message: " + testMsgInfo.data);
                dataStr.text = testMsgInfo.data;
                return CallbackStatus.PROCESSED;
            }
            return CallbackStatus.NOT_PROCESSED;
        };
        networkUtils.onReceive(callback);
    }
} 
