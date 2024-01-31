using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;

public class TestScriptIOS : MonoBehaviour 
{

    // [SerializeField]
    // private TextMeshProUGUI testInt;

    // [SerializeField]
    // private TextMeshProUGUI testString;

    [SerializeField]
    private TextMeshProUGUI name;

    [SerializeField]
    private TextMeshProUGUI discoveredEndpointListStr;

    [SerializeField]
    private TextMeshProUGUI connectedEndpointListStr;

    [SerializeField]
    private TextMeshProUGUI discoveredJSONStr;

    [SerializeField]
    private TextMeshProUGUI connectedJSONStr;


    [SerializeField]
    private TextMeshProUGUI dataStr;

    [DllImport("__Internal")]
    private static extern void modelInitialize();

    [DllImport("__Internal")]
    private static extern void modelStartDiscovery();

    [DllImport("__Internal")]
    private static extern void modelStartAdvertising();

    [DllImport("__Internal")]
    private static extern IntPtr modelGetEndpointName();

    [DllImport("__Internal")]
    private static extern void modelSetEndpointName(string endpointName);

    [DllImport("__Internal")]
    private static extern IntPtr modelGetDiscoveredEndpoints();

    [DllImport("__Internal")]
    private static extern IntPtr modelGetConnectedEndpoints();

    [DllImport("__Internal")]
    private static extern void modelBroadcastString(string message);

    [DllImport("__Internal")]
    private static extern IntPtr modelGetMessages();

    [DllImport("__Internal")]
    private static extern IntPtr modelGetNewMessages();

    private string[] discoveredEndpoints;
    private string[] connectedEndpoints;

    private float timer = 0.0f;
    private float delay = 4.0f;

    void Start() {
        modelInitialize();
        name.text = handleStringPointer(modelGetEndpointName());
        startAdvertising();
        startDiscovering();
    }

    void Update() {
        timer += Time.deltaTime;
        discoveredEndpoints = deserializeJsonToStringArray(handleStringPointer(modelGetDiscoveredEndpoints()));
        connectedEndpoints = deserializeJsonToStringArray(handleStringPointer(modelGetConnectedEndpoints()));
        // connectionRequests = deserializeJsonToConnectionRequests(handleStringPointer(modelGetConnectionRequests()));
        name.text = handleStringPointer(modelGetEndpointName());
        if (timer > delay) {
            dataStr.text = "data\n" + handleStringPointer(modelGetNewMessages());
        }
        discoveredEndpointListStr.text = "Discovered Devices\n" + string.Join("\n ", discoveredEndpoints);
        connectedEndpointListStr.text = "Connected Devices\n" + string.Join(", ", connectedEndpoints);
        if (timer > delay) {
            timer = 0.0f;
        }
    }

    public void setRoomName() {
        string roomName = "room1";
        modelSetEndpointName(roomName);
    }

    // to be called from UI
    public void startDiscovering() {
        modelStartDiscovery();
    }

    public void startAdvertising() {
        modelStartAdvertising();
    }

    // // to be called from UI
    // public void getDiscoveredEndpointList() {
    //     discoveredEndpoints = deserializeJsonToDiscoveredEndpoints(handleStringPointer(modelGetDiscoveredEndpoints()));
    //     IEnumerable<string> endpointNames = discoveredEndpoints.Select(endpoint => endpoint.EndpointName);
    //     discoveredEndpointListStr.text = string.Join(", ", endpointNames);
    // }

    // sends "hello world" hardcoded string (called from UI)
    public void sendString() {
        string message = "hell world";
        // connectedEndpoints = deserializeJsonToConnectedEndpoints(handleStringPointer(modelGetConnectedEndpoints()));
        // endpointIDs = connectedEndpoints.Select(endpoint => endpoint.EndpointID);
        modelBroadcastString(message);
    }

    // gets data sent from other devices
    // public string getData() {
    //     var jsonStr = handleStringPointer(modelGetConnectedEndpoints());
    //     connectedEndpoints = deserializeJsonToConnectedEndpoints(jsonStr);
    //     IEnumerable<string> data = connectedEndpoints.SelectMany(endpoint => endpoint.Payloads
    //                                                     .Where(payload => payload.IsIncoming == true)
    //                                                     .Select(payload => payload.Data));
    //     return string.Join(", ", data);
    // }

    private string handleStringPointer(IntPtr pointer) {
        string str = Marshal.PtrToStringAnsi(pointer);
        return str;
    }

    private ConnectedEndpoint[] deserializeJsonToConnectedEndpoints(string jsonStr) {
        if (timer > delay) {
            connectedJSONStr.text = jsonStr;
        }
        ConnectedEndpoint[] connectedEndpoints = JsonConvert.DeserializeObject<ConnectedEndpoint[]>(jsonStr);
        return connectedEndpoints;
    }

    private DiscoveredEndpoint[] deserializeJsonToDiscoveredEndpoints(string jsonStr) {
        if (timer > delay) {
            discoveredJSONStr.text = jsonStr;
        }
        DiscoveredEndpoint[] discoveredEndpoints = JsonConvert.DeserializeObject<DiscoveredEndpoint[]>(jsonStr);
        return discoveredEndpoints;
    }

    private ConnectionRequest[] deserializeJsonToConnectionRequests(string jsonStr) {
        ConnectionRequest[] connectionRequests = JsonConvert.DeserializeObject<ConnectionRequest[]>(jsonStr);
        return connectionRequests;
    }

    private string[] deserializeJsonToStringArray(string jsonStr) {
        string[] strArray = JsonConvert.DeserializeObject<string[]>(jsonStr);
        return strArray;
    }
} 