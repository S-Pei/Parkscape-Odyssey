using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BattleCardRenderer : CardRenderer, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler {
    private RectTransform rectTransform;
    private Vector3 defaultPosition;
    private Vector3 defaultRotation;
    
    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        // Debug.Log("OnBeginDrag");
    }
    public void OnEndDrag(PointerEventData eventData) {
        // Debug.Log("OnEndDrag");
        StartCoroutine(ResetCardPosition(0.3f));
    }
    public void OnDrag(PointerEventData eventData) {
        // Move the card to follow the pointer every frame
        rectTransform.anchoredPosition += eventData.delta;
    }
    public void OnPointerDown(PointerEventData eventData) {
        // Debug.Log($"{rectTransform.anchoredPosition}, {rectTransform.eulerAngles}");
        // Store the original position for snapping back
        defaultPosition = rectTransform.position;
        defaultRotation = rectTransform.eulerAngles;

        // No rotation while moving the card around
        rectTransform.rotation = Quaternion.identity;
    }

    private IEnumerator ResetCardPosition (float time) {
        Vector3 startingPosition  = rectTransform.position;
        Vector3 startingRotation  = rectTransform.eulerAngles;

        float elapsedTime = 0;
        
        while (elapsedTime < time) {
            rectTransform.position = Vector3.Lerp(startingPosition, defaultPosition, (elapsedTime / time));
            rectTransform.rotation = Quaternion.Lerp(
                Quaternion.Euler(startingRotation.x, startingRotation.y, startingRotation.z),
                Quaternion.Euler(defaultRotation.x, defaultRotation.y, defaultRotation.z),
                (elapsedTime / time)
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}