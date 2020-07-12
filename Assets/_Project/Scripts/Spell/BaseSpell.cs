using System.Collections;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Spell/New Spell", order = 0)]
public class BaseSpell : ScriptableObject
{
    [Header("Spell information")]
    public string spellName; 
    [Multiline] public string spellDescription;
    
    [Header("Spell Parameter")]
    public SpellType spellType;
    public Vector2Int spellRange;
    public List<ResourcesCost> spellCosts;
    public List<SpellAction> spellActions;

    [Header("Spell Feedback")] 
    public int spellEffectID;

    #region Serialize Custom Photon

    public static byte[] Serialize(object o)
    {
        var team = (BaseSpell)o;
        var bytes = SerializationUtility.SerializeValue(team, DataFormat.JSON);
        return bytes;
    }

    public static BaseSpell Deserialize(byte[] bytes)
    {
        var result = new BaseSpell();
        result = SerializationUtility.DeserializeValue<BaseSpell>(bytes, DataFormat.JSON);

        return result;
    }
    
    public static void Register()
    {
        ExitGames.Client.Photon.PhotonPeer.RegisterType(typeof(BaseSpell), (byte)'S', Serialize, Deserialize);
    }
    
    #endregion
}

// ----------------------------------------

public enum SpellType
{
    DAMAGE,
    HEAL,
    BUFF
}

// ----------------------------------------

[System.Serializable]
public struct SpellAction
{
    public SpellActionType spellActionType;
    public int value;
    public ResourcesType resource;
}

public enum SpellActionType
{
    DAMAGE,
    MODIFY_RESSOURCES,
}

// ----------------------------------------

[System.Serializable]
public struct ResourcesCost
{
    public ResourcesType resourcesType;
    public int cost;
}

public enum ResourcesType
{
    PA,
    PM,
    LIFE
}