using System.Collections.Generic;
using UnityEngine;
public class QuestFactory {

    // ----------------------------- BASIC QUESTS ------------------------------
    // public static BasicQuest CreateFindObjectQuest(string label, int target, Texture2D referenceImage, float[] featureVector) {
        // return new BasicQuest(QuestType.FIND, label, target, referenceImage, featureVector);
    // }


    // ----------------------------- GAMESTATE INIT ----------------------------
    public static List<LocationQuest> CreateInitialLocationQuests() {
        List<LocationQuest> locationQuests = new();
        return locationQuests;
    } 

    public static List<BasicQuest> CreateInitialBasicQuests() {
        List<BasicQuest> basicQuests = new();
        return basicQuests;
    }
}