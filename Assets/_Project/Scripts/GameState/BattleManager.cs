using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Sirenix.OdinInspector;

public class BattleManager : NetworkSingleton<BattleManager>, IPunObservable
{
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    public List<Team> teams;

    public Timeline timeline;

    public static bool battleStart = false;

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
        
        timeline.ActiveCharacter.SearchMoveableTile();
    }
    
    [PunRPC]
    public void SetNextTurn()
    {
        timeline.ActiveCharacter.PlayerStats.PM = timeline.ActiveCharacter.PlayerStats.DEFAULT_PM;
        timeline.ActiveCharacter.PlayerStats.PA = timeline.ActiveCharacter.PlayerStats.DEFAULT_PA;
        
        this.timeline.SetNextTurn();
    }
}
