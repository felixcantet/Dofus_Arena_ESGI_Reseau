using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;
public class Character : MonoBehaviourPun, IPunObservable
{
    [Header("Character Informations")]
    public string name = "IronMan";
    [SerializeField] Stats stats;
    public Stats PlayerStats
    {
        get => this.stats;
        set => this.stats = value;
    }

    [Header("Character Spells")] 
    public List<BaseSpell> Spells = new List<BaseSpell>();
    public int selectedSpell = 0;
    
    [Header("Character State")]
    public CharacterState currentState = CharacterState.STATIC;
    
    [Header("Character Tiles Info")]
    public Tile position;
    public List<Tile> moveableTiles = new List<Tile>();

    [Header("Feedback")]
    public GameObject activeEffect;
    
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
        //this.name = name;
    }

    [PunRPC]
    public void Damage(int damage)
    {
        this.PlayerStats.currentLife -= damage;
        
        if (this.PlayerStats.currentLife <= 0)
        {
            currentState = CharacterState.DEAD;
            this.gameObject.SetActive(false);
            BattleManager.Instance.timeline.RemoveCharacterFromTimeline(this);
        }
    }

    [PunRPC]
    public void CastSpell(BaseSpell spell)
    {
        foreach (var sa in spell.spellActions)
        {
            switch (sa.spellActionType)
            {
                case SpellActionType.DAMAGE:
                    this.PlayerStats.currentLife += sa.value;
                    break;
                        
                case SpellActionType.MODIFY_RESSOURCES:
                    //Switch entre si ca boost pa ou pm
                    switch (sa.resource)
                    {
                        case ResourcesType.LIFE:
                            this.PlayerStats.currentLife += sa.value;
                            break;
                        
                        case ResourcesType.PA:
                            this.PlayerStats.PA = Mathf.Clamp(this.PlayerStats.PA + sa.value, 0, 999);
                            break;
                        
                        case ResourcesType.PM:
                            this.PlayerStats.PM = Mathf.Clamp(this.PlayerStats.PM + sa.value, 0, 999);
                            break;
                    }
                    break;
            }
        }
        
        if (this.PlayerStats.currentLife <= 0)
        {
            currentState = CharacterState.DEAD;
            this.gameObject.SetActive(false);
            BattleManager.Instance.timeline.RemoveCharacterFromTimeline(this);
        }
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

    public void SearchMoveableTile(Vector2Int range, bool attackState = false)
    {
        if (!photonView.IsMine)
            return;

        if (stats.PM <= 0 && !attackState)
        {
            moveableTiles = new List<Tile>();
            return;
        }
        
        moveableTiles = MapManager.GetTilesInRange(position, range, attackState);

        foreach (var i in moveableTiles)
        {
            if(!attackState)
                i.SetColor(Color.yellow);
            else
                i.SetColor(new Color(1, 0.6f, 1, 1));
        }
    }
    
    public IEnumerator MoveToTile(Stack<Tile> path)
    {
        currentState = CharacterState.MOVE;
        position.used = false;

        Tile tmp = this.position;
        
        BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllBuffered, transform.position,
            0.0f,
            0.78f,
            0.25f,
            "-" + path.Count + " PM");
        
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

        currentState = CharacterState.STATIC;

        var tileID = tmp.photonView.ViewID;
        PhotonNetwork.GetPhotonView(photonView.ViewID).RPC("SetCurrentTile", RpcTarget.AllBuffered, tileID);
        
        SearchMoveableTile(new Vector2Int(1, stats.PM));
        
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

    public void SetActiveCharacter(bool isActive = true)
    {
        //Enable effect
        activeEffect.SetActive(isActive);
        
        //Search range
        if(isActive)
            SearchMoveableTile(new Vector2Int(1, stats.PM));
    }
    
    public void SwitchToAttackStateToStaticState(int spellId)
    {
        currentState = currentState == CharacterState.ATTACK_MODE ? CharacterState.STATIC : CharacterState.ATTACK_MODE;
        
        selectedSpell = spellId;
        
        Debug.Log("Spell " + spellId.ToString() + " Chosen --- Character");
        
        switch (currentState)
        {
            case CharacterState.ATTACK_MODE:
                SearchMoveableTile(Spells[selectedSpell].spellRange, true);
                break;
            
            case CharacterState.STATIC:
                SearchMoveableTile(new Vector2Int(1, stats.PM));
                break;
        }
        
    }

    public void SetAttackProcess()
    {
        if (!photonView.IsMine)
            return;
            
        currentState = CharacterState.ATTACK_PROCESS;
        
        BaseSpell s = Spells[selectedSpell];


        int count = s.spellCosts.Count - 1;
        
        foreach (var res in s.spellCosts)
        {
            Vector3 offset = 0.5f * count * Vector3.up;
            
            switch (res.resourcesType)
            {
                case ResourcesType.PA:
                    this.PlayerStats.PA = Mathf.Clamp(this.PlayerStats.PA + res.cost, 0, 999);
                    BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllBuffered, transform.position + offset,
                        0.0f,
                        0.25f,
                        0.78f,
                        res.cost.ToString() + " PA");
                    break;
                        
                case ResourcesType.PM:
                    this.PlayerStats.PM = Mathf.Clamp(this.PlayerStats.PM + res.cost, 0, 999);
                    BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllBuffered, transform.position + offset,
                        0.0f,
                        0.78f,
                        0.25f,
                        res.cost.ToString() + " PM");
                    break;
               
                case ResourcesType.LIFE:
                    stats.currentLife += res.cost;
                    BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllBuffered, transform.position + offset,
                        BattleManager.Instance.textEffectPrefab.displayColor.r,
                        BattleManager.Instance.textEffectPrefab.displayColor.g,
                        BattleManager.Instance.textEffectPrefab.displayColor.b,
                        res.cost.ToString());
                    break;
            }

            count--;
        }
        
        StartCoroutine(nameof(DelayAttack));
        
        //Fonction de check si le character n'est pas mort
    }

    IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(1.5f);

        currentState = CharacterState.STATIC;
        SearchMoveableTile(new Vector2Int(1, stats.PM));
        
        yield break;
    }

    public bool ResourcesAvailable()
    {
        BaseSpell s = Spells[selectedSpell];

        foreach (var res in s.spellCosts)
        {
            switch (res.resourcesType)
            {
                case ResourcesType.PA:
                    if (stats.PA < res.cost)
                        return false;
                    break;
                
                case ResourcesType.PM:
                    if (stats.PM < res.cost)
                        return false;
                    break;
                
                case ResourcesType.LIFE:
                    continue;
            }
        }
        
        return true;
    }
}

public enum CharacterState
{
    STATIC,
    MOVE,
    ATTACK_MODE,
    ATTACK_PROCESS,
    DEAD
}