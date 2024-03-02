using UnityEngine;

public class ARObjectSpawner : MonoBehaviour {
    [SerializeField]
    private GameObject arObjectPrefab;

    [SerializeField]
    private Camera arCamera;

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
    }

    public GameObject SpawnARObject(float distance = 2.0f) {
        //spawn in front of at the camera
        // will fall to the ground
        var pos = arCamera.transform.position;
        var forw = arCamera.transform.forward;
        var obj = Instantiate(arObjectPrefab, pos + (forw * distance), Quaternion.identity);
        return obj;
    }
}
