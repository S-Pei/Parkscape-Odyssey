using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BattleCardRenderer : CardRenderer, IBeginDragHandler, IEndDragHandler, IDragHandler {
    private RectTransform rectTransform;
    // private BattleUIManager battleUIManager;

    
    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        // battleUIManager = (BattleUIManager) GetComponent(typeof(BattleUIManager));
    }

    public void OnBeginDrag(PointerEventData eventData) {
        // Debug.Log("OnBeginDrag");
        // No rotation while moving the card around
        rectTransform.rotation = Quaternion.identity;
    }
    public void OnEndDrag(PointerEventData eventData) {
        // Debug.Log("OnEndDrag");
        StartCoroutine(ResetCardPosition(0.3f));
    }
    public void OnDrag(PointerEventData eventData) {
        // Move the card to follow the pointer every frame
        rectTransform.anchoredPosition += eventData.delta;
    }

    private IEnumerator ResetCardPosition (float time) {
        Vector3 startingPosition  = rectTransform.position;
        Vector3 startingRotation  = rectTransform.eulerAngles;

        (Vector3 defaultPosition, Quaternion defaultRotation) =
            BattleUIManager.getCardPositionAtIndex(this.cardIndex);

        float elapsedTime = 0;
        
        while (elapsedTime < time) {
            rectTransform.position = Vector3.Lerp(startingPosition, defaultPosition, (elapsedTime / time));
            rectTransform.rotation = Quaternion.Lerp(
                Quaternion.Euler(startingRotation.x, startingRotation.y, startingRotation.z),
                defaultRotation,
                (elapsedTime / time)
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}