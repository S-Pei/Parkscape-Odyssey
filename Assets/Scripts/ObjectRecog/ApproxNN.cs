using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using HNSW.Net;

public class ApproxNN : MonoBehaviour
{
    private const string VectorsPathSuffix = "vectors.bytes";
    private const string LabelsPathSuffix = "labels.bytes";
    private const string GraphpathSuffix = "world.bytes";

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

        // Construct initial world
        var parameters = new SmallWorld<float[], float>.Parameters()
            {
                M = 15,
                LevelLambda = 1 / Math.Log(15),
            };
        this.world = new SmallWorld<float[], float>(CosineDistance.NonOptimized, DefaultRandomGenerator.Instance, parameters);
        this.world.AddItems(input);
        Debug.Log("World constructed");

        this.vectors = input;
        this.labels = labels;
    }

    /*** Performs KNN search and outputs label ***/
    public string Search(float[] query, int k = 3)
    {
        var results = this.world.KNNSearch(query, k);
        var indices = results.Select(r => r.Id).ToArray();
        var labels = indices.Select(i => this.labels[i]).ToArray();
        return string.Join(", ", labels);
        // var bestResult = results.OrderBy(r => r.Distance).First();
        // Debug.Log("Search result: " + bestResult.ToString());
        // return bestResult.ToString();
    }

    public void Save(string path) {
        BinaryFormatter formatter = new BinaryFormatter();

        MemoryStream sampleVectorsStream = new MemoryStream();
        formatter.Serialize(sampleVectorsStream, vectors);
        File.WriteAllBytes($"{path}.{VectorsPathSuffix}", sampleVectorsStream.ToArray());

        MemoryStream labelsStream = new MemoryStream();
        formatter.Serialize(labelsStream, labels);
        File.WriteAllBytes($"{path}.{LabelsPathSuffix}", labelsStream.ToArray());

        using (var f = File.Open($"{path}.{GraphpathSuffix}", FileMode.Create))
        {
            world.SerializeGraph(f);
        }
    }

    public void Load(string path) {
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream sampleVectorsStream = new MemoryStream(File.ReadAllBytes($"{path}.{VectorsPathSuffix}"));
        vectors = (float[][])formatter.Deserialize(sampleVectorsStream);

        MemoryStream labelsStream = new MemoryStream(File.ReadAllBytes($"{path}.{LabelsPathSuffix}"));
        labels = (string[])formatter.Deserialize(labelsStream);

        using (var f = File.OpenRead($"{path}.{GraphpathSuffix}"))
        {
            world = SmallWorld<float[], float>.DeserializeGraph(vectors, CosineDistance.NonOptimized, DefaultRandomGenerator.Instance, f);
        }

        Debug.Log("World loaded");
    }
}
