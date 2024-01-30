using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerViewManager : MonoBehaviour
{
    private string role;
    
    [SerializeField]
    private TMP_Text playerRoleText;

    [SerializeField]
    private GameObject playerIcon;

    public void SetRole(string role) {
        this.role = role;
        playerRoleText.text = role;
    }

    public void SetRoleIcon(Sprite roleIcon) {
        ((Image) playerIcon.GetComponent(typeof(Image))).sprite = roleIcon;
    }
}
