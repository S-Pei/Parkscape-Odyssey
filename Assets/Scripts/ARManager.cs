using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARManager : MonoBehaviour
{
    public static ARManager selfReference;
    
    [SerializeField]
    private GameObject xrInteractionManager;
    
    [SerializeField]
    private GameObject xrOrigin;

    [SerializeField]
    private List<GameObject> arSpawnLocations;

    public static ARManager Instance {
        get {
            if (selfReference == null) {
                selfReference = new();
            }
            return selfReference;
        }
    }

    public void Awake() {
        selfReference = this;
    }

    public void StartAR() {
        Debug.Log("Starting AR session.");
        xrOrigin.SetActive(true);
        SpawnAllLocations();
    }

    public void StopAR() {
        Debug.Log("Stopping AR session.");
        DeSpawnAllLocations();
        xrOrigin.SetActive(false);
    }

    public Texture2D TakeScreenCapture() {
        Debug.Log("Taking a screen capture.");
        return ScreenCapture.CaptureScreenshotAsTexture();
    }

    public void SpawnAllLocations() {
        foreach (GameObject arLocation in arSpawnLocations) {
            GameObject spawnedARLocation = Instantiate(arLocation, xrOrigin.transform);
            spawnedARLocation.transform.SetParent(xrOrigin.transform);
        }
    }

    public void DeSpawnAllLocations() {
        var i = 0;
        foreach (Transform child in xrOrigin.transform) {
            if (i != 0) {
                Destroy(child.gameObject);
            }
            i++;
        }
    }
}
