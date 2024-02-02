using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [SerializeField]
    private List<MonsterName> monsterNames;

    [SerializeField]
    private List<Sprite> monsterImgs;

    private readonly Dictionary<MonsterName, Monster> allMonsters = new();


    void Awake() {
        if (monsterNames.Count != monsterImgs.Count) {
            Debug.LogWarning("MonsterController: Monster Names and Monster Images provided do not have the same amount. Please check these fields.");
            return;
        }

        // Initialise data for all monsters
        for (int i = 0; i < monsterNames.Count; i++) {
            Monster monsterData = MonsterFactory.CreateMonster(monsterNames[i], monsterImgs[i]);
            if (monsterData != null) {
                allMonsters.Add(monsterNames[i], monsterData);
                Debug.Log($"MonsterController: Successfully initialised monster - {monsterNames[i]}");
            } else {
                Debug.LogWarning($"MonsterController: Error initializing montser - {monsterNames[i]}");
            }
        }
    }
}
