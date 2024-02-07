using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterSpawnManager : MonoBehaviour
{
    private List<Monster> monsters;

    private string encounterId;

    private GameObject encounterManager;

    public void EncounterSpawnInit(string encounterId, List<Monster> monsters) {
        this.encounterId = encounterId;
        this.monsters = monsters;
        encounterManager = GameObject.FindGameObjectWithTag("EncounterManager");
    }

    public void CreateEncounterLobby() {
        // Create a new encounter lobby
        encounterManager.GetComponent<EncounterController>()
            .CreateEncounterLobby(encounterId, monsters);
    }
}
