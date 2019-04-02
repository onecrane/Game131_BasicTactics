using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Actor : MonoBehaviour {

    #region Enums

    public enum TargetSelectionRule
    {
        AnyAvailable,
        HighestHealth,
        StrongestAttack
    }
    public TargetSelectionRule targetSelectionRule;

    [Tooltip("Don't change this property; it should be read-only.")]
    public Actor currentTarget;

    public enum ActionTarget
    {
        MeleeEnemy,
        AnyEnemy,
        AnyAlly,
        AllEnemy,
        AllAlly
    }

    public enum ActionEffect
    {
        Normal,
        Disable,
        Heal
    }

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

    #endregion

    #region Configurable member variables

    public string actorName;

    public int maxHitPoints = 100;
    public int hitPoints = 100;

    public int initiative = 50;

    public ActionTarget actionTarget;

    public int damage = 25;

    public ActionEffect actionEffect;

    public ActionSource actionEffectSource;

    public ActionSource[] immunities;

    public int percentChanceToHit = 75;

    public Position boardPosition;

    // TODO: Provide some means of configuring how the target is selected

    #endregion

    #region Private member variables

    private BoardData boardData;

    #endregion

    #region Unity events (do not change)

    void Start()
    {
        boardData = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardData>();
    }

    void Update()
    {
        currentTarget = RefreshTargetSelection(GetAvailableTargets());
    }

    #endregion

    /// <summary>
    /// Modify this function however you need to. The main challenge of this project
    /// is to create a form-based means of authoring the AI for each actor based on
    /// the targets they can currently reach. This function receives a list of 
    /// available targets, and should choose an appropriate target based on how the
    /// designer has configured the AI.
    /// 
    /// How you implement this is up to you. Refer to the project requirements for
    /// what it has to be able to do.
    /// </summary>
    Actor RefreshTargetSelection(List<Actor> availableTargets)
    {
        if (availableTargets.Count == 0)
        {
            return null;
        }
        switch (targetSelectionRule)
        {
            case TargetSelectionRule.AnyAvailable:
                return availableTargets[Random.Range(0, availableTargets.Count)];
            case TargetSelectionRule.HighestHealth:
                int highestHealth = 0;
                for (int i = 0; i < availableTargets.Count; i++)
                    if (availableTargets[i].hitPoints > highestHealth)
                        highestHealth = availableTargets[i].hitPoints;
                List<int> highestHealthIndexes = new List<int>();
                for (int i = 0; i < availableTargets.Count; i++)
                    if (availableTargets[i].hitPoints == highestHealth)
                        highestHealthIndexes.Add(i);
                return availableTargets[highestHealthIndexes[Random.Range(0, highestHealthIndexes.Count)]];
            case TargetSelectionRule.StrongestAttack:
                int highestAttack = 0;
                for (int i = 0; i < availableTargets.Count; i++)
                    if (availableTargets[i].damage > highestAttack)
                        highestAttack = availableTargets[i].damage;
                List<int> highestAttackIndexes = new List<int>();
                for (int i = 0; i < availableTargets.Count; i++)
                    if (availableTargets[i].damage == highestAttack)
                        highestAttackIndexes.Add(i);
                return availableTargets[highestAttackIndexes[Random.Range(0, highestAttackIndexes.Count)]];
        }
        return availableTargets[Random.Range(0, availableTargets.Count)];
    }

    #region Target selection core (do not change)

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
                if (candidate != null)
                {
                    result.Add(candidate);
                }
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

    #endregion


}
