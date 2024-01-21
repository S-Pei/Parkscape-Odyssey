using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WifiDirect : WifiDirectBase
{
    public GameObject addrButton;
    public GameObject buttonList;
    // Start is called before the first frame update
    void Start()
    {
        base.initialize(this.gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //when the WifiDirect services is connected to the phone, begin broadcasting and discovering services
    public override void onServiceConnected() {
        Dictionary<string, string> record = new Dictionary<string, string>();
        record.Add("demo", "unity");
        base.broadcastService("hello", record);
        base.discoverServices();
    }

    //On finding a service, create a button with that service's address
    public override void onServiceFound(string addr) {
        GameObject newButton = Instantiate(addrButton);
        newButton.GetComponentInChildren<Text>().text = addr;
        newButton.transform.SetParent(buttonList.transform, false);
        newButton.GetComponent<Button>().onClick.AddListener(() => {
            this.makeConnection(addr);
        });
    }

    //When the button is clicked, connect to the service at its address
    private void makeConnection(string addr) {
        base.connectToService(addr);
    }

    //Kill Switch
    public override void onServiceDisconnected() {
        base.terminate();
        Application.Quit();
    }
}
