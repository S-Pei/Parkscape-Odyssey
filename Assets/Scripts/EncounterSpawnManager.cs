using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterSpawnManager : MonoBehaviour
{
    private List<Monster> monsters;
    private List<List<SkillName>> skillSequences;

    private string encounterId;

    private GameObject encounterManager;

    public void EncounterSpawnInit(string encounterId, List<Monster> monsters, List<List<SkillName>> skillSequences) {
        this.encounterId = encounterId;
        this.monsters = monsters;
        this.skillSequences = skillSequences;
        encounterManager = GameObject.FindGameObjectWithTag("EncounterManager");
    }

    public void CreateEncounterLobby() {
        // Create a new encounter lobby
        encounterManager.GetComponent<EncounterController>()
            .CreateEncounterLobby(encounterId, monsters, skillSequences);
    }

    public string GetEncounterId() {
        return encounterId;
    }
}
