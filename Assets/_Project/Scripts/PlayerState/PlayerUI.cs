using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Character Icon")]
    public Image[] teamA = new Image[0];
    public Image[] teamB = new Image[0];
    
    [Header("Panel Info")]
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI pmText;
    public TextMeshProUGUI paText;

    [Header("Spell Info")] 
    public Image[] spellImage = new Image[0];
    
    [Header("Timer Text")] 
    public TextMeshProUGUI timerText;
    
    private int selectedCharacter = -1;

    private IEnumerator Start()
    {
        while (BattleManager.Instance.teams.Count < 2)
        {
            yield return new WaitForSeconds(0.01f);
        }

        for (int i = 0; i < teamA.Length; i++)
        {
            teamA[i].sprite = BattleManager.Instance.teams[0].characters[i].characterIcon;
        }
        
        for (int i = 0; i < teamB.Length; i++)
        {
            teamB[i].sprite = BattleManager.Instance.teams[1].characters[i].characterIcon;
        }
    }

    public void NextTurn()
    {
        selectedCharacter = -1;
        if (BattleManager.Instance.timeline.ActiveCharacter.photonView.IsMine)
        {
            if (BattleManager.Instance.timeline.ActiveCharacter.currentState == CharacterState.MOVE ||
                BattleManager.Instance.timeline.ActiveCharacter.currentState == CharacterState.ATTACK_PROCESS || 
                BattleManager.Instance.timeline.ActiveCharacter.currentState == CharacterState.DEAD)
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

    public static void NextTurnAfterTimer()
    {
        Debug.LogError("Je suis call ---- > ");
        {
            foreach(var item in MapManager.Instance.map)
            {
                item.SetColor(Color.white);
            }
            
            BattleManager.Instance.photonView.RPC("SetNextTurn", RpcTarget.AllBuffered);
        }
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
                lifeText.text = c.stats.currentLife + " PV";
                paText.text = c.stats.PA + " PA";
                pmText.text = c.stats.PM + " PM";
            }
            else
            {
                nameText.text = BattleManager.Instance.timeline.ActiveCharacter.name;
                lifeText.text = BattleManager.Instance.timeline.ActiveCharacter.stats.currentLife + " PV";
                paText.text = BattleManager.Instance.timeline.ActiveCharacter.stats.PA + " PA";
                pmText.text = BattleManager.Instance.timeline.ActiveCharacter.stats.PM + " PM";
            }

            for (int i = 0; i < BattleManager.Instance.timeline.ActiveCharacter.Spells.Count; i++)
            {
                spellImage[i].sprite = BattleManager.Instance.timeline.ActiveCharacter.Spells[i].spellIcon;
            }
            
            timerText.text = BattleManager.Instance.currentTurnTime.ToString("00");
        }
        else
        {
            if (selectedCharacter >= 0)
            {
                Character c = BattleManager.Instance.teams[selectedCharacter > 2 ? 1 : 0]
                    .characters[selectedCharacter > 2 ? selectedCharacter - 3 : selectedCharacter];
                
                nameText.text = c.name;
                lifeText.text = c.stats.currentLife + " PV";
                paText.text = c.stats.PA + " PA";
                pmText.text = c.stats.PM + " PM";
            }
            else
            {
                nameText.text = " - ";
                lifeText.text = " PV";
                paText.text = " PA";
                pmText.text = " PM";
            }
            
            timerText.text = "00";
        }

        Button a;

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
