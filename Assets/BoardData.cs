using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardData : MonoBehaviour {

    public enum Side
    {
        left,
        right
    }

    public enum Rank
    {
        front,
        rear
    }

    public enum Line
    {
        bottom,
        center,
        top
    }

    // Note: Only returns actors that are alive
    public Actor GetActorByPosition(Side side, Rank rank, Line line)
    {
        string boardPosition = string.Format("{0}_{1}_{2}", side, rank, line);
        for (int i = 0; i < actors.Length; i++)
            if (actors[i].boardPosition.ToString() == boardPosition)
                return actors[i].hitPoints > 0 ? actors[i] : null;
        return null;
    }

    public Actor[] actors;

	// Use this for initialization
	void Start () {
        GameObject[] actorObjects = GameObject.FindGameObjectsWithTag("Actor");
        actors = new Actor[actorObjects.Length];
        for (int i = 0; i < actorObjects.Length; i++)
            actors[i] = actorObjects[i].GetComponent<Actor>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
