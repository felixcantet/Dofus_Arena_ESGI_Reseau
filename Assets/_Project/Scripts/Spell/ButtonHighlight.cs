using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject panelDescription;
    public int sortId = 0;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!BattleManager.Instance.timeline.ActiveCharacter.photonView.IsMine)
            return;
        
        //Enable description
        panelDescription.transform.position = transform.position;
        panelDescription.SetActive(true);
        panelDescription.GetComponentInChildren<TextMeshProUGUI>().text =
            BattleManager.Instance.timeline.ActiveCharacter.Spells[sortId].spellDescription;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        //Disable description
        panelDescription.SetActive(false);
    }
}
