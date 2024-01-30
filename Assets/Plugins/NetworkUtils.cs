using System.Collections.Generic;
using Newtonsoft.Json;

public abstract class NetworkUtils
{
    /* Cache of set of message IDs received so far. */
    HashSet<string> messageIDs;
    /* Discover other endpoints and connect with endpoints with same name (room code) */
    public abstract void discover();
    /* Advertise so that other endpoints can connect. */ 
    public abstract void advertise();   
    /* Returns list of connected devices by device ID. */
    public abstract List<string> getConnectedDevices();
    /* Returns a list of discovered devices by device ID. */
    public abstract List<string> getDiscoveredDevices();
    /* Broadcasts a message to ALL connected devices as a JSON string. */
    public abstract void broadcast(string message);
    /* Sends a message to a device by device ID. */
    public abstract void send(string message, string deviceID);
    /* Sets own endpoint name to be the room code. */
    public abstract void setRoomCode(string roomCode);
    /* Returns a JSON string of messages received so far. */
    public abstract string getMessagesReceived();
    /* (For IOS use) Initialises P2P. */
    public abstract void initP2P();

    /* Called in Update to process any incoming messages. */
    // public void processMessages() {
    //     string jsonString = this.getMessagesReceived();

    // }

    /* Compare list of received messages with cached set of message IDs. */
    /* Format of jsonString: Dict: <Message ID, Message JSON> */
    // private void checkMessages(string jsonString) {
    //     Dictionary<string, Message> msgs = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
    //     foreach (string id in msgs.Keys) {
    //         if (!messageIDs.Contains(id)) {

    //         }
    //     }
    // }

}