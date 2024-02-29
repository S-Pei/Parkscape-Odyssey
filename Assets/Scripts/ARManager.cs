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
        xrInteractionManager.SetActive(true);
        xrOrigin.SetActive(true);
    }

    public void StopAR() {
        Debug.Log("Stopping AR session.");
        xrInteractionManager.SetActive(false);
        xrOrigin.SetActive(false);
    }

    public Texture2D TakeScreenCapture() {
        Debug.Log("Taking a screen capture.");
        return ScreenCapture.CaptureScreenshotAsTexture();
    }
}
