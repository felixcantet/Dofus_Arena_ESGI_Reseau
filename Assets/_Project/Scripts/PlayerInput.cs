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
        Debug.LogError("Is Active Player : " + IsLocalPlayerActiveCharacter);
        if (IsLocalPlayerActiveCharacter)
        {
            var tile = GetTileUnderMouse();
            foreach(var item in MapManager.Instance.map)
            {
                item.SetColor(Color.white);
            }

            var startTile = BattleManager.Instance.timeline.ActiveCharacter.position;
            if (Input.GetMouseButtonDown(0))
            {
                var path = MapManager.GetPath(startTile, tile);
                BattleManager.Instance.timeline.ActiveCharacter.StartCoroutine(BattleManager.Instance.timeline.ActiveCharacter.MoveToTile(path));
            }
            if (tile != null)
            {
                var path = MapManager.GetPath(startTile, tile);
                Debug.Log(path.Count);
                while(path.Count != 0)
                {
                    path.Pop().SetColor(Color.red);
                }
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
            return tile;
        }
        return null;
    }
}
