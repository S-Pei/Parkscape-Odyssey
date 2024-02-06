using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour {
    private static NetworkManager instance;
    
    private NetworkUtils networkUtils;

    public static NetworkManager Instance {
        get {
            if (instance == null) {
                // To make sure that script is persistent across scenes
                GameObject go = new GameObject("NetworkManager");
                instance = go.AddComponent<NetworkManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        } else {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        #if UNITY_ANDROID
        networkUtils = AndroidNetwork.Instance;
        #endif
        #if UNITY_IOS
        networkUtils = IOSNetwork.Instance;
        #endif
        // InvokeRepeating("OnReceive", 0.0f, 1.0f);
    }

    public NetworkUtils NetworkUtils {
        get {
            return networkUtils;
        }
    }

    // public void OnReceive() {
    //     System.Func<Message, CallbackStatus> callback = (Message msgInfo) => {
    //         TestMessageInfo testMsgInfo = (TestMessageInfo) msgInfo;
    //         Debug.Log("ran callback");
    //         Debug.Log("received message: " + testMsgInfo.data);
    //         return CallbackStatus.PROCESSED;
    //     };
    //     networkUtils.onReceive(callback);
    // }
}