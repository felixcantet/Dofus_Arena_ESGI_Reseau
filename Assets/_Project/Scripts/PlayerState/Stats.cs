using System.Collections;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

[System.Serializable]
public class Stats
{
    [Header("Current Parameter")]
    public int PM = 4;
    public int PA = 6;

    public int currentLife;
    
    [Header("Default Parameter")]
    [SerializeField] private int maxLife = 200;
    [SerializeField] private int defaultPM = 4;
    [SerializeField] private int defaultPA = 6;
    
    
    public int MAX_LIFE
    {
        get { return maxLife; }
    }
    
    public int DEFAULT_PM
    {
        get { return defaultPM; }
    }
    
    public int DEFAULT_PA
    {
        get { return defaultPA; }
    }
    
    #region Serialize Custom Photon

    public static byte[] Serialize(object o)
    {
        var team = (Stats)o;
        var bytes = SerializationUtility.SerializeValue(team, DataFormat.JSON);
        return bytes;
    }

    public static Stats Deserialize(byte[] bytes)
    {
        var result = new Stats();
        result = SerializationUtility.DeserializeValue<Stats>(bytes, DataFormat.JSON);

        return result;
    }
    
    public static void Register()
    {
        ExitGames.Client.Photon.PhotonPeer.RegisterType(typeof(Stats), (byte)'A', Serialize, Deserialize);
    }
    
    #endregion
}
