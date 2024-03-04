using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using HNSW.Net;

public class ApproxNN : MonoBehaviour
{
    private const string FilePrefix = "quests/";
    private const string VectorsPathSuffix = "locationQuestVectors.bytes";
    private const string LabelsPathSuffix = "locationQuestLabels.bytes";
    private const string GraphpathSuffix = "locationQuestGraph.bytes";
    private const float KNNThreshold = 0.5f;

    private SmallWorld<float[], float> world;
    private string[] labels;
    private float[][] vectors;
    private static ApproxNN instance;

    public static ApproxNN Instance { 
        get {
            if (instance == null) {
                // To make sure that script is persistent across scenes
                GameObject go = new GameObject("ApproxNN");
                instance = go.AddComponent<ApproxNN>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    
    }

    public void Initialize(float[][] input, string[] labels) {
        if (input.Length != labels.Length) {
            throw new ArgumentException("Number of training data points must match number of labels.");
        }

        var normalizedInputs = input.Select(NormalizeVector).ToArray();

        this.vectors = normalizedInputs;
        this.labels = labels;

        // Construct initial world
        var parameters = new SmallWorld<float[], float>.Parameters()
            {
                M = 15,
                LevelLambda = 1 / Math.Log(15),
            };
        this.world = new SmallWorld<float[], float>(CosineDistance.ForUnits, DefaultRandomGenerator.Instance, parameters);
        this.world.AddItems(normalizedInputs);
        Debug.Log("World constructed");
    }

    /*** Performs KNN search and outputs label ***/
    public string[] Search(float[] query, int k = 3)
    {
        float[] normalizedQuery = NormalizeVector(query);
        Debug.Log("Normalized query");
        var results = this.world.KNNSearch(normalizedQuery, k);
        Debug.Log("Labels " + string.Join(", ", results.Select(r => this.labels[r.Id])));
        Debug.Log("Distances " + string.Join(", ", results.Select(r => r.Distance)));

        var filteredResults = results.Where(r => r.Distance < KNNThreshold).ToArray();
        if (filteredResults.Length == 0) {
            return new string[0];
        }
        return new string[] { WeightedNN(filteredResults) };
    }
    
    /*** weighted nearest neighbour ***/
    private string WeightedNN(SmallWorld<float[], float>.KNNSearchResult[] results) {
        var distances = results.Select(r => r.Distance).ToArray();
        var weights = distances.Select(d => 1 / (d + float.Epsilon)).ToArray();
        var labelWeights = new Dictionary<string, float>();
        for (int i = 0; i < results.Length; i++) {
            var label = this.labels[results[i].Id];
            if (labelWeights.ContainsKey(label)) {
                labelWeights[label] += weights[i];
            } else {
                labelWeights[label] = weights[i];
            }
        }
        Debug.Log("Label weights " + string.Join(", ", labelWeights.Select(kv => $"{kv.Key}: {kv.Value}")));
        return labelWeights.OrderByDescending(kv => kv.Value).First().Key;
    }

    public void Save(string path) {
        BinaryFormatter formatter = new BinaryFormatter();

        MemoryStream sampleVectorsStream = new MemoryStream();
        formatter.Serialize(sampleVectorsStream, vectors);
        File.WriteAllBytes($"{path}/{VectorsPathSuffix}", sampleVectorsStream.ToArray());

        MemoryStream labelsStream = new MemoryStream();
        formatter.Serialize(labelsStream, labels);
        File.WriteAllBytes($"{path}/{LabelsPathSuffix}", labelsStream.ToArray());

        using (var f = File.Open($"{path}/{GraphpathSuffix}", FileMode.Create))
        {
            world.SerializeGraph(f);
        }
    }

    public void Load() {
        string path = DatabaseManager.Instance.dataPath + FilePrefix;
        Debug.Log("Loading from " + path);
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream sampleVectorsStream = new MemoryStream(File.ReadAllBytes($"{path}/{VectorsPathSuffix}"));
        Debug.Log("Sample vectors stream " + sampleVectorsStream.Length);
        this.vectors = (float[][])formatter.Deserialize(sampleVectorsStream);
        Debug.Log("Vectors loaded");

        MemoryStream labelsStream = new MemoryStream(File.ReadAllBytes($"{path}/{LabelsPathSuffix}"));
        this.labels = (string[])formatter.Deserialize(labelsStream);
        Debug.Log("Labels loaded");

        using (var f = File.OpenRead($"{path}/{GraphpathSuffix}"))
        {
            Debug.Log("Deserializing graph");
            this.world = SmallWorld<float[], float>.DeserializeGraph(vectors, CosineDistance.ForUnits, DefaultRandomGenerator.Instance, f);
            Debug.Log("Graph deserialized");
        }

        Debug.Log("World loaded");
    }

    private float[] NormalizeVector(float[] vector) {
        double l2Norm = Math.Sqrt(vector.Sum(x => x * x));
        return vector.Select(x => (float)(x / l2Norm)).ToArray();
    }
}
