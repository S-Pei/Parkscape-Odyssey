using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Unity.Barracuda;
using UnityEngine;
using TMPro;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;


public class VecSearchManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI resultText;

    [SerializeField]
    private NNModel modelAsset;
    private Model m_RuntimeModel;
    private List<float[]> vectors = new List<float[]>();
    private int dimension = 128;
    private int vectorsCount = 100000;
    private int k = 5;
    private static readonly System.Random _rand = new System.Random();
    private float[][] featureVectors;
    private string[] labels;

    private static VecSearchManager instance;

    public static VecSearchManager Instance { 
        get {
            if (instance == null) {
                // To make sure that script is persistent across scenes
                GameObject go = new GameObject("VecSearchManager");
                instance = go.AddComponent<VecSearchManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    // Start is called before the first frame updatesimi
    void Start()
    {
        // Initialize mobilenet encoder
        m_RuntimeModel = ModelLoader.Load(modelAsset);
        Initialize();
        Texture2D imageFromFile = LoadImage("Assets/Resources/speke-monument.jpg");
        string[] outputs = ClassifyImage(imageFromFile);
        Debug.Log(string.Join(", ", outputs));
    }

    // Initialize Approximate NN with training data
    public void Initialize() {
        // TODO: CHANGE AFTER PERSISTENT STORAGE
        // Initialize Approximate NN with training data
        string jsonString = LoadJsonFile("Assets/Resources/metadata.json");
        parseMetadataJson(jsonString);
        ApproxNN.Instance.Initialize(featureVectors, labels);
        ApproxNN.Instance.Save("Assets/Resources/");
    }

    // Classify input image
    public string[] ClassifyImage(Texture2D image)
    {
        image = ResizeImage(image, 224, 224);
        var input = new Tensor(image, channels: 3);
        var worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, m_RuntimeModel);
        worker.Execute(input);
        Tensor output = worker.PeekOutput();

        // transform tensor into float32 vector
        float[] queryFeatureVector = output.data.Download(output.shape);
        Debug.Log("Feature vector: " + string.Join(", ", queryFeatureVector));

        // perform similarity search
        string[] results = ApproxNN.Instance.Search(queryFeatureVector, 5); 
        Debug.Log("Search result: " + string.Join(", ", results));
        worker.Dispose();
        return results;
    }

    Texture2D LoadImage(string path)
    {
        // Read the image bytes from the file path
        byte[] fileData = System.IO.File.ReadAllBytes(path);

        // Create a new Texture2D
        Texture2D texture = new Texture2D(2, 2);

        // Load the image bytes into the Texture2D
        texture.LoadImage(fileData);

        return texture;
    }

    Texture2D ResizeImage(Texture2D originalTexture, int targetWidth, int targetHeight)
    {
        RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 24);
        RenderTexture.active = rt;
        Graphics.Blit(originalTexture, rt);
        
        Texture2D result = new Texture2D(targetWidth, targetHeight);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        Destroy(rt);

        return result;
    }

    private string LoadJsonFile(string jsonFilePath) {
        // Load the JSON file as a text asset
        TextAsset jsonTextAsset = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(jsonFilePath));

        if (jsonTextAsset != null)
        {
            // Access the JSON content as a string
            string jsonString = jsonTextAsset.text;

            // Now you can use the jsonString as needed
            Debug.Log(jsonString);
            return jsonString;
        }
        else
        {
            throw new FileNotFoundException("Failed to load JSON file.");
        }
    }

    private void parseMetadataJson(string jsonString) {
        List<TrainingData> metadata = JsonConvert.DeserializeObject<List<TrainingData>>(jsonString);
        int length = metadata.Count;
        float[][] featureVectors = new float[length][];
        string[] labels = new string[length];
        for (int i = 0; i < length; i++)
        {
            var entry = metadata[i];
            featureVectors[i] = entry.feature_vector;
            labels[i] = entry.label;
        }
        this.featureVectors = featureVectors;
        this.labels = labels;
    }

    private float[] parseTestJson(string jsonString) {
        Dictionary<string, float[]> testData = JsonConvert.DeserializeObject<Dictionary<string, float[]>>(jsonString);
        return testData["feature_vector"];
    }
}

[System.Serializable]
public class TrainingData
{
    public float[] feature_vector;
    public string label;
    public TrainingData(float[] feature_vector, string label)
    {
        this.feature_vector = feature_vector;
        this.label = label;
    }
    
}
