using System.Collections;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

public class CustomTypeRegister : MonoBehaviour
{
    private void Awake()
    {
        Team.Register();
        BaseSpell.Register();
        RegisterGameObject.Register();
    }
}

public class RegisterGameObject
{
    #region Serialize Custom Photon

    public static byte[] Serialize(object o)
    {
        var team = (GameObject)o;
        var bytes = SerializationUtility.SerializeValue(team, DataFormat.JSON);
        return bytes;
    }

    public static GameObject Deserialize(byte[] bytes)
    {
        var result = new GameObject();
        result = SerializationUtility.DeserializeValue<GameObject>(bytes, DataFormat.JSON);

        return result;
    }
    
    public static void Register()
    {
        ExitGames.Client.Photon.PhotonPeer.RegisterType(typeof(GameObject), (byte)'G', Serialize, Deserialize);
    }
    
    #endregion
}