using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterObjectManager : MonoBehaviour
{
    void Update() {
        if (Input.touches.Length > 0) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out hit)) {
                    if (hit.transform.gameObject.CompareTag("AREncounterSpawn")) {
                        GameManager.Instance.LogTxt("Encounter object touched");
                    }
                }
            }
        }
    }
}
