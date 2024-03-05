using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Geospatial;
using Niantic.Lightship.AR.Semantics;
using UnityEngine;

public class EncounterObjectManager : MonoBehaviour
{
    private int COL_DETECT_POINTS_NUM = 4;
    private float MAX_HEIGHT_FOR_COL = 1/2;
    private int ROW_DETECT_POINTS_NUM = 3;

    [SerializeField] private GameObject _xrOrigin;
    private Depth_ScreenToWorldPosition _depth_ScreenToWorldPosition;

    // Encounter spawning
    [SerializeField] private GameObject _randomEncounterPrefab;
    [SerializeField] private GameObject _bossEncounterPrefab;
    private Queue<(string, EncounterType)> _encountersToSpawn = new();

    private bool _inARMode = false;

    // Semantics check for ground
    [SerializeField] private GameObject _segmentationManager;
    private SemanticQuerying _semanticQuerying;
    private List<(int, int)> _detectGroundPoints;
    private float MIN_DEPTH_FOR_SPAWN = 2.5f;
    public float SPAWN_Y_OFFSET = 3f;
    private int CHECK_GROUND_INTERVAL = 100;
    private int _checkGroundCounter = 0;

    void Start() {
        _semanticQuerying = _segmentationManager.GetComponent<SemanticQuerying>();
        _depth_ScreenToWorldPosition = _xrOrigin.GetComponent<Depth_ScreenToWorldPosition>();

        // Preload the points in the grid to detect for ground
        LoadDetectGroundPoints();
    }

    private bool _readyToSpawnEncounter = false;
    private EncounterType _toSpawnEncounterType;

    void Update() {
        if (!_inARMode) {
            return;
        }

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
        
        if (_checkGroundCounter >= CHECK_GROUND_INTERVAL) {
            if (_readyToSpawnEncounter) {
                if (TrySpawnEncounter()) {
                    // Successfully spawned an encounter on ground
                    _readyToSpawnEncounter = false;
                }
            } else {
                // Check if there is an encounter to spawn
                if (TryGetEncounterToSpawn() is EncounterType encounterType) {
                    _toSpawnEncounterType = encounterType;
                    _readyToSpawnEncounter = true;
                    GameManager.Instance.LogTxt("Checking for ground...");
                }
            }
            // Reset counter
            _checkGroundCounter = 0;
        } else {
            _checkGroundCounter += 1;
        }
    }

    private EncounterType? TryGetEncounterToSpawn() {
        if (_encountersToSpawn.TryPeek(out (string, EncounterType) encounter)) {
            (string _, EncounterType encounterType) = encounter;
            return encounterType;
        }
        return null;
    }
    private bool TrySpawnEncounter() {
        List<(int, int)> detectedGroundPoints = new();
        foreach ((int x, int y) point in _detectGroundPoints) {
            if (_semanticQuerying.GetPositionChannel(point.x, point.y) == "ground") {
                if (_depth_ScreenToWorldPosition.GetDepthOfPoint(point.x, point.y) >= MIN_DEPTH_FOR_SPAWN) {
                    detectedGroundPoints.Add(point);
                }
            }
        }

        if (detectedGroundPoints.Count > 0) {
            GameManager.Instance.LogTxt("Ground detected");

            (int x, int y) = SelectRandomPoint(detectedGroundPoints);
            Vector3 worldPosition = _depth_ScreenToWorldPosition.TranslateScreenToWorldPoint(x, y);
            worldPosition.y += SPAWN_Y_OFFSET;
            Instantiate(_toSpawnEncounterType == EncounterType.RANDOM_ENCOUNTER ? _randomEncounterPrefab : _bossEncounterPrefab, worldPosition, Quaternion.identity);
            _encountersToSpawn.Dequeue();
            return true;
        }

        return false;
    }


    // Select a random point from a list of points
    private (int, int) SelectRandomPoint(List<(int, int)> points) {
        int index = UnityEngine.Random.Range(0, points.Count);
        return points[index];
    }

    // Get all the points in the grid to detect for ground
    private void LoadDetectGroundPoints() {
        List<(int, int)> detectPoints = new List<(int, int)>();

        for (int i = 1; i <= ROW_DETECT_POINTS_NUM; i++) {
            for (int j = 1; j <= COL_DETECT_POINTS_NUM; j++) {
                detectPoints.Add((Screen.width / (ROW_DETECT_POINTS_NUM + 1) * i, (int) (Screen.height * MAX_HEIGHT_FOR_COL) / (COL_DETECT_POINTS_NUM + 1) * j + (int) (Screen.height * MAX_HEIGHT_FOR_COL)));
                // GameManager.Instance.LogTxt($"Detect point added: {Screen.width / (ROW_DETECT_POINTS_NUM + 1) * i}, {Screen.height / (COL_DETECT_POINTS_NUM + 1) * j}");
            }
        }
        _detectGroundPoints = detectPoints;
    }

    // Add an encounter to the list of encounters to spawn
    public void AddEncounterToSpawn(string encounterId, EncounterType encounterType) {
        _encountersToSpawn.Enqueue((encounterId, encounterType));
        GameManager.Instance.LogTxt($"Encounter added to spawn queue: {encounterId}");
    }

    // Set AR mode
    public void SetARMode(bool inARMode) {
        _inARMode = inARMode;
    }
}
