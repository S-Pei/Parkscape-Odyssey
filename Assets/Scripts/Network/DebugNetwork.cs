using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

public class DebugNetwork : NetworkUtils
{
    private static DebugNetwork instance;

    public static DebugNetwork Instance {
        get {
            if (instance == null) {
                instance = new DebugNetwork();
            }
            return instance;
        }
    }
    public Dictionary<string, Queue<string>> _messageQueues = new();

    public string _myID = "MYID";

    public List<string> _connectedDevices = new();

    public Dictionary<string, string> _devicesToRoomCode = new();

    public bool _isAdvertising = false;
    public bool _isDiscovering = false;

    private readonly bool DEBUGLOG = true;

    public override void broadcast(string message)
    {   
        if (!DEBUGLOG)
            return;
        foreach (var device in _messageQueues.Keys) {
            if (_devicesToRoomCode[device] == _devicesToRoomCode[_myID])
                _messageQueues[device].Enqueue(message);
        }
    }

    public override List<string> getConnectedDevices()
    {
        return _connectedDevices;
    }

    public override List<string> getDiscoveredDevices()
    {
        return new();
    }

    public override string getMessageReceived()
    {
        return "";
    }

    public override string getName()
    {
        return "";
    }

    public override void initP2P()
    {
        return;
    }

    public override void onReceive(Func<Message, CallbackStatus> callback)
    {
        if (!DEBUGLOG)
            return;
        if (!_messageQueues.ContainsKey(_myID) || _messageQueues[_myID].Count == 0) {
            return;
        }
        Message message = JsonConvert.DeserializeObject<Message>(_messageQueues[_myID].Peek(), new NetworkJsonConverter());
        CallbackStatus status = callback(message);
        if (status == CallbackStatus.PROCESSED || status == CallbackStatus.DORMANT) {
            _messageQueues[_myID].Dequeue();
        }
    }

    public override void send(string message, string deviceID)
    {
        if (!DEBUGLOG)
            return;
        // Debug.Log("Send message: " + message);
    }

    public override void setRoomCode(string roomCode)
    {
        if (!DEBUGLOG)
            return;
        _devicesToRoomCode[_myID] = roomCode;
    }

    public override void startAdvertising()
    {
        if (!DEBUGLOG)
            return;
        // Debug.Log("Start advertising.");
        _messageQueues[_myID] = new();
        _isAdvertising = true;
    }

    public override void startDiscovering()
    {
        if (!DEBUGLOG)
            return;
        // Debug.Log("Start discovering.");
        _isDiscovering = true;
    }

    public override void stopAdvertising()
    {
        if (!DEBUGLOG)
            return;
        // Debug.Log("Stop advertising.");
        _isAdvertising = false;
    }

    public override void stopDiscovering()
    {
        if (!DEBUGLOG)
            return;
        // Debug.Log("Stop discovering.");
        _isDiscovering = false;
    }

    public void InitiateConnectedDevices(Dictionary<string, string> devicesToRoomCode) {
        _devicesToRoomCode = devicesToRoomCode;
        _connectedDevices = new(devicesToRoomCode.Keys);
    }

    public void DisconnectDevice(string id) {
        _connectedDevices.Remove(id);
    }

    public void SetMyID(string id) {
        _myID = id;
    }
}