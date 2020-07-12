using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Tile : MonoBehaviourPun, IPunObservable
{
    [Header("Tile informations")]
    public TileType tileType = TileType.NORMAL;
    public Vector2Int position;
    public bool used;
    
    [Header("Neighbours")]
    public LayerMask tileDetection;
    public List<Tile> neighbours;

    [Header("Tile Behavior")] 
    public MeshRenderer rd;
    public GameObject obstacle;
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //stream.SendNext(position);
            stream.SendNext(used);
        }
        if (stream.IsReading)
        {
            var tmp = used;
            this.used = (bool)stream.ReceiveNext();
            if (this.used == true && this.used != tmp)
            {
                this.GetComponent<Renderer>().material.color = Color.red;
            }
            else if (this.used == false && this.used != tmp)
            {
                this.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

    public void Awake()
    {
        if (rd == null)
            rd = GetComponent<MeshRenderer>();
        
        this.position = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(this.transform.position.z));
    }

    private void Start()
    {
        GetNeighbours();

        switch (tileType)
        {
            case TileType.NORMAL:
                break;
            
            case TileType.OBSTACLE_VIDE:
                rd.enabled = false;
                break;
            
            case TileType.OBSTACLE:
                obstacle.SetActive(true);
                break;
        }
        
        MapManager.Instance.AddTile(this.photonView.ViewID);
        //MapManager.Instance.photonView.RPC("AddTile", RpcTarget.AllBuffered, this.photonView.ViewID);
    }

    void GetNeighbours()
    {

        var tileCenter = GetComponent<Renderer>().bounds.center;
        var tiles = Physics.OverlapBox(tileCenter, new Vector3(0.75f, 0.75f, 0.75f), Quaternion.identity, tileDetection);

        foreach (var item in tiles)
        {
            var tile = item.GetComponent<Tile>();
            if (tile != null)
            {
                if (Vector2Int.Distance(position, tile.position) < 1.01f && tile != this)
                    this.neighbours.Add(tile);
            }
        }
    }
    public void SetColor(Color newColor)
    {
        this.GetComponent<Renderer>().material.color = newColor;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetComponent<Renderer>().bounds.center, new Vector3(1.5f, 1.5f, 1.5f));
    }
}

public enum TileType
{
    NORMAL,
    OBSTACLE_VIDE,
    OBSTACLE
}