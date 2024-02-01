using UnityEngine;
using System.Collections.Generic;

#if UNITY_ANDROID
public class AndroidNetwork : NetworkUtils {
    private AndroidJavaObject p2pObj;
    private NetworkUtils networkUtils;

    public AndroidNetwork() {
        this.messageIDs = new HashSet<string>();
        AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        p2pObj = unityClass.GetStatic<AndroidJavaObject>("currentActivity"); 
    }

    public override void startDiscovering() {
        p2pObj.Call("startDiscovering");
    }

    public override void startAdvertising() {
        p2pObj.Call("startAdvertising");
    }

    public override void stopDiscovering() {
        p2pObj.Call("stopDiscovering");
    }

    public override void stopAdvertising() {
        p2pObj.Call("stopAdvertising");
    }
    public override List<string> getConnectedDevices() {
        int numConnected = p2pObj.Call<int>("getNumberOfConnectedDevices");
        List<string> connectedDevices = new List<string>();
        for (int i = 0; i < numConnected; i++) {
            string connectedDevice = p2pObj.Call<string>("getConnectedDevice", i);
            connectedDevices.Add(connectedDevice);
        }
        return connectedDevices;
    }

    public override List<string> getDiscoveredDevices() {
        int numDiscovered = p2pObj.Call<int>("getNumberOfDiscoveredDevices");
        List<string> discoveredDevices = new List<string>();
        for (int i = 0; i < numDiscovered; i++) {
            string discoveredDevice = p2pObj.Call<string>("getDiscoveredDevice", i);
            discoveredDevices.Add(discoveredDevice);
        }
        return discoveredDevices;
    }

    public override void broadcast(string message) {
        Debug.Log("starting to broadcast message " + message);
        p2pObj.Call("broadcast", message);
        Debug.Log("broadcasted message " + message);
    }
    public override void send(string message, string deviceID) {
        p2pObj.Call("send", message, deviceID);
    }

    public override void setRoomCode(string roomCode) {
        p2pObj.Call("setName", roomCode);
    }

    public override string getMessageReceived() {
        return p2pObj.Call<string>("getNextReceivedMessage");
    }

    public override string getName() {
        return p2pObj.Call<string>("getName");
    }

    /* Initialisation done automatically in the Unity Player Activity life cycle in Android Plugin. */
    public override void initP2P() {}

}
#endif