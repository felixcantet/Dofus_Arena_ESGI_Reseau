using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;

public class Character : MonoBehaviourPun, IPunObservable
{
    [Header("Character Informations")] 
    public string name = "IronMan";
    public Sprite characterIcon;
    [SerializeField] public Stats stats;

   // public Stats PlayerStats
   // {
       // get => this.stats;
   // }

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

    #region PUN Callbacks

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this.stats);
        }
        
        if(stream.IsReading)
        {
            // Network player, receive data
            this.stats = (Stats) stream.ReceiveNext();
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
        this.stats.currentLife -= damage;

        BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllViaServer,
            transform.position,
            BattleManager.Instance.lifeTxtColor.r,
            BattleManager.Instance.lifeTxtColor.g,
            BattleManager.Instance.lifeTxtColor.b,
            "-" + damage.ToString());
        
        Debug.LogError("Le personnage a perdu de la vie : " + damage + " actual pv : " + stats.currentLife);
        
        if (this.stats.currentLife <= 0)
        {
            currentState = CharacterState.DEAD;
            this.gameObject.SetActive(false);
            BattleManager.Instance.timeline.RemoveCharacterFromTimeline(this);
        }
    }

    [PunRPC]
    public void CastSpell(BaseSpell spell)
    {
        if (spell != null)
        {
            int count = spell.spellActions.Count - 1;
            
            foreach (var sa in spell.spellActions)
            {
                Vector3 offset = 0.5f * count * Vector3.up;
                string str = "<b>" + this.name + "</b>" + " : ";
                
                switch (sa.spellActionType)
                {
                    case SpellActionType.DAMAGE:
                        this.stats.currentLife += sa.value;
                        Debug.LogError("Le personnage a perdu de la vie : " + sa.value + " actual pv : " + stats.currentLife);
                        
                        
                        BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllViaServer,
                            transform.position + offset,
                            BattleManager.Instance.lifeTxtColor.r,
                            BattleManager.Instance.lifeTxtColor.g,
                            BattleManager.Instance.lifeTxtColor.b,
                            sa.value.ToString());
                        
                        str += sa.value.ToString() + " Vie.";
                        break;

                    case SpellActionType.MODIFY_RESSOURCES:
                        //Switch entre si ca boost pa ou pm
                        switch (sa.resource)
                        {
                            case ResourcesType.LIFE:
                                this.stats.currentLife = Mathf.Clamp(this.stats.currentLife + sa.value, 0,
                                    this.stats.MAX_LIFE);
                                
                                BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllViaServer,
                                    transform.position + offset,
                                    BattleManager.Instance.lifeTxtColor.r,
                                    BattleManager.Instance.lifeTxtColor.g,
                                    BattleManager.Instance.lifeTxtColor.b,
                                    sa.value.ToString());
                                
                                str += sa.value.ToString() + " Vie.";
                                break;

                            case ResourcesType.PA:
                                this.stats.PA = Mathf.Clamp(this.stats.PA + sa.value, 0, 30);
                                
                                BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllViaServer,
                                    transform.position + offset,
                                    BattleManager.Instance.paTxtColor.r,
                                    BattleManager.Instance.paTxtColor.g,
                                    BattleManager.Instance.paTxtColor.b,
                                    sa.value.ToString());
                                
                                str += sa.value.ToString() + " PA.";
                                break;

                            case ResourcesType.PM:
                                this.stats.PM = Mathf.Clamp(this.stats.PM + sa.value, 0, 30);
                                
                                BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllViaServer,
                                    transform.position + offset,
                                    BattleManager.Instance.pmTxtColor.r,
                                    BattleManager.Instance.pmTxtColor.g,
                                    BattleManager.Instance.pmTxtColor.b,
                                    sa.value.ToString());
                                
                                str += sa.value.ToString() + " PM.";
                                break;
                        }

                        break;
                }

                count--;
                
                if(photonView.IsMine)
                    BattleManager.Instance.photonView.RPC("AddBattleLog", RpcTarget.AllViaServer, str);
            }
        }

        if (this.stats.currentLife <= 0)
        {
            //Battle log de mort
            string str = "<b>" + this.name + "</b>" + " est mort.";
            if(photonView.IsMine)
                BattleManager.Instance.photonView.RPC("AddBattleLog", RpcTarget.AllViaServer, str);
            
            currentState = CharacterState.DEAD;

            position.used = false;
            
            this.gameObject.SetActive(false);
            
            BattleManager.Instance.timeline.RemoveCharacterFromTimeline(this);
            
            BattleManager.Instance.CheckVictoryCondition();
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

    #endregion

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
            if (!attackState)
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

        BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllViaServer, transform.position,
            BattleManager.Instance.pmTxtColor.r,
            BattleManager.Instance.pmTxtColor.g,
            BattleManager.Instance.pmTxtColor.b,
            "-" + path.Count);

        float moveSpeed = 0.5f;
        var currentTarget = path.Pop();
        float pos = 0.0f;


        while (stats.PM > 0)
        {
            pos += Time.deltaTime;
            var delta = pos / moveSpeed;
            var prevPosition = tmp.transform.position + Vector3.up * 0.5f;
            var targetPos = currentTarget.transform.position + Vector3.up * 0.5f;
            this.transform.position = Vector3.Lerp(prevPosition, targetPos, delta);
            if (delta >= 1.0f)
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


        var tileID = tmp.photonView.ViewID;
        PhotonNetwork.GetPhotonView(photonView.ViewID).RPC("SetCurrentTile", RpcTarget.AllBuffered, tileID);
        
        yield return new WaitForSeconds(0.2f);
        
        SearchMoveableTile(new Vector2Int(1, stats.PM), false);

        currentState = CharacterState.STATIC;
        
        yield break;
    }

    private Tile GetCurrentTile()
    {
        Ray ray = new Ray(this.transform.position + Vector3.up, Vector3.down);
        RaycastHit hit;
        var layer = MapManager.Instance.tileMask;
        if (Physics.Raycast(ray, out hit, layer))
        {
            return hit.transform.GetComponent<Tile>();
        }

        return null;
    }

    public void SetActiveCharacter(bool isActive = true)
    {
        //Enable effect
        activeEffect.SetActive(isActive);

        moveableTiles.Clear();
        
        //Search range
        if (isActive == true && photonView.IsMine)
            SearchMoveableTile(new Vector2Int(1, stats.PM), false);
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

    public void SetAttackProcess(Vector3 tilesPosition)
    {
        if (!photonView.IsMine)
            return;

        currentState = CharacterState.ATTACK_PROCESS;

        BaseSpell s = Spells[selectedSpell];

        //Instantiate Spell Effect
        BattleManager.Instance.photonView.RPC("DisplaySpellEffect", RpcTarget.AllViaServer, tilesPosition, s.spellEffectID);

        int count = s.spellCosts.Count - 1;
        foreach (var res in s.spellCosts)
        {
            Vector3 offset = 0.5f * count * Vector3.up;

            switch (res.resourcesType)
            {
                case ResourcesType.PA:
                    this.stats.PA = Mathf.Clamp(this.stats.PA + res.cost, 0, 999);
                    BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllViaServer,
                        transform.position + offset,
                        BattleManager.Instance.paTxtColor.r,
                        BattleManager.Instance.paTxtColor.g,
                        BattleManager.Instance.paTxtColor.b,
                        res.cost.ToString());
                    break;

                case ResourcesType.PM:
                    this.stats.PM = Mathf.Clamp(this.stats.PM + res.cost, 0, 999);
                    BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllViaServer,
                        transform.position + offset,
                        BattleManager.Instance.pmTxtColor.r,
                        BattleManager.Instance.pmTxtColor.g,
                        BattleManager.Instance.pmTxtColor.b,
                        res.cost.ToString());
                    break;

                case ResourcesType.LIFE:
                    stats.currentLife += res.cost;
                    BattleManager.Instance.photonView.RPC("DisplayTextEffect", RpcTarget.AllViaServer,
                        transform.position + offset,
                        BattleManager.Instance.lifeTxtColor.r,
                        BattleManager.Instance.lifeTxtColor.g,
                        BattleManager.Instance.lifeTxtColor.b,
                        res.cost.ToString());
                    break;
            }

            count--;
        }

        StartCoroutine(nameof(DelayAttack));

        //Check si le character n'est pas mort
        if (this.stats.currentLife <= 0)
            PhotonNetwork.GetPhotonView(photonView.ViewID).RPC("CastSpell", RpcTarget.AllBuffered, null);
    }

    IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(1.5f);

        currentState = CharacterState.STATIC;
        SearchMoveableTile(new Vector2Int(1, stats.PM), false);

        yield break;
    }

    public bool ResourcesAvailable()
    {
        BaseSpell s = Spells[selectedSpell];

        bool availaible = true;

        foreach (var res in s.spellCosts)
        {
            switch (res.resourcesType)
            {
                case ResourcesType.PA:
                    if (stats.PA < Mathf.Abs(res.cost))
                        availaible = false;
                    break;

                case ResourcesType.PM:
                    if (stats.PM < Mathf.Abs(res.cost))
                        availaible = false;
                    break;

                case ResourcesType.LIFE:
                    break;
            }
        }

        Debug.Log("Le sort est : " + availaible);

        return availaible;
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