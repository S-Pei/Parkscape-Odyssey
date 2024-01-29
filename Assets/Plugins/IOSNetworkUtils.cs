using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

public class IOSNetworkUtils:  NetworkUtils {

    [DllImport("__Internal")]
    private static extern void modelInitialize();

    [DllImport("__Internal")]
    private static extern void modelStartDiscovery();

    [DllImport("__Internal")]
    private static extern void modelStartAdvertising();

    [DllImport("__Internal")]
    private static extern IntPtr modelGetEndpointName();

    [DllImport("__Internal")]
    private static extern IntPtr modelSetEndpointName(string name);

    [DllImport("__Internal")]
    private static extern void modelGetDiscoveredEndpoints();

    [DllImport("__Internal")]
    private static extern IntPtr modelGetConnectedEndpoints();

    [DllImport("__Internal")]
    private static extern IntPtr modelGetConnectionRequests();

    [DllImport("__Internal")]
    private static extern void modelBroadcastString(string message);

    [DllImport("__Internal")]
    private static extern void modelSendString(string message, string destinationID);

    /* Discover other endpoints and connect with endpoints with same name (room code) */
    public override void discover() {
        modelStartDiscovery();
    } 

    /* Advertise so that other endpoints can connect. */ 
    public override void advertise() {
        modelStartAdvertising();
    }

    /* Returns list of connected devices by device ID. */
    public override List<string> getConnectedDevices() {
        string jsonString = handleStrPtr(modelGetConnectedEndpoints());
        List<string> list = handleJSONStr(jsonString);
        return list;
    }

    /* Returns a list of discovered devices by device ID. */
    public override List<string> getDiscoveredDevices() {
        string jsonString = handleStrPtr(modelGetDiscoveredEndpoints());
        List<string> list = handleJSONStr(jsonString);
        return list;
    }
    /* Broadcasts a message to ALL connected devices as a JSON string. */
    public override void broadcast(string message) {
        modelBroadcastString(message);
    }

    /* Sends a message to a device by device ID. */
    public override void send(string message, string deviceID) {
        modelSendString(message, deviceID);
    }

    /* Sets own endpoint name to be the room code. */
    public override void setRoomCode(string roomCode) {
        modelSetEndpointName(roomCode);
    }
    
    /* Returns a JSON string of messages received so far. */
    private override string getMessagesReceived() {
        return "";
    }

    /* (For IOS use) Initialises P2P. */
    public override void initP2P() {
        modelInitialize();
    }

    private string handleStrPtr(IntPtr pointer) {
        string str = Marshal.PtrToStringAnsi(pointer);
        return str;
    }

    private List<string> handleJSONStr(string jsonString) {
        List<string> list = JsonConvert.DeserializeObject<List<string>>(jsonString);
        return list;
    }

}