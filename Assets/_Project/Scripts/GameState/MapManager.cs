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
                if(item.used)
                    continue;
                
                if (item.tileType != TileType.NORMAL)
                    continue;
                
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
            if (safety == 20)
                break;

            path.Push(currentTile);
//            Debug.Log(tileFlags[currentTile].parent);
            currentTile = tileFlags[currentTile].parent;

        }
        return path;
    }

    public static List<Tile> GetTilesInRange(Tile startPos, Vector2Int range, bool rangeForAttack = false)
    {
        List<Tile> tiles = new List<Tile>();
        tiles.Add(startPos);

        foreach (var tile in startPos.neighbours)
        {
            if (tiles.Contains(tile))
                continue;
            
            if(tile.used)
                continue;
            
            tiles.Add(tile);
        }
        
        for (int i = 0; i < tiles.Count; i++)
        {
            foreach (var neig in tiles[i].neighbours)
            {
                if (tiles.Contains(neig))
                    continue;
                
                if(!rangeForAttack)
                    if(neig.used)
                        continue;
                
                if (neig.tileType != TileType.NORMAL)
                        continue;
                
                int calX = neig.position.x > startPos.position.x
                    ? neig.position.x - startPos.position.x
                    : startPos.position.x - neig.position.x;
                
                int calY = neig.position.y > startPos.position.y
                    ? neig.position.y - startPos.position.y
                    : startPos.position.y - neig.position.y;

                int dist = calX + calY;
                
                //Debug.Log("la tile se trouve a : " + dist + " pour X = " + calX + " et Y = " + calY);
                
                if (dist <= range.y)
                {
                    //Debug.Log("j'ajoute la tile !");
                    tiles.Add(neig);
                }
            }
        }
        
        List<Tile> finalTiles = new List<Tile>();
        
        foreach (var tile in tiles)
        {
            int calX = tile.position.x > startPos.position.x
                ? tile.position.x - startPos.position.x
                : startPos.position.x - tile.position.x;
                
            int calY = tile.position.y > startPos.position.y
                ? tile.position.y - startPos.position.y
                : startPos.position.y - tile.position.y;
            
            int dist = calX + calY;
            
            if (dist >= range.x)
            {
                //Debug.Log("j'ajoute la tile !");
                finalTiles.Add(tile);
            }
        }

        tiles = finalTiles;
        
        //Check ligne de vue
        if (rangeForAttack)
        {
            
        }
        else
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                var path = GetPath(startPos, tiles[i]);
                if (path.Count >= (range.x + range.y))
                {
                    Debug.LogWarning("Count : " + path.Count + " from : " + startPos + " to : " + tiles[i]);
                    finalTiles.Remove(tiles[i]);
                }

            }
        }

        return finalTiles;
    }
}
