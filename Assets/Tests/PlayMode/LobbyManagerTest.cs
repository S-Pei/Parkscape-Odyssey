using System.Collections;
using NUnit.Framework;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using TMPro;
using System;

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
        Assert.AreEqual("123", DebugNetwork.Instance._devicesToRoomCode[DebugNetwork.Instance._myID]);

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
        Assert.AreEqual(new Vector3(1, 1, 1), firstItem.transform.localScale);

        Assert.AreEqual("USER", firstItem.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text);

        GameObject startButton = GameObject.Find("Canvas/Lobby Pop Up/Start Button");
        Assert.IsNotNull(startButton);
        Assert.AreEqual(new Vector3(1, 1, 1), startButton.transform.localScale);
        
        yield return null;
    }

    [UnityTest]
    public IEnumerator SetupLobbyAsMember_FoundRoom() {
        // Get the lobby manager
        GameObject lobbyPopup = GameObject.Find("Canvas/Lobby Pop Up");
        LobbyManager lobbyManager = lobbyPopup.GetComponent<LobbyManager>();
        Assert.IsNotNull(lobbyManager);

        DebugNetwork.Instance.AddDevice("LEADER", "123");

        // Set up the lobby
        lobbyManager.SetUpLobby("123", false);

        Debug.Log("Successfully found room!");

        yield return null;
    }

    [UnityTest]
    public IEnumerator SetupLobbyAsMember_NotFoundRoomWillThrowException() {
        // Get the lobby manager
        GameObject lobbyPopup = GameObject.Find("Canvas/Lobby Pop Up");
        LobbyManager lobbyManager = lobbyPopup.GetComponent<LobbyManager>();
        Assert.IsNotNull(lobbyManager);

        // Joining the lobby fails and throws exception
        Assert.Throws<Exception>(() => lobbyManager.SetUpLobby("123", false));

        yield return null;
    }

    [UnityTest]
    public IEnumerator SetupLobbyAsMember_LoadLobbyUIInitialiseCorrectly() {
        // Get the lobby manager
        GameObject lobbyPopup = GameObject.Find("Canvas/Lobby Pop Up");
        LobbyManager lobbyManager = lobbyPopup.GetComponent<LobbyManager>();
        Assert.IsNotNull(lobbyManager);

        // Set own name as USER
        PlayerPrefs.SetString("name", "USER");


        DebugNetwork.Instance.AddDevice("LEADER", "123");

        // Joining the lobby succeeds
        lobbyManager.SetUpLobby("123", false);

        GameObject roomCodeText = GameObject.Find("Canvas/Lobby Pop Up/Lobby Code Text");
        Assert.IsNotNull(roomCodeText);
        Assert.AreEqual("123", roomCodeText.GetComponent<TMP_Text>().text);

        GameObject lobbyList = GameObject.Find("Canvas/Lobby Pop Up/Lobby List");
        Assert.IsNotNull(lobbyList);
        GameObject firstItem = lobbyList.transform.GetChild(0).gameObject;
        Assert.IsNotNull(firstItem);
        Assert.AreEqual(new Vector3(1, 1, 1), firstItem.transform.localScale);

        Assert.AreEqual("USER", firstItem.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text);

        GameObject startButton = GameObject.Find("Canvas/Lobby Pop Up/Start Button");
        Assert.IsNotNull(startButton);
        Assert.AreEqual(new Vector3(0, 0, 0), startButton.transform.localScale);

        yield return null;
    }


    [UnityTest]
    public IEnumerator MemberJoinedRoom_LeaderUpdatesLobbyUI() {
        string myName = "USER";
        string myID = SystemInfo.deviceUniqueIdentifier;
        string memberName = "MEMBER";

        // Get the lobby manager
        GameObject lobbyPopup = GameObject.Find("Canvas/Lobby Pop Up");
        LobbyManager lobbyManager = lobbyPopup.GetComponent<LobbyManager>();
        Assert.IsNotNull(lobbyManager);

        // Set own name as USER
        PlayerPrefs.SetString("name", myName);

        DebugNetwork.Instance.AddDevice("MEMBERID", "123");

        // Setup the lobby succeeds
        lobbyManager.SetUpLobby("123", true);

        LobbyMessage amIInMessage = new(LobbyMessageType.MEMBER_I_AM_IN, false, memberName, mediumEncounterLocations: null, sendTo : DebugNetwork.Instance._myID, sendFrom: "MEMBERID");
        Message msg = Message.createMessage(amIInMessage);
        DebugNetwork.Instance.BroadcastFromPlayer("MEMBERID", msg.toJson());

        yield return new WaitForSeconds(3);

        GameObject lobbyList = GameObject.Find("Canvas/Lobby Pop Up/Lobby List");
        Assert.IsNotNull(lobbyList);

        GameObject firstItem = lobbyList.transform.GetChild(0).gameObject;
        Assert.IsNotNull(firstItem);
        Assert.AreEqual(new Vector3(1, 1, 1), firstItem.transform.localScale);

        GameObject secondItem = lobbyList.transform.GetChild(1).gameObject;
        Assert.IsNotNull(secondItem);
        Assert.AreEqual(new Vector3(1, 1, 1), secondItem.transform.localScale);

        Assert.AreEqual(myName, firstItem.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text);
        Assert.AreEqual(memberName, secondItem.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text);

        yield return null;
    }
}
