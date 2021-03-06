﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DerivedStatList : ScriptableObject
{
    public DerivedStat[] list;
    public int Length
    {
        get
        {
            return list == null ? 0 : list.Length;
        }
    }
}

[System.Serializable]
public class DerivedStat
{
    public string statName = string.Empty, expression = string.Empty;

    public bool HasCircularReference(DerivedStatList derivedStats)
    {
        // Assumption: All stats have different names. (Enforceable?)
        // (Append (1) (2) etc.? )

        // TODO: Recursion
        // Find derivedStat references within this expression
        // For each, drill down into their derived stats; if we ever find
        // the statName of this item, return true up the recursion.
        //
        // Is it necessary to check for infinite recursion, since
        // by definition, if there is no circular reference,
        // eventually we'll hit the bottom of the tree on all branches?


        throw new System.NotImplementedException();

    }


    public bool TryEvaluate(Actor actor, DerivedStatList derivedStats, out int outcome)
    {
        Dictionary<string, int> statSubs = new Dictionary<string, int>();

        statSubs.Add("MAXHP", actor.maxHitPoints);
        statSubs.Add("HP", actor.hitPoints);
        statSubs.Add("DAMAGE", actor.damage);

        int numTargets = 1;
        if (actor.actionTarget.ToString().StartsWith("All")) numTargets = actor.GetAvailableTargets().Count;
        statSubs.Add("NUMTARGETS", numTargets);

        string workbench = expression.ToUpper();
        foreach (string k in statSubs.Keys) workbench = workbench.Replace(k, statSubs[k].ToString());

        // TODO: Watch out for circular definitions
        if (derivedStats != null)
        {
            for (int i = 0; i < derivedStats.Length; i++)
            {
                if (derivedStats.list[i].statName != statName)
                {
                    int derivedValue;
                    if (derivedStats.list[i].TryEvaluate(actor, null, out derivedValue))
                    {
                        workbench = workbench.Replace(derivedStats.list[i].statName.ToUpper(), derivedValue.ToString());
                    }
                }
            }
        }

        if (!UnityEditor.ExpressionEvaluator.Evaluate<int>(workbench, out outcome))
        {
            UnityEngine.MonoBehaviour.print(workbench);
            return false;
        }
        else
        {
            return true;
        }
    }
}