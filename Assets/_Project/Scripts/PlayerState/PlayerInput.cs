using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;

public class PlayerInput : MonoBehaviour
{
    public Camera camera;
    
    public static bool IsLocalPlayerActiveCharacter
    {
        get
        {
            return PhotonNetwork.LocalPlayer == BattleManager.Instance.timeline.ActiveCharacter.photonView.Owner;
        } 
    }

    private void Update()
    {
        if (!BattleManager.battleStart)
            return;
        
        Debug.LogWarning("Is Active Player : " + IsLocalPlayerActiveCharacter);
        
        if (IsLocalPlayerActiveCharacter)
        {
            switch (BattleManager.Instance.timeline.ActiveCharacter.currentState)
            {
                case CharacterState.STATIC:
                    var tile = GetTileUnderMouse();
            
                    foreach(var item in MapManager.Instance.map)
                    {
                        if(BattleManager.Instance.timeline.ActiveCharacter.moveableTiles.Contains(item))
                            item.SetColor(Color.yellow);
                        else
                            item.SetColor(Color.white);
                    }

                    var startTile = BattleManager.Instance.timeline.ActiveCharacter.position;
            
                    if (Input.GetMouseButtonDown(0) && tile != null)
                    {
                        var path = MapManager.GetPath(startTile, tile);
                        BattleManager.Instance.timeline.ActiveCharacter.StartCoroutine(BattleManager.Instance.timeline.ActiveCharacter.MoveToTile(path));
                    }
            
                    if (tile != null && BattleManager.Instance.timeline.ActiveCharacter.PlayerStats.PM > 0)
                    {
                        var path = MapManager.GetPath(startTile, tile);
                        Debug.Log(path.Count);
                        while(path.Count != 0)
                        {
                            path.Pop().SetColor(Color.cyan);
                        }
                    }
                    
                    break;
                
                case CharacterState.MOVE:
                    
                    break;
                
                case CharacterState.ATTACK_MODE:
                    if (Input.GetMouseButtonDown(1))
                    {
                        BattleManager.Instance.timeline.ActiveCharacter.SwitchToAttackStateToStaticState();
                    }
                    
                    foreach(var item in MapManager.Instance.map)
                    {
                        if(BattleManager.Instance.timeline.ActiveCharacter.moveableTiles.Contains(item))
                            item.SetColor(new Color(1, 0.6f, 1, 1));
                        else
                            item.SetColor(Color.white);
                    }
                    
                    var tileAtck = GetTileUnderMouse();
                    if (tileAtck != null)
                    {
                        tileAtck.SetColor(Color.red);
                        if (Input.GetMouseButtonDown(0))
                        {
                            if (BattleManager.Instance.timeline.ActiveCharacter.PlayerStats.PA < 3)
                            {
                                BattleManager.Instance.timeline.ActiveCharacter.SwitchToAttackStateToStaticState();
                                return;
                            }
                            
                            Character c = null;
                            foreach (var team in BattleManager.Instance.teams)
                            {
                                foreach (var chara in team.characters)
                                {
                                    if (chara.position == tileAtck)
                                    {
                                        c = chara;
                                        break;
                                    }
                                }

                                if (c != null)
                                    break;
                            }

                            if (c != null)
                            {
                                PhotonNetwork.GetPhotonView(c.photonView.ViewID).RPC("Damage", 
                                    RpcTarget.AllBuffered, BattleManager.Instance.timeline.ActiveCharacter.PlayerStats.DAMAGE);
                                
                                BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllBuffered, c.transform.position,
                                    BattleManager.Instance.textEffectPrefab.displayColor.r,
                                    BattleManager.Instance.textEffectPrefab.displayColor.g,
                                    BattleManager.Instance.textEffectPrefab.displayColor.b,
                                    "-" +  BattleManager.Instance.timeline.ActiveCharacter.PlayerStats.DAMAGE.ToString());
                            }
                            
                            BattleManager.Instance.timeline.ActiveCharacter.SetAttackProcess();
                        }
                    }
                    break;
                
                case CharacterState.ATTACK_PROCESS:
                    
                    break;
            }
            
            if (BattleManager.Instance.timeline.ActiveCharacter.currentState != CharacterState.STATIC)
                return;
            
            
        }
    }

    private Tile GetTileUnderMouse()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        var layerMask = LayerMask.GetMask("Tile");
        if(Physics.Raycast(ray, out hit, layerMask))
        {
            var tile = hit.transform.GetComponent<Tile>();

            if (!BattleManager.Instance.timeline.ActiveCharacter.moveableTiles.Contains(tile))
                return null;

            if (tile.used)
                return null;
            
            return tile;
        }
        return null;
    }
    
}
