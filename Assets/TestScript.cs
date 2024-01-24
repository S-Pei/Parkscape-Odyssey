using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TestScript : MonoBehaviour 
{

    [SerializeField]
    private TextMeshProUGUI testInt;

    [SerializeField]
    private TextMeshProUGUI testString;

    void Start() {
        AndroidJavaObject p2pObj = new AndroidJavaObject("com.example.p2pnetwork");
        int javaInt = p2pObj.Call<int>("getInt");
        string javaStr = p2pObj.Call<string>("getString");
        testInt.text = javaInt.ToString();
        testString.text = javaStr;
    }
}