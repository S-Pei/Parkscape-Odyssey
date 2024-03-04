using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterObjectManager : MonoBehaviour
{
    private int COL_DETECT_POINTS_NUM = 4;
    private int ROW_DETECT_POINTS_NUM = 3;


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

    

    // Get all the points in the grid to detect the ground
    private List<(int, int)> GetDetectGroundPoints() {
        List<(int, int)> detectPoints = new List<(int, int)>();

        for (int i = 1; i <= ROW_DETECT_POINTS_NUM; i++) {
            for (int j = 1; j <= COL_DETECT_POINTS_NUM; j++) {
                detectPoints.Add((Screen.width / (ROW_DETECT_POINTS_NUM + 1) * i, Screen.height / (COL_DETECT_POINTS_NUM + 1) * j));
            }
        }
        return detectPoints;
    }
}
