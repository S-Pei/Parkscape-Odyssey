using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class RoomJoining : MonoBehaviour
{

    public TMP_Text roomCodeInput;

    public void Start() {
        //  Potentially need some initialisation here for P2P.
    }

    public void JoinRoom() {
        string roomCode = roomCodeInput.text;
        Debug.Log("Joining room: " + roomCode);

        if (foundRoom(roomCode)) {
            //  Join the room.
            SceneManager.LoadScene("Gameplay");
        } else {
            CreateRoom(roomCode);
        }
    }

    private void CreateRoom(string roomCode) {
        //  Create the room.
    }

    private bool foundRoom(string roomCode) {
        return roomCode == "123456";
    }
}
