using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;
[System.Serializable]
public class Team
{
    public List<int> charactersID;
    public List<Character> characters;

    public static byte[] Serialize(object o)
    {
        var team = (Team)o;
        var bytes = SerializationUtility.SerializeValue(team, DataFormat.JSON);
        return bytes;
    }

    public static Team Deserialize(byte[] bytes)
    {
        var result = new Team();
        result = SerializationUtility.DeserializeValue<Team>(bytes, DataFormat.JSON);

        return result;
    }
    public static void Register()
    {
        ExitGames.Client.Photon.PhotonPeer.RegisterType(typeof(Team), (byte)'C', Serialize, Deserialize);
    }

}
