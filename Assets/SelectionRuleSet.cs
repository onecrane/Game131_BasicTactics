using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SelectionRuleSet : ScriptableObject
{
    // A set of selection rules is just that; 


}

[System.Serializable]
public class SelectionRule
{
    // A selection rule can be a lot of things, but as long as it's serializable, we're free to wreck it.

    // Killable: target.hp < my.damage
    // All string-based here
    public string expression = string.Empty;
    public int depth = 0;

    public List<Actor> Apply(Actor initiator, List<Actor> availableTargets, DerivedStatList derivedStats)
    {
        // Parse the expression.
        string operation = null;

        if (expression.Contains(">=")) operation = ">=";
        else if (expression.Contains("<=")) operation = "<=";
        else if (expression.Contains("!=")) operation = "!=";
        else if (expression.Contains(">")) operation = ">";
        else if (expression.Contains("<")) operation = "<";
        else if (expression.Contains("=")) operation = "=";

        if (operation == null) throw new System.Exception("Invalid operator in SelectionRule expression [" + expression + "]. Only <, >, <=, >=, =, != are allowed.");

        string[] components = expression.Split(new string[] { operation }, System.StringSplitOptions.RemoveEmptyEntries);

        // Require a simple expression; each side is a property of either the initiator or a target..? What about things like targets.max?

        // Maybe start with the left side? Oh, or, evaluate for each either way. Derp.
        List<Actor> newAvailableTargets = new List<Actor>();
        for (int i = 0; i < availableTargets.Count; i++)
        {
            Actor candidateTarget = availableTargets[i];
            string[] left = components[0].ToLower().Split('.');
            string[] right = components[1].ToLower().Split('.');

            int leftValue, rightValue;
            // Evaluate left
            if (left[0] == "my")
            {
                // Evaluate on initiator
                initiator.TryEvaluate(left[1], derivedStats, out leftValue);
            }
            if (left[0] == "target")
            {
                // Evaluate on candidateTarget
                candidateTarget.TryEvaluate(left[1], derivedStats, out leftValue);
            }
            if (left[0] == "targets")
            {
                // max or min here
                int[] allValues = new int[availableTargets.Count];
                for (int j = 0; j < allValues.Length; j++)
                {
                    availableTargets[j].TryEvaluate(left[2], derivedStats, out allValues[j]);
                }
                int outcome = allValues[0];
                for (int j = 1; j < allValues.Length; j++)
                {
                    if (left[1] == "max" && allValues[j] > outcome) outcome = allValues[j];
                    if (left[1] == "min" && allValues[i] < outcome) outcome = allValues[j];
                }
                leftValue = outcome;
            }

            // Evaluate right
            if (right[0] == "my")
            {
                // Evaluate on initiator
                initiator.TryEvaluate(right[1], derivedStats, out rightValue);
            }
            if (right[0] == "target")
            {
                // Evaluate on candidateTarget
                candidateTarget.TryEvaluate(right[1], derivedStats, out rightValue);
            }
            if (right[0] == "targets")
            {
                // max or min here
                int[] allValues = new int[availableTargets.Count];
                for (int j = 0; j < allValues.Length; j++)
                {
                    availableTargets[j].TryEvaluate(right[2], derivedStats, out allValues[j]);
                }
                int outcome = allValues[0];
                for (int j = 1; j < allValues.Length; j++)
                {
                    if (right[1] == "max" && allValues[j] > outcome) outcome = allValues[j];
                    if (right[1] == "min" && allValues[i] < outcome) outcome = allValues[j];
                }
                rightValue = outcome;
            }


        }

    }

}
