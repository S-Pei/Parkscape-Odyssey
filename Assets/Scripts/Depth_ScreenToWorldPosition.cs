using Niantic.Lightship.AR.Utilities;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class Depth_ScreenToWorldPosition : MonoBehaviour
{
    public AROcclusionManager _occMan;
    public Camera _camera;
    public GameObject _prefabToSpawn;

    [SerializeField]
    private GameObject segmentationManager;
    private SemanticQuerying semanticQuerying;

    [SerializeField]
    private GameObject gameManagerObj;
    private GameManager gameManager;


    // FISHING
    [SerializeField]
    private GameObject overlayCamera;
    private LineRenderer fishingRod;
    private Vector3? fishingAnchorPosition = null;

    [SerializeField]
    private GameObject waterRippleEffect;
    private bool isFishing = false;

    void Start() 
    {
        semanticQuerying = segmentationManager.GetComponent<SemanticQuerying>();
        gameManager = gameManagerObj.GetComponent<GameManager>();

        fishingRod = overlayCamera.GetComponent<LineRenderer>();
        fishingRod.positionCount = 2;
    }

    XRCpuImage? depthimage;
    void Update()
    {

        if (!_occMan.subsystem.running)
        {
            return;
        }

        Matrix4x4 displayMat = Matrix4x4.identity;

        if (_occMan.TryAcquireEnvironmentDepthCpuImage(out var image))
        {
            depthimage?.Dispose();
            depthimage = image;
        }
        else
        {
            return;
        }

        if (fishingAnchorPosition != null && isFishing) {
            // Sample eye depth
            var uvt = new Vector2(1 / 2, 1 / 2);
            var eyeDeptht = depthimage.Value.Sample<float>(uvt, displayMat);

            var centerWorldPosition =
                _camera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, eyeDeptht));

            fishingRod.SetPosition(0, fishingAnchorPosition.Value);
            fishingRod.SetPosition(1, centerWorldPosition);
        }

        

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            var screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
#else
        if(Input.touches.Length>0)
        {
            var screenPosition = Input.GetTouch(0).position;
#endif
            if (depthimage.HasValue)
            {
                // Sample eye depth
                var uv = new Vector2(screenPosition.x / Screen.width, screenPosition.y / Screen.height);
                var eyeDepth = depthimage.Value.Sample<float>(uv, displayMat);

                // Get world position
                var worldPosition =
                    _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, eyeDepth));
                fishingAnchorPosition = worldPosition;

                // Get semantics of screen position touched
                string channelName = semanticQuerying.GetPositionChannel((int) screenPosition.x, (int) screenPosition.y);
                if (channelName == "ground") {
                    ShowFishingLayer();
                    waterRippleEffect.transform.position = worldPosition;
                    isFishing = true;
                } else {
                    CloseFishingLayer();
                    isFishing = false;
                }
                
                // gameManager.LogTxt("Screen width: " + Screen.width + " Screen height: " + Screen.height);
                // gameManager.LogTxt($"Screen position: {screenPosition.x}, {screenPosition.y}");

                // Get overlay world position
                // var overlayWorldPosition =
                //     overlayCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, eyeDepth));
            }
        }
    }

    private void ShowFishingLayer() {
        overlayCamera.SetActive(true);
        waterRippleEffect.SetActive(true);
    }

    private void CloseFishingLayer() {
        overlayCamera.SetActive(false);
        waterRippleEffect.SetActive(false);
    }
}