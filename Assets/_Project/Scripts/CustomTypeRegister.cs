using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTypeRegister : MonoBehaviour
{
    private void Awake()
    {
        Team.Register();
        BaseSpell.Register();
    }
}
