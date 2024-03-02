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


    void Start() 
    {
        semanticQuerying = segmentationManager.GetComponent<SemanticQuerying>();
        gameManager = gameManagerObj.GetComponent<GameManager>();
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

                // Get semantics of screen position touched
                string channelName = semanticQuerying.GetPositionChannel((int) screenPosition.x, (int) screenPosition.y);
                gameManager.LogTxt($"SEMANTICS: Touched on a {channelName}");

                //spawn a thing on the depth map
                Instantiate(_prefabToSpawn, worldPosition, Quaternion.identity);

            }
        }
    }
}