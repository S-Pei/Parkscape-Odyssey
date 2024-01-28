// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// public class TestScript : MonoBehaviour 
// {

//     // [SerializeField]
//     // private TextMeshProUGUI testInt;

//     // [SerializeField]
//     // private TextMeshProUGUI testString;

//     [SerializeField]
//     private TextMeshProUGUI name;

//     [SerializeField]
//     private TextMeshProUGUI endpointListStr;

//     [SerializeField]
//     private TextMeshProUGUI dataStr;
    
//     private AndroidJavaObject p2pObj;

//     void Start() {
//         Debug.Log("runs here");
//         // p2pObj = new AndroidJavaObject("com.example.p2pnetwork.ManualConnect");
//         AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
//         p2pObj = unityClass.GetStatic<AndroidJavaObject>("currentActivity");     

//         // int javaInt = unityActivity.Call<int>("getInt");

//         // string javaStr = p2pObj.Call<string>("getString");
//         // testInt.text = javaInt.ToString();
//         // testString.text = javaStr;
//         name.text = p2pObj.Call<string>("getDeviceName");
        
//     }

//     void Update() {
//         dataStr.text = getData();
//     }

//     // to be called from UI
//     public void startDiscovering() {
//         p2pObj.Call("startDiscovering");
//     }

//     // to be called from UI
//     public void startAdvertising() {
//         p2pObj.Call("startAdvertising");
//     }

//     // to be called from UI
//     public void getDiscoveredEndpointList() {
//         endpointListStr.text = p2pObj.Call<string>("getDiscoveredEndpointList");
//     }

//     // sends "hello world" hardcoded string (called from UI)
//     public void sendString() {
//         string str = "Hello World";
//         p2pObj.Call("sendString", str);
//     }

//     // gets data sent from other devices
//     public string getData() {
//         string data = p2pObj.Call<string>("getData");
//         return data;
//     }


// } 