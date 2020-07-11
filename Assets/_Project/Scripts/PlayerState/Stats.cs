using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stats
{
    [HideInInspector] public int PM = 4;
    [HideInInspector] public int PA = 6;

    [HideInInspector] public int currentLife;
    
    [SerializeField] private int maxLife = 200;
    [SerializeField] private int defaultPM = 4;
    [SerializeField] private int defaultPA = 6;

    [SerializeField] private Vector2Int attackRange = new Vector2Int(1, 2);

    [SerializeField] private int damage = 50;
    
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

    public Vector2Int ATTAQUE_RANGE
    {
        get { return attackRange; }
    }

    public int DAMAGE
    {
        get { return damage; }
    }
}
