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
    private TextMeshProUGUI endpointListStr;

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
    private static extern IntPtr modelGetDiscoveredEndpoints();

    [DllImport("__Internal")]
    private static extern IntPtr modelGetConnectedEndpoints();

    [DllImport("__Internal")]
    private static extern IntPtr modelGetConnectionRequests();

    [DllImport("__Internal")]
    private static extern void modelSendHelloWorld();

    private DiscoveredEndpoint[] discoveredEndpoints;
    private ConnectedEndpoint[] connectedEndpoints;
    private ConnectionRequest[] connectionRequests;

    void Start() {
        Debug.Log("runs here");
        modelInitialize();
        name.text = Marshal.PtrToStringAnsi(modelGetEndpointName());
        // startAdvertising();
        // startDiscovering();
    }

    void Update() {
        // discoveredEndpoints = deserializeJsonToDiscoveredEndpoints(handleStringPointer(modelGetDiscoveredEndpoints()));
        // connectedEndpoints = deserializeJsonToConnectedEndpoints(handleStringPointer(modelGetConnectedEndpoints()));
        // connectionRequests = deserializeJsonToConnectionRequests(handleStringPointer(modelGetConnectionRequests()));
        dataStr.text = getData();
    }

    // to be called from UI
    public void startDiscovering() {
        modelStartDiscovery();
    }

    // to be called from UI
    public void startAdvertising() {
        modelStartAdvertising();
    }

    // to be called from UI
    public void getDiscoveredEndpointList() {
        discoveredEndpoints = deserializeJsonToDiscoveredEndpoints(handleStringPointer(modelGetDiscoveredEndpoints()));
        connectedEndpoints = deserializeJsonToConnectedEndpoints(handleStringPointer(modelGetConnectedEndpoints()));
        IEnumerable<string> endpointNames = discoveredEndpoints.Select(endpoint => endpoint.EndpointName);
        endpointListStr.text = string.Join(", ", endpointNames);
        // Debug.Log("Discovered endpoints: " + string.Join(", ", endpointNames));
        // Debug.Log("Connected endpoints: " + string.Join(", ", connectedEndpoints.Select(endpoint => endpoint.EndpointName)));
    }

    // sends "hello world" hardcoded string (called from UI)
    public void sendString() {
        // connectedEndpoints = deserializeJsonToConnectedEndpoints(handleStringPointer(modelGetConnectedEndpoints()));
        // endpointIDs = connectedEndpoints.Select(endpoint => endpoint.EndpointID);
        modelSendHelloWorld();
    }

    // gets data sent from other devices
    public string getData() {
        var jsonStr = handleStringPointer(modelGetConnectedEndpoints());
        connectedEndpoints = deserializeJsonToConnectedEndpoints(jsonStr);
        IEnumerable<string> data = connectedEndpoints.SelectMany(endpoint => endpoint.Payloads.Select(payload => payload.Data));
        return string.Join(", ", data);
    }

    private string handleStringPointer(IntPtr pointer) {
        string str = Marshal.PtrToStringAnsi(pointer);
        return str;
    }

    private ConnectedEndpoint[] deserializeJsonToConnectedEndpoints(string jsonStr) {
        ConnectedEndpoint[] connectedEndpoints = JsonConvert.DeserializeObject<ConnectedEndpoint[]>(jsonStr);
        return connectedEndpoints;
    }

    private DiscoveredEndpoint[] deserializeJsonToDiscoveredEndpoints(string jsonStr) {
        DiscoveredEndpoint[] discoveredEndpoints = JsonConvert.DeserializeObject<DiscoveredEndpoint[]>(jsonStr);
        return discoveredEndpoints;
    }

    private ConnectionRequest[] deserializeJsonToConnectionRequests(string jsonStr) {
        ConnectionRequest[] connectionRequests = JsonConvert.DeserializeObject<ConnectionRequest[]>(jsonStr);
        return connectionRequests;
    }
} 