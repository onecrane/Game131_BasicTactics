using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
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

	// Use this for initialization
	void Start () {
        InitBoardTileObjects();
    }

    private Actor[] actors;
    private Dictionary<string, BoardLocationUIControl> boardTiles = new Dictionary<string, BoardLocationUIControl>();

    private void InitBoardTileObjects()
    {
        GameObject[] boardTileObjects = GameObject.FindGameObjectsWithTag("BoardTile");
        for (int i = 0; i < boardTileObjects.Length; i++)
        {
            //print("Added " + boardTileObjects[i].name);
            boardTiles.Add(boardTileObjects[i].name, boardTileObjects[i].GetComponent<BoardLocationUIControl>());
        }
    }

    public void RefreshActors()
    {
        if (boardTiles.Count == 0) InitBoardTileObjects();
        foreach (string boardTileName in boardTiles.Keys) boardTiles[boardTileName].myActor = null;

        GameObject[] actorObjects = GameObject.FindGameObjectsWithTag("Actor");
        actors = new Actor[actorObjects.Length];
        for (int i = 0; i < actorObjects.Length; i++)
        {
            actors[i] = actorObjects[i].GetComponent<Actor>();
            try
            {
                boardTiles[actors[i].boardPosition.ToString().ToLower()].myActor = actors[i];
            }
            catch
            {
                // ... Ew.
                print("Error setting tile " + actors[i].boardPosition.ToString().ToLower());
            }
        }


    }

    // Update is called once per frame
    void Update () {
        RefreshActors();
    }
}
