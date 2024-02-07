using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterRenderer : MonoBehaviour
{
    // private BoxCollider2D collider;
    private Monster monster;
    private Sprite monsterImage;
    private int monsterHealth;

    private GameObject monsterImgObj;
    private Image monsterImgRenderer;

    void Awake() {
        monsterImgObj = transform.GetChild(0).gameObject;
        monsterImgRenderer = monsterImgObj.GetComponentInChildren<Image>();
    }

    void Start() {
        GetComponent<BoxCollider2D>().size = monsterImgRenderer.rectTransform.rect.size;
        Debug.Log("Collider:");
        Debug.Log(GetComponent<BoxCollider2D>().size.ToString("f7"));
    }

    // Runs whenever a collision (between Colliders) is detected
    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.GetComponent<BattleCardRenderer>()) {
            // Collision with card; highlight the sprite
            monsterImgRenderer.color = new Color32(255, 0, 0, 255);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if(other.gameObject.GetComponent<BattleCardRenderer>()) {
            // Collision with card; highlight the sprite
            monsterImgRenderer.color = new Color32(255, 255, 255, 255);
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        Debug.Log("COLLISION DETECTED!");
        if(other.gameObject.GetComponent<BattleCardRenderer>()) {
            Debug.Log("WITH CARD TOO");
            // Collision with card; highlight the sprite
            monsterImgRenderer.color = new Color32(255, 0, 0, 255);
        }
    }


    public void renderMonster(Monster monster) {
        // Render card image
        
        monsterImgRenderer.sprite = monster.img;
        monsterImage = monster.img;


        // Render monster health
        GameObject monsterHealthObj = transform.GetChild(1).gameObject;
        GameObject monsterHealthValue = monsterHealthObj.transform.GetChild(2).gameObject;
        TextMeshProUGUI textComp = monsterHealthValue.GetComponentInChildren<TextMeshProUGUI>();
        textComp.text = monster.Health.ToString();
        monsterHealth = monster.Health;
    }

    public Monster getMonsterDetails() {
        return monster;
    }
}
