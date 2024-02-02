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
    
    private AndroidJavaObject p2pObj;
    
    private NetworkUtils networkUtils;

    [SerializeField]
    private TextMeshProUGUI sendStr;

    void Start() {
        #if UNITY_ANDROID
        networkUtils = new AndroidNetwork();
        networkUtils.setRoomCode("1234");
        name.text = networkUtils.getName();
        #endif
    }

    void Update() {
        
    }

    // to be called from UI
    public void startDiscovering() {
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
        string TESTSTRING = "{\"messageID\":123456,\"messageType\":\"Text\",\"sentFrom\":\"94C1\",\"messageInfo\":{\"type\":0, \"data\":\""+ sendStr.text +"\"}}";
        networkUtils.broadcast(TESTSTRING);
    }

    public void updateString() {
        dataStr.text = networkUtils.processNewMessage();
    }
} 
