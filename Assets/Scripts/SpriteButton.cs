using UnityEngine;
using UnityEngine.Events;

public class SpriteButton : MonoBehaviour {
    public UnityEvent onClick = new();
    public bool Disabled;

    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;

    protected void Start() {
        if (onClick == null) {
            throw new System.Exception("SpriteButton requires an onClick event to be set.");
        }

        // Check if gameobject has box collider
        if (GetComponent<BoxCollider>() == null || GetComponent<BoxCollider2D>() == null 
            || GetComponent<SphereCollider>() == null || GetComponent<MeshCollider>() == null) {
            throw new System.Exception("SpriteButton requires a Collider component on the object.");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    // Update is called once per frame
    protected void Update() {
        if (Disabled) return;
        if (Input.GetMouseButtonUp(0)) {
            GameObject target = GetClickedObject(out RaycastHit hit);
            if (target == gameObject) {
                onClick.Invoke();
            }
        }
    }

    protected void SetDisabled(bool disabled) {
        Disabled = disabled;
        if (disabled) {
            spriteRenderer.color = Color.gray;
        } else {
            spriteRenderer.color = originalColor;
        }
    }

    private GameObject GetClickedObject(out RaycastHit hit) {
        GameObject target = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction * 10, out hit)) {
            if (hit.collider != null)
                target = hit.collider.gameObject;
        }
        return target;
    }
}
