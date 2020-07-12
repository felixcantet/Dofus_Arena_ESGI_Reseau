using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Character Text")]
    public TextMeshProUGUI[] teamA = new TextMeshProUGUI[0];
    public TextMeshProUGUI[] teamB = new TextMeshProUGUI[0];
    
    [Header("Panel Info")]
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI pmText;
    public TextMeshProUGUI paText;

    private int selectedCharacter = -1;

    private IEnumerator Start()
    {
        while (BattleManager.Instance.teams.Count < 2)
        {
            yield return new WaitForSeconds(0.01f);
        }

        for (int i = 0; i < teamA.Length; i++)
        {
            teamA[i].text = BattleManager.Instance.teams[0].characters[i].name;
        }
        
        for (int i = 0; i < teamB.Length; i++)
        {
            teamB[i].text = BattleManager.Instance.teams[1].characters[i].name;
        }
    }

    public void NextTurn()
    {
        selectedCharacter = -1;
        if (BattleManager.Instance.timeline.ActiveCharacter.photonView.IsMine)
        {
            if (BattleManager.Instance.timeline.ActiveCharacter.currentState != CharacterState.STATIC)
                return;
            
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
    
    public void SelectAttack(int spellId)
    {
        if (!BattleManager.Instance.timeline.ActiveCharacter.photonView.IsMine)
            return;

        if (BattleManager.Instance.timeline.ActiveCharacter.currentState != CharacterState.STATIC)
            return;
        
        Debug.Log("Spell " + spellId.ToString() + " Chosen --- Player UI");
        
        BattleManager.Instance.timeline.ActiveCharacter.SwitchToAttackStateToStaticState(spellId);
        //call function
    }
    
    
}
