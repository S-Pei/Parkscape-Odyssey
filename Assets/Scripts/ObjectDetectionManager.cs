using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Niantic.Lightship.AR.ObjectDetection;
using Niantic.Lightship.AR.Subsystems.ObjectDetection;
using Niantic.Lightship.AR.XRSubsystems;
using TMPro;
using UnityEngine;

public class ObjectDetectionManager : MonoBehaviour
{
    [SerializeField]
    private ARObjectDetectionManager _objectDetectionManager;

    [SerializeField]
    private GameObject gameManagerObj;
    private GameManager gameManager;

    private Dictionary<string, int> objectDetectionTimes = new();
    private const int detectionTTL = 60;

    private void Start()
    {
        _objectDetectionManager.enabled = true;
        _objectDetectionManager.MetadataInitialized += OnMetadataInitialized;

        gameManager = gameManagerObj.GetComponent<GameManager>();
    }

    private void OnMetadataInitialized(ARObjectDetectionModelEventArgs args)
    {
        _objectDetectionManager.ObjectDetectionsUpdated += ObjectDetectionsUpdated;
    }

    private void ObjectDetectionsUpdated(ARObjectDetectionsUpdatedEventArgs args)
    {
        //Initialize our output string
        string resultString = "";
        var result = args.Results;

        if (result == null)
        {
            Debug.Log("No results found.");
            return;
        }

        //Reset our results string
        resultString = "";

        //Iterate through our results
        for (int i = 0; i < result.Count; i++)
        {
            var detection = result[i];
            var categorizations = detection.GetConfidentCategorizations();
            if (categorizations.Count <= 0)
            {
                break;
            }

            //Sort our categorizations by highest confidence
            categorizations.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));

            //Iterate through found categoires and form our string to output
            for (int j = 0; j < categorizations.Count; j++)
            {
                var categoryToDisplay = categorizations[j];

                resultString += "Detected " + $"{categoryToDisplay.CategoryName}: " + "with " + $"{categoryToDisplay.Confidence} Confidence \n";
                // Add object to category.
                AddToDetectionTimes(categoryToDisplay.CategoryName);
            }
        }

        //Iterate through our detection times and increment them.
        foreach (var pair in objectDetectionTimes) {
            objectDetectionTimes[pair.Key]++;
        }

        //Output our string
        gameManager.RelogTxt(resultString);
    }
    private void OnDestroy()
    {
        _objectDetectionManager.MetadataInitialized -= OnMetadataInitialized;
        _objectDetectionManager.ObjectDetectionsUpdated -= ObjectDetectionsUpdated;
    }

    public List<string> GetLabels()
    {
        List<string> labels = new();
        foreach (var pair in objectDetectionTimes)
        {
            if (pair.Value <= detectionTTL)
            {
                labels.Add(pair.Key);
            }
        }
        return labels;
    }

    private void AddToDetectionTimes(string label)
    {
        if (objectDetectionTimes.ContainsKey(label))
        {
            objectDetectionTimes[label] = 0;
        }
        else
        {
            objectDetectionTimes.Add(label, 0);
        }
    }
}

