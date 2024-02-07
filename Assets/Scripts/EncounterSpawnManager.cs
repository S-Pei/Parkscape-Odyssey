using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterSpawnManager : MonoBehaviour
{
    public List<Monster> monsters;

    public string encounterId;

    [SerializeField]
    private GameObject encounterController;

    public void CreateEncounterLobby() {
        // Create a new encounter lobby
        encounterController.GetComponent<EncounterController>()
            .CreateEncounterLobby(encounterId, monsters);
    }
}
