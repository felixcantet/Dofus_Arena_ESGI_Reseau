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

    private int selectedCharacter = -1;
    
    public void NextTurn()
    {
        selectedCharacter = -1;
        if (BattleManager.Instance.timeline.ActiveCharacter.photonView.IsMine)
        {
            foreach(var item in MapManager.Instance.map)
            {
                item.SetColor(Color.white);
            }
            
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
            
            if (selectedCharacter >= 0)
            {
                Character c = BattleManager.Instance.teams[selectedCharacter > 2 ? 1 : 0]
                    .characters[selectedCharacter > 2 ? selectedCharacter - 3 : selectedCharacter];
                
                nameText.text = c.name;
                lifeText.text = c.PlayerStats.currentLife + " PV";
                paText.text = c.PlayerStats.PA + " PA";
                pmText.text = c.PlayerStats.PM + " PM";
            }
            else
            {
                nameText.text = BattleManager.Instance.timeline.ActiveCharacter.name;
                lifeText.text = BattleManager.Instance.timeline.ActiveCharacter.PlayerStats.currentLife + " PV";
                paText.text = BattleManager.Instance.timeline.ActiveCharacter.PlayerStats.PA + " PA";
                pmText.text = BattleManager.Instance.timeline.ActiveCharacter.PlayerStats.PM + " PM";
            }
        }
        else
        {
            if (selectedCharacter >= 0)
            {
                Character c = BattleManager.Instance.teams[selectedCharacter > 2 ? 1 : 0]
                    .characters[selectedCharacter > 2 ? selectedCharacter - 3 : selectedCharacter];
                
                nameText.text = c.name;
                lifeText.text = c.PlayerStats.currentLife + " PV";
                paText.text = c.PlayerStats.PA + " PA";
                pmText.text = c.PlayerStats.PM + " PM";
            }
            else
            {
                nameText.text = " - ";
                lifeText.text = " PV";
                paText.text = " PA";
                pmText.text = " PM";
            }
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
    
    public void SelectAttack()
    {
        if (!BattleManager.Instance.timeline.ActiveCharacter.photonView.IsMine)
            return;

        if (BattleManager.Instance.timeline.ActiveCharacter.currentState != CharacterState.STATIC)
            return;
        
        BattleManager.Instance.timeline.ActiveCharacter.SwitchToAttackStateToStaticState();
        //call function
    }
}
