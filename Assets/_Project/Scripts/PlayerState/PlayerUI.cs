using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI pmText;
    public TextMeshProUGUI paText;

    private int selectedCharacter = 0;
    
    public void NextTurn()
    {
        if (BattleManager.Instance.timeline.ActiveCharacter.photonView.IsMine)
        {
            BattleManager.Instance.photonView.RPC("SetNextTurn", RpcTarget.AllBuffered);
        }
        else
        {
            Debug.Log("You are not the Active Player");
        }
        
        nameText.text = " - ";
        lifeText.text = " PV";
        paText.text = " PA";
        pmText.text = " PM";
    }

    private void Update()
    {
        if (BattleManager.Instance == null || BattleManager.Instance.timeline == null)
            return;
        
        if (BattleManager.Instance.timeline.ActiveCharacter == null)
            return;
        
        if (BattleManager.Instance.timeline.ActiveCharacter.photonView == null)
            return;
        
        if (BattleManager.Instance.timeline.ActiveCharacter.photonView.IsMine)
        {
            nameText.text = BattleManager.Instance.timeline.ActiveCharacter.name;
            lifeText.text = BattleManager.Instance.timeline.ActiveCharacter.PlayerStats.currentLife + " PV";
            paText.text = BattleManager.Instance.timeline.ActiveCharacter.PlayerStats.PA + " PA";
            pmText.text = BattleManager.Instance.timeline.ActiveCharacter.PlayerStats.PM + " PM";
        }

        if (selectedCharacter != -1)
        {
            
        }
    }

    public void SelectCharacter(int id)
    {
        if (selectedCharacter == id)
        {
            selectedCharacter = -1;
            return;
        }

        selectedCharacter = id;
    }
    
    public void Attack()
    {
        
    }
}
