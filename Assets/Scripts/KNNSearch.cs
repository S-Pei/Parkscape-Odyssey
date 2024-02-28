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


public class KNNSearch : MonoBehaviour
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
    private double[][] featureVectors;
    private string[] labels;

    // Start is called before the first frame updatesimi
    void Start()
    {
        Texture2D imageFromFile = LoadImage("Assets/Resources/peter_pan_test_img.jpeg");
        imageFromFile = ResizeImage(imageFromFile, 224, 224);
        m_RuntimeModel = ModelLoader.Load(modelAsset);
        var input_names = m_RuntimeModel.inputs;
        var output_names = m_RuntimeModel.outputs;
        Debug.Log("Input names: " + string.Join(", ", input_names));
        Debug.Log("Output names: " + string.Join(", ", output_names));
        Tensor input = new Tensor(imageFromFile, channels: 3);
        Debug.Log("Input shape: " + input.shape);
        var worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, m_RuntimeModel);
        worker.Execute(input);
        Tensor output = worker.PeekOutput();
        Debug.Log("Output shape: " + output.shape);
        Debug.Log("Output: " + output);
        string outputStr = string.Join(", ", output.ToReadOnlyArray());
        resultText.text = outputStr;
        worker.Dispose();
        
        string jsonString = LoadJsonFile("Assets/Resources/metadata.json");
        parseMetadataJson(jsonString);
        string testJsonString = LoadJsonFile("Assets/Resources/test_metadata_1.json");
        var queryFeatureVector = parseTestJson(testJsonString);

        // Initialize KNN with training data
        KNN.Instance.Initialize(featureVectors, labels);
        resultText.text = KNN.Instance.Search(queryFeatureVector, 10);
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
        double[][] featureVectors = new double[length][];
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

    private double[] parseTestJson(string jsonString) {
        Dictionary<string, double[]> testData = JsonConvert.DeserializeObject<Dictionary<string, double[]>>(jsonString);
        return testData["feature_vector"];
    }
}

[System.Serializable]
public class TrainingData
{
    public double[] feature_vector;
    public string label;
    public TrainingData(double[] feature_vector, string label)
    {
        this.feature_vector = feature_vector;
        this.label = label;
    }
    
}
