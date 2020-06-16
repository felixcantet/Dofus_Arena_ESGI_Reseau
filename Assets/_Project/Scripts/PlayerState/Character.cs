using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;
public class Character : MonoBehaviourPun, IPunObservable
{
    public string name;
    [SerializeField] Stats stats;
    public Tile position;
    public Stats PlayerStats
    {
        get => this.stats;
        set => this.stats = value;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
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
        var tile = PhotonNetwork.GetPhotonView(tileID).GetComponent<Tile>();
        this.position = tile;
    }

    private void Awake()
    {
        this.stats.currentLife = this.stats.maxLife;
    }
    private void Start()
    {
        
        //this.position = GetCurrentTile();
    }

    public IEnumerator MoveToTile(Stack<Tile> path)
    {
        float moveSpeed = 0.5f;
        var currentTarget = path.Pop();
        float pos = 0.0f;
        while(true)
        {
            pos += Time.deltaTime;
            var delta = pos / moveSpeed;
            var prevPosition = this.position.transform.position + Vector3.up * 0.5f;
            var targetPos = currentTarget.transform.position + Vector3.up * 0.5f;
            this.transform.position = Vector3.Lerp(prevPosition, targetPos, delta);
            if(delta >= 1.0f)
            {
                delta = 0.0f;
                pos = 0.0f;
                this.transform.position = targetPos;
                this.position = currentTarget;
                if (path.Count > 0)
                    currentTarget = path.Pop();
                else
                    break;
            }
            yield return 0;
        }
        
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
