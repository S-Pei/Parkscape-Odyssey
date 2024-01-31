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

    void Start() {
        #if UNITY_ANDROID
        networkUtils = new AndroidNetwork();
        #endif
    }

    void Update() {
        dataStr.text = networkUtils.processNewMessage();
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
        string TESTSTRING = "{\"messageID\":123456,\"type\":\"test\",\"messageType\":\"Text\",\"sentFrom\":\"94C1\",\"messageInfo\":{\"data\":\"helloworld\"}.}";
        networkUtils.broadcast(TESTSTRING);
    }
} 
