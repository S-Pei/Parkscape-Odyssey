using UnityEngine;
using System.Collections;

public class ScannerController : MonoBehaviour
{
    public float speed = 1f; // Adjust the speed of the scanner line
    private RectTransform rectTransform;
    private bool isScannerComplete = false;
    
    float x_pos = 0;
    float starting_y = -1169.4f;
    float width = 1.3059f;
    float height = 0.398f;
    bool justStarted = true;
    private float startTime;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        StartCoroutine(AnimateScanner());
    }

    private IEnumerator AnimateScanner()
    {
        startTime = Time.time;
        rectTransform.sizeDelta = new Vector2(width, height);
        float screenHeight = 2340;
        while (!isScannerComplete)
        {
            // Oscillate the Y position using Mathf.Cos
            float elapsedTime = Time.time - startTime; 
            Debug.Log(Mathf.Sin(elapsedTime * speed / 2));
            float newY = starting_y + Mathf.Sin(elapsedTime * speed / 2) * screenHeight; // Adjust the amplitude (100f) as needed
            rectTransform.anchoredPosition3D = new Vector3(x_pos, newY, 0f);

            // Check if the scanner completes one cycle (up and down)
            if (Mathf.Abs(newY - starting_y) < 4f) // Adjust the threshold as needed
            {
                if (justStarted)
                {
                    Debug.Log("Scanner here");
                    justStarted = false;
                }
                else
                {
                    isScannerComplete = true;
                    // Show Photo Results Function
                    Debug.Log("Scanner complete");
                    Destroy(gameObject);
                }
            }

            yield return null;
        }
    }
}
