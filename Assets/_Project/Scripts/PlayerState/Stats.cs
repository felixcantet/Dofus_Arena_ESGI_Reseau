using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stats
{
    public int PM = 4;
    public int PA = 6;

    public int currentLife;

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
}
