using System.Collections.Generic;
using UnityEngine;
using Microsoft.Geospatial;

public class QuestFactory : MonoBehaviour {

    // ----------------------------- BASIC QUESTS ------------------------------
    // public static BasicQuest CreateFindObjectQuest(string label, int target, Texture2D referenceImage, float[] featureVector) {
        // return new BasicQuest(QuestType.FIND, label, target, referenceImage, featureVector);
    // }


    // ----------------------------- GAMESTATE INIT ----------------------------
    public static List<LocationQuest> CreateInitialLocationQuests(List<Texture2D> referenceImages) {
        List<LocationQuest> locationQuests = new();
        // List of hardcoded location quests based on Hyde Park
        // TODO: FETCH FROM DATABASE
        locationQuests.Add(new LocationQuest(QuestType.FIND, "Peter Pan statue in Kensington Gardens", referenceImages[0], new LatLon(51.508621, -0.175916)));
        locationQuests.Add(new LocationQuest(QuestType.FIND, "Albert Memorial", referenceImages[1], new LatLon(51.502382, -0.177694)));
        locationQuests.Add(new LocationQuest(QuestType.FIND, "Speke's Monument", referenceImages[2], new LatLon(51.508995, -0.179137)));
        return locationQuests;
    } 

    public static List<BasicQuest> CreateInitialBasicQuests() {
        List<BasicQuest> basicQuests = new();
        Texture2D emptyTexture = new Texture2D(1, 1);
        emptyTexture.SetPixel(0, 0, Color.clear);
        emptyTexture.Apply();
        basicQuests.Add(new BasicQuest(QuestType.FIND, "flower", 1, emptyTexture));
        basicQuests.Add(new BasicQuest(QuestType.FIND, "bird", 1, emptyTexture));
        basicQuests.Add(new BasicQuest(QuestType.FIND, "duck", 1, emptyTexture));
        return basicQuests;
    }
}