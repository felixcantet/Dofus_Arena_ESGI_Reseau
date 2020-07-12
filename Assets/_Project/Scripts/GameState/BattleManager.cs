using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.UI;

public class BattleManager : NetworkSingleton<BattleManager>, IPunObservable
{
    [Header("UI Effect")]
    public TextEffect textEffectPrefab;

    [Header("Spell Effect")] 
    public List<GameObject> spellEffects;
    
    [Header("Character Button")]
    public Button[] teamA_button = new Button[0];
    public Button[] teamB_button = new Button[0];
    

    public List<Team> teams;

    public Timeline timeline;

    public static bool battleStart = false;

    public float turnTime = 30.0f;
    public float currentTurnTime = 30.0f;
    
    #region PUN Callbacks
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentTurnTime);
        }
        else if (stream.IsReading)
        {
            this.currentTurnTime = (float) stream.ReceiveNext();
        }
    }
    
    [PunRPC]
    public void AddTeam(Team team)
    {
        team.characters = new List<Character>();
        teams.Add(team);

        for(int i = 0; i < team.charactersID.Count; i++)
        {
            team.characters.Add(PhotonNetwork.GetPhotonView(team.charactersID[i]).GetComponent<Character>());
        }
    }
    
    [PunRPC]
    public void LogFromBuild(string message)
    {
        Debug.Log("BUILD MESSAGE : " + message);
    }
    
    [PunRPC]
    public void BuildTimeline()
    {
        this.timeline = new Timeline(teams);
        battleStart = true;

        timeline.ActiveCharacter.SetActiveCharacter();//SearchMoveableTile(timeline.ActiveCharacter.PlayerStats.PM);
        
        bool finded = false;
        for (int i = 0; i < teams.Count; i++)
        {
            for (int j = 0; j < teams[i].characters.Count; j++)
            {
                if (timeline.ActiveCharacter == teams[i].characters[j])
                {
                    finded = true;

                    
                    SetActifAvatar(j, i);
                    break;
                }
            }

            if (finded)
                break;
        }
        
        if (timeline.ActiveCharacter.photonView.IsMine)
            StartCoroutine(nameof(Chrono));
    }
    
    [PunRPC]
    public void SetNextTurn()
    {
        StopAllCoroutines();
        
        timeline.ActiveCharacter.PlayerStats.PM = timeline.ActiveCharacter.PlayerStats.DEFAULT_PM;
        timeline.ActiveCharacter.PlayerStats.PA = timeline.ActiveCharacter.PlayerStats.DEFAULT_PA;

        for (int i = 0; i < teamA_button.Length; i++)
        {
            teamA_button[i].image.color = Color.white;
            teamB_button[i].image.color = Color.white;
        }
        
        this.timeline.SetNextTurn();

        bool finded = false;
        for (int i = 0; i < teams.Count; i++)
        {
            for (int j = 0; j < teams[i].characters.Count; j++)
            {
                if (timeline.ActiveCharacter == teams[i].characters[j])
                {
                    finded = true;

                    
                    SetActifAvatar(j, i);
                    break;
                }
            }

            if (finded)
                break;
        }

        if (photonView.IsMine)
            StartCoroutine(nameof(Chrono));
    }

    [PunRPC]
    public void DisplayTextEffect(Vector3 pos, float r, float g, float b, string txt)
    {
        TextEffect t = Instantiate(BattleManager.Instance.textEffectPrefab, pos + Vector3.up * 1.5f,
            BattleManager.Instance.textEffectPrefab.transform.rotation);
        t.displayColor = new Color(r, g, b, 0.0f);
        t.text.text = txt;
    }

    [PunRPC]
    public void DisplaySpellEffect(Vector3 pos, int spellEffectId)
    {
        pos.y = spellEffects[spellEffectId].transform.position.y;
        Instantiate(spellEffects[spellEffectId], pos, spellEffects[spellEffectId].transform.rotation);
    }
    #endregion
    
    public void SetActifAvatar(int id, int team)
    {
        if (team == 0)
        {
            teamA_button[id].image.color = Color.yellow;
        }
        else
        {
            teamB_button[id].image.color = Color.yellow;
        }
    }

    private IEnumerator Chrono()
    {
        currentTurnTime = turnTime;
        yield return new WaitForSeconds(1.0f);

        while (currentTurnTime > 0.0f)
        {
            yield return new WaitForSeconds(1.0f);
            currentTurnTime -= 1.0f;

            if (currentTurnTime <= 0.0f)
                break;
        }
        
        PlayerUI.NextTurnAfterTimer();
        yield break;
    }
}
