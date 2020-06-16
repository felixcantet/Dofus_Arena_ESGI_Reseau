using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerUI : MonoBehaviour
{
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
    }
}
