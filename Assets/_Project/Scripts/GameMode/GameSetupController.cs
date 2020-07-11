using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
public class GameSetupController : MonoBehaviour
{

    public SpawnPositions spawnPosition;
    void Start()
    {
        StartCoroutine(WaitForSetup());
    }

    public IEnumerator WaitForSetup()
    {
        yield return new WaitForSeconds(0.5f);
        this.CreateTeam();
    }

    private void CreateTeam()
    {
        Team team = new Team();
        team.charactersID = new List<int>();
        var teamID = BattleManager.Instance.teams.Count;
        Tile targetTile = null;
        var paths = new string[3];
        paths[0] = Path.Combine("PhotonPrefabs", "Chomper");
        paths[1] = Path.Combine("PhotonPrefabs", "Ellen");
        paths[2] = Path.Combine("PhotonPrefabs", "Grenadier");
        for (int i = 0; i < 3; i++)
        {
            var pos = Vector3.zero;
            if (teamID == 0)
            {
                pos = spawnPosition.spawnPositionTeam1[i].position;
                targetTile = spawnPosition.spawnPositionTeam1[i].GetComponent<Tile>();
            }
            else
            {
                pos = spawnPosition.spawnPositionTeam2[i].position;
                targetTile = spawnPosition.spawnPositionTeam2[i].GetComponent<Tile>();
            }
            
            var go = PhotonNetwork.Instantiate(paths[i], pos + Vector3.up * 0.5f, Quaternion.identity);
            var id = go.GetPhotonView().ViewID;
            team.charactersID.Add(id);
            PhotonNetwork.GetPhotonView(id).RPC("SetCharacterName", RpcTarget.AllBuffered, id + "_" + i);
            var tileID = targetTile.photonView.ViewID;
            PhotonNetwork.GetPhotonView(id).RPC("SetCurrentTile", RpcTarget.AllBuffered, tileID);
            //PhotonNetwork.GetPhotonView(id).GetComponent<Character>().name = id + "_" + i;
        }

        BattleManager.Instance.photonView.RPC("AddTeam", RpcTarget.AllBuffered, team);

        if (!PhotonNetwork.IsMasterClient)
        {
            BattleManager.Instance.photonView.RPC("LogFromBuild", RpcTarget.All, BattleManager.Instance.teams[0].characters[0].name.ToString());
        }

        if (BattleManager.Instance.teams.Count == 2)
        {

            BattleManager.Instance.photonView.RPC("BuildTimeline", RpcTarget.AllBuffered);
            if (!PhotonNetwork.IsMasterClient)
            {
              
                BattleManager.Instance.photonView.RPC("LogFromBuild", RpcTarget.All, "Active Player : " + BattleManager.Instance.timeline.ActiveCharacter.name.ToString());

            }
        }

    }

    private void SetupBattle()
    {

    }
}
