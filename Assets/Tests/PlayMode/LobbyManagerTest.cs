using System.Collections;
using NUnit.Framework;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using TMPro;

public class LobbyManagerTest
{

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    // [UnityTest]
    // public IEnumerator DummyTestWithEnumeratorPasses()
    // {
    //     // Use the Assert class to test conditions.
    //     // Use yield to skip a frame.
    //     Assert.IsTrue(true);
    //     return null;
    // }
    [OneTimeSetUp]
    public void InitScene() {
        SceneManager.LoadScene("Main Menu");
        
        DebugNetwork network = DebugNetwork.Instance;
        network.SetMyID("MYID");
        network.startAdvertising();
    }

    [UnityTest]
    public IEnumerator SetupLobbyAsLeader_SetsCorrectRoomCode() {
        // Get the lobby manager
        GameObject lobbyPopup = GameObject.Find("Canvas/Lobby Pop Up");
        LobbyManager lobbyManager = lobbyPopup.GetComponent<LobbyManager>();
        Assert.IsNotNull(lobbyManager);

        // Set up the lobby
        lobbyManager.SetUpLobby("123", true);
        Assert.AreEqual("123", DebugNetwork.Instance._devicesToRoomCode["MYID"]);

        yield return null;
    }


    [UnityTest]
    public IEnumerator SetupLobbyAsLeader_OnlyAdvertise() {
        // Get the lobby manager
        GameObject lobbyPopup = GameObject.Find("Canvas/Lobby Pop Up");
        LobbyManager lobbyManager = lobbyPopup.GetComponent<LobbyManager>();
        Assert.IsNotNull(lobbyManager);

        // Set up the lobby
        lobbyManager.SetUpLobby("123", true);
        Assert.IsTrue(DebugNetwork.Instance._isAdvertising);
        Assert.IsFalse(DebugNetwork.Instance._isDiscovering);

        yield return null;
    }

    [UnityTest]
    public IEnumerator SetupLobbyAsLeader_LoadLobbyUICorrectly() {
        // Get the lobby manager
        GameObject lobbyPopup = GameObject.Find("Canvas/Lobby Pop Up");
        LobbyManager lobbyManager = lobbyPopup.GetComponent<LobbyManager>();
        Assert.IsNotNull(lobbyManager);

        PlayerPrefs.SetString("name", "USER");

        // Set up the lobby
        lobbyManager.SetUpLobby("123", true);
        
        Assert.IsTrue(lobbyPopup.activeSelf);

        GameObject roomCodeText = GameObject.Find("Canvas/Lobby Pop Up/Lobby Code Text");
        Assert.IsNotNull(roomCodeText);
        Assert.AreEqual("123", roomCodeText.GetComponent<TMP_Text>().text);

        GameObject lobbyList = GameObject.Find("Canvas/Lobby Pop Up/Lobby List");
        Assert.IsNotNull(lobbyList);
        GameObject firstItem = lobbyList.transform.GetChild(0).gameObject;
        Assert.IsNotNull(firstItem);
        Assert.IsTrue(firstItem.activeSelf);

        Assert.AreEqual("USER", firstItem.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text);

        GameObject startButton = GameObject.Find("Canvas/Lobby Pop Up/Start Button");
        Assert.IsNotNull(startButton);
        Assert.IsTrue(startButton.activeSelf);
        

        yield return null;
    }

}
