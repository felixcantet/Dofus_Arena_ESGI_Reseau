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
                        //Debug.Log(path.Count);
                        while(path.Count != 0)
                        {
                            path.Pop().SetColor(Color.cyan);
                        }
                    }
                    
                    break;
                
                case CharacterState.MOVE:
                    
                    break;
                
                // TODO : change with new spell system
                case CharacterState.ATTACK_MODE:
                    if (Input.GetMouseButtonDown(1))
                    {
                        BattleManager.Instance.timeline.ActiveCharacter.SwitchToAttackStateToStaticState(0);
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
                            //Je get le spell choisi
                            BaseSpell spell =
                                BattleManager.Instance.timeline.ActiveCharacter.Spells[
                                    BattleManager.Instance.timeline.ActiveCharacter.selectedSpell];
                            
                            //Je demande si les resources sont présentes
                            if (BattleManager.Instance.timeline.ActiveCharacter.ResourcesAvailable() == false)
                            {
                                Debug.Log("Je peux pas lancer le sort !!! zut");
                                BattleManager.Instance.timeline.ActiveCharacter.SwitchToAttackStateToStaticState(0);
                                return;
                            }
                            
                            //si oui, je cherche le character sur la tiles en question
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

                            //Si y'a un character
                            if (c != null)
                            {
                                //Je lui dit de caster le spell
                                PhotonNetwork.GetPhotonView(c.photonView.ViewID).RPC("CastSpell", RpcTarget.AllBuffered, spell);
                                
                                //J'affiche les feed back (a voir si on peut pas les mettre dans le cast de spell)
                                int count = spell.spellActions.Count - 1;
                                foreach (var v in spell.spellActions)
                                {
                                    Vector3 offset = 0.5f * count * Vector3.up;
                                    switch (v.resource)
                                    {
                                        case ResourcesType.PA:
                                            BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllBuffered, 
                                                c.transform.position + offset,
                                                0.0f,
                                                0.25f,
                                                0.78f,
                                                v.value.ToString());
                                            break;
                
                                        case ResourcesType.PM:
                                            BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllBuffered, 
                                                c.transform.position + offset,
                                                0.0f,
                                                0.78f,
                                                0.25f,
                                                v.value.ToString());
                                            break;
                
                                        case ResourcesType.LIFE:
                                            BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllBuffered, 
                                                c.transform.position + offset,
                                                BattleManager.Instance.textEffectPrefab.displayColor.r,
                                                BattleManager.Instance.textEffectPrefab.displayColor.g,
                                                BattleManager.Instance.textEffectPrefab.displayColor.b,
                                                v.value.ToString());
                                            continue;
                                    }

                                    count--;

                                }
                            }
                            
                            //Je process l'attaque
                            BattleManager.Instance.timeline.ActiveCharacter.SetAttackProcess(new Vector3(tileAtck.position.x, 0.0f, tileAtck.position.y));
                        }
                    }
                    break;
                
                case CharacterState.ATTACK_PROCESS:
                    
                    break;
            }
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

            if (tile.used && BattleManager.Instance.timeline.ActiveCharacter.currentState != CharacterState.ATTACK_MODE)
                return null;
            
            return tile;
        }
        return null;
    }
    
}
