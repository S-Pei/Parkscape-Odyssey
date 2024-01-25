using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TestScript : MonoBehaviour 
{

    // [SerializeField]
    // private TextMeshProUGUI testInt;

    // [SerializeField]
    // private TextMeshProUGUI testString;

    [SerializeField]
    private TextMeshProUGUI name;

    [SerializeField]
    private TextMeshProUGUI endpointListStr;
    
    private AndroidJavaObject p2pObj;

    void Start() {
        Debug.Log("runs here");
        p2pObj = new AndroidJavaObject("com.example.p2pnetwork.ManualConnect");
        // int javaInt = p2pObj.Call<int>("getInt");
        // string javaStr = p2pObj.Call<string>("getString");
        // testInt.text = javaInt.ToString();
        // testString.text = javaStr;
        name.text = p2pObj.Call<string>("getDeviceName");
        
    }

    // to be called from UI
    public void startDiscovering() {
        p2pObj.Call("startDiscovering");
    }

    // to be called from UI
    public void startAdvertising() {
        p2pObj.Call("startAdvertising");
    }

    // to be called from UI
    public void getDiscoveredEndpointList() {
        endpointListStr.text = p2pObj.Call<string>("getDiscoveredEndpointList");
    }
} 