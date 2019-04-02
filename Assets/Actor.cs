using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Actor : MonoBehaviour {

    public string actorName;

    public int maxHitPoints = 100;
    public int hitPoints = 100;

    public int initiative = 50;

    public enum ActionTarget
    {
        MeleeEnemy,
        AnyEnemy,
        AnyAlly,
        AllEnemy,
        AllAlly
    }
    public ActionTarget actionTarget;

    public int damage = 25;

    public enum ActionEffect
    {
        Normal,
        Disable,
        Heal
    }
    public ActionEffect actionEffect;

    public enum ActionSource
    {
        Weapon,
        Life,
        Death,
        Fire,
        Earth,
        Water,
        Air
    }
    public ActionSource actionEffectSource;

    public ActionSource[] immunities;

    public int percentChanceToHit = 75;

    public enum Position
    {
        left_rear_center,
        left_rear_bottom,
        left_rear_top,
        left_front_center,
        left_front_bottom,
        left_front_top,
        right_rear_center,
        right_rear_bottom,
        right_rear_top,
        right_front_center,
        right_front_bottom,
        right_front_top,
    }
    public Position boardPosition;

    private BoardData boardData;

    // Use this for initialization
    void Start () {
        boardData = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardData>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    BoardData.Side MySide { get { return (BoardData.Side)System.Enum.Parse(typeof(BoardData.Side), boardPosition.ToString().Split('_')[0]); } }
    BoardData.Rank MyRank { get { return (BoardData.Rank)System.Enum.Parse(typeof(BoardData.Rank), boardPosition.ToString().Split('_')[1]); } }
    BoardData.Line MyLine { get { return (BoardData.Line)System.Enum.Parse(typeof(BoardData.Line), boardPosition.ToString().Split('_')[2]); } }

    List<Actor> GetAvailableTargets()
    {
        List<Actor> result = new List<Actor>();
        BoardData.Side enemySide = MySide == BoardData.Side.left ? BoardData.Side.right : BoardData.Side.left;

        BoardData.Rank[] rankTargetOrder = new BoardData.Rank[] { BoardData.Rank.front, BoardData.Rank.rear };

        if (actionTarget == ActionTarget.MeleeEnemy)
        {
            // The weird one.
            // If I'm in the back row and anybody is in front of me, I cannot attack.
            if (MyRank == BoardData.Rank.rear)
            {
                if (boardData.GetActorByPosition(MySide, BoardData.Rank.front, BoardData.Line.top) != null 
                    || boardData.GetActorByPosition(MySide, BoardData.Rank.front, BoardData.Line.center) != null
                    || boardData.GetActorByPosition(MySide, BoardData.Rank.front, BoardData.Line.bottom) != null)
                {
                    return result;
                }
            }

            // Melee units can only attack units that are right in front of them, or one line away from
            // their current line. They can only attack the rear rank once the front rank is empty.
            for (int i = 0; i < rankTargetOrder.Length && result.Count == 0; i++)
            {
                BoardData.Rank targetRank = rankTargetOrder[i];

                // I can always hit the center...
                Actor candidate = boardData.GetActorByPosition(enemySide, targetRank, BoardData.Line.center);
                if (candidate != null) result.Add(candidate);

                // ... and my own line (applicable only if I'm not at the center).
                if (MyLine != BoardData.Line.center)
                {
                    candidate = boardData.GetActorByPosition(enemySide, targetRank, MyLine);
                    if (candidate != null) result.Add(candidate);
                }


                // I can only hit across the field if there's nobody in the way (applies to bottom and top lines only).
                if (MyLine == BoardData.Line.center || (MyLine == BoardData.Line.top && result.Count == 0))
                {
                    candidate = boardData.GetActorByPosition(enemySide, targetRank, BoardData.Line.bottom);
                    if (candidate != null) result.Add(candidate);
                }
                if (MyLine == BoardData.Line.center || (MyLine == BoardData.Line.bottom && result.Count == 0))
                {
                    candidate = boardData.GetActorByPosition(enemySide, targetRank, BoardData.Line.top);
                    if (candidate != null) result.Add(candidate);
                }
            }

        }
        else
        {
            BoardData.Line[] lines = new BoardData.Line[] { BoardData.Line.top, BoardData.Line.bottom, BoardData.Line.center };
            BoardData.Side targetSide = (actionTarget.ToString().EndsWith("Enemy")) ? enemySide : MySide;
            for (int l = 0; l < lines.Length; l++)
                for (int r = 0; r < rankTargetOrder.Length; r++)
                {
                    Actor candidate = boardData.GetActorByPosition(targetSide, rankTargetOrder[r], lines[l]);
                    if (candidate != null) result.Add(candidate);
                }

        }
        return result;
    }
}
