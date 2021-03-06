﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timeline
{
    public Timeline()
    {
        timeline = new Queue<Character>();
    }
    public Timeline(List<Team> teams)
    {
        timeline = new Queue<Character>();

        for (int i = 0; i < teams[0].characters.Count; i++)
        {
            //teams[0].characters[i].GetComponent<Renderer>().material.color = Color.white;
            //teams[1].characters[i].GetComponent<Renderer>().material.color = Color.white;
            timeline.Enqueue(teams[0].characters[i]);
            timeline.Enqueue(teams[1].characters[i]);

        }
        //ActiveCharacter.GetComponent<Renderer>().material.color = Color.red;

    }
    public Queue<Character> timeline;

    public Character ActiveCharacter
    {
        get => timeline.Peek();
    }

    public void SetNextTurn()
    {
        var oldChara = ActiveCharacter;
        //oldChara.GetComponent<Renderer>().material.color = Color.white;
        this.timeline.Enqueue(this.timeline.Dequeue());
        //ActiveCharacter.GetComponent<Renderer>().material.color = Color.red;
        Debug.Log(ActiveCharacter.name);
    }

}
