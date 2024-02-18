using UnityEngine;
using UnityEngine.Events;

public class SpriteButton : MonoBehaviour {
    [SerializeField]
    private UnityEvent onClick = new();

    [SerializeField]
    private bool Disabled;

    void Start() {
        if (onClick == null) {
            throw new System.Exception("SpriteButton requires an onClick event to be set.");
        }

        // Check if gameobject has box collider
        if (GetComponent<BoxCollider>() == null) {
            throw new System.Exception("SpriteButton requires a BoxCollider component on the object.");
        }
    }

    // Update is called once per frame
    void Update() {
        if (Disabled) return;
        if (Input.GetMouseButtonUp(0)) {
            GameObject target = GetClickedObject(out RaycastHit hit);
            if (target == gameObject) {
                onClick.Invoke();
            }
        }
    }

    private GameObject GetClickedObject(out RaycastHit hit) {
        GameObject target = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction * 10, out hit)) {
            if (hit.collider != null)
                target = hit.collider.gameObject;
        }
        Debug.Log(target);
        return target;
    }
}
