using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;
public class Character : MonoBehaviourPun, IPunObservable
{
    public string name = "IronMan";
    [SerializeField] Stats stats;
    public Tile position;
    public Stats PlayerStats
    {
        get => this.stats;
        set => this.stats = value;
    }
    
    public List<Tile> moveableTiles = new List<Tile>();

    public bool isMoving = false;
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this.stats.PM);
            stream.SendNext(this.stats.PA);
            stream.SendNext(this.stats.currentLife);
        }
        else
        {
            // Network player, receive data
            this.stats.PM = (int)stream.ReceiveNext();
            this.stats.PA = (int)stream.ReceiveNext();
            this.stats.currentLife = (int)stream.ReceiveNext();
        }
    }
    
    [PunRPC]
    public void SetCharacterName(string name)
    {
        this.name = name;
    }

    [PunRPC]
    public void Damage(int damage)
    {
        this.PlayerStats.currentLife -= damage;
    }

    [PunRPC]
    public void SetCurrentTile(int tileID)
    {
        if (this.position != null)
            this.position.used = false;
        
        var tile = PhotonNetwork.GetPhotonView(tileID).GetComponent<Tile>();
        this.position = tile;
        this.position.used = true;
    }
    
    private void Awake()
    {
        this.stats.currentLife = this.stats.MAX_LIFE;
    }

    public void SearchMoveableTile()
    {
        if (!photonView.IsMine)
            return;

        if (stats.PM <= 0)
        {
            moveableTiles = new List<Tile>();
            return;
        }
        
        moveableTiles = MapManager.GetTilesInRange(position, stats.PM);

        foreach (var i in moveableTiles)
        {
            i.SetColor(Color.yellow);
        }
    }
    
    public IEnumerator MoveToTile(Stack<Tile> path)
    {
        isMoving = true;
        position.used = false;

        Tile tmp = this.position;
        
        float moveSpeed = 0.5f;
        var currentTarget = path.Pop();
        float pos = 0.0f;
        
        while(stats.PM > 0)
        {
            pos += Time.deltaTime;
            var delta = pos / moveSpeed;
            var prevPosition = tmp.transform.position + Vector3.up * 0.5f;
            var targetPos = currentTarget.transform.position + Vector3.up * 0.5f;
            this.transform.position = Vector3.Lerp(prevPosition, targetPos, delta);
            if(delta >= 1.0f)
            {
                delta = 0.0f;
                pos = 0.0f;
                this.transform.position = targetPos;
                tmp = currentTarget;

                if (photonView.IsMine)
                    stats.PM -= 1;
                
                if (path.Count > 0 && stats.PM > 0)
                    currentTarget = path.Pop();
                else
                    break;
            }
            yield return 0;
        }

        isMoving = false;

        var tileID = tmp.photonView.ViewID;
        PhotonNetwork.GetPhotonView(photonView.ViewID).RPC("SetCurrentTile", RpcTarget.AllBuffered, tileID);
        
        SearchMoveableTile();
        
        yield break;
    }

    private Tile GetCurrentTile()
    {
        Ray ray = new Ray(this.transform.position + Vector3.up, Vector3.down);
        RaycastHit hit;
        var layer = MapManager.Instance.tileMask;
        if(Physics.Raycast(ray, out hit, layer))
        {
            return hit.transform.GetComponent<Tile>();
        }
        return null;
    }

}
