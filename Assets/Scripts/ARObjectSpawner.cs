using System;
using System.Collections.Generic;
using UnityEngine;

public class ARObjectSpawner : MonoBehaviour {
    [SerializeField]
    private GameObject arObjectPrefab;

    [SerializeField]
    private Camera arCamera;

    [SerializeField]
    private bool gravity = true;

    private Dictionary<GameObject, Action> trackObjects = new();

    private const float threshold = -500;

    void Start() {
        if (arObjectPrefab == null) {
            Debug.LogError("ARObjectSpawner: arObjectPrefab is not set.");
        }
        if (arCamera == null) {
            Debug.LogError("ARObjectSpawner: arCamera is not set.");
        }
    }

    void Update() {
        // #if UNITY_EDITOR
        // if(Input.GetMouseButtonDown(0))
        // #else
        // if (Input.touchCount > 0)
        // #endif
        // {
        //     var obj = SpawnARObject();
        //     Debug.Log("Spawned AR object: " + obj);
        // }

        // Track if the object has settled on the ground
        if (!gravity)
            return;

        foreach (var obj in trackObjects) {
            if (obj.Key.transform.position.y < threshold) {
                trackObjects.Remove(obj.Key);
                Destroy(obj.Key);
                // Spawn new object
                obj.Value();
            }
        }   
    }

    public GameObject SpawnARObject(float distance = 2.0f) {
        //spawn in front of at the camera with some random degree deviation.
        var pos = arCamera.transform.position;
        var forw = arCamera.transform.forward.normalized;
        var obj = Instantiate(arObjectPrefab, pos + (forw * distance), Quaternion.identity);

        // Track if the object has settled on the ground
        if (!gravity)
            return obj;
        
        trackObjects.Add(obj, () => SpawnARObject(distance));
        return obj;
    }
}
