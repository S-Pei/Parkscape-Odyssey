using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameInterfaceManager gameInterfaceManager;

    // Start is called before the first frame update
    void Start() {
        gameInterfaceManager = GetComponent<GameInterfaceManager>();
        gameInterfaceManager.SetUpInterface();
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void OpenInventory() {
        gameInterfaceManager.OpenInventory(GameState.Instance.MyCards);
    }

    public void CloseInventory() {
        gameInterfaceManager.CloseInventory();
    }

    public void OpenPlayerView() {
        gameInterfaceManager.OpenPlayerView();
    }
}
