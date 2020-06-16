using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class MapManager : NetworkSingleton<MapManager>, IPunObservable
{
    public List<Tile> map;

    public LayerMask tileMask;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //throw new System.NotImplementedException();
    }
    [PunRPC]
    public void AddTile(int tileViewID)
    {
        var view = PhotonNetwork.GetPhotonView(tileViewID);
        var tile = view.GetComponent<Tile>();
        map.Add(tile);
    }
    private void Awake()
    {
        this.map = new List<Tile>();
    }

    IEnumerator Start()
    {
        yield return 0;
        //yield return new WaitForSeconds(0.1f);
        //var path = MapManager.GetPath(map[0], map[1]);

        //while (path.Count > 0)
        //{
        //    var tile = path.Pop();
        //    tile.SetColor(Color.red);
        //}
    }
    // Start is called before the first frame update
    class TileFlags
    {
        public bool discovered;
        public Tile parent;

        public TileFlags(bool _discovered, Tile _parent)
        {
            this.discovered = _discovered;
            this.parent = _parent;
        }

        public void SetFlags(bool discovered, Tile parent)
        {
            this.discovered = discovered;
            this.parent = parent;
        }
        public void SetFlags(bool discovered)
        {
            this.discovered = discovered;
        }

        public void SetFlags(Tile parent)
        {
            this.parent = parent;
        }
    }
    public static Stack<Tile> GetPath(Tile from, Tile to)
    {
        Queue<Tile> search = new Queue<Tile>();
        Dictionary<Tile, TileFlags> tileFlags = new Dictionary<Tile, TileFlags>();

        foreach (var item in MapManager.Instance.map)
        {
            tileFlags.Add(item, new TileFlags(false, null));
        }
        tileFlags[from].SetFlags(true);
        search.Enqueue(from);

        while (search.Count > 0)
        {
            var tile = search.Dequeue();
            if (tile == to)
            {
                break;
            }

            foreach (var item in tile.neighbours)
            {
                if (!tileFlags[item].discovered)
                {
                    tileFlags[item].SetFlags(true, tile);
                    search.Enqueue(item);
                }
            }
        }

        var path = new Stack<Tile>();
        var currentTile = to;
        //path.Push(to);
        int safety = 0;
        while (currentTile != from)
        {
            safety++;
            if (safety == 250)
                break;

            path.Push(currentTile);
            Debug.Log(tileFlags[currentTile].parent);
            currentTile = tileFlags[currentTile].parent;

        }
        return path;

    }
}
