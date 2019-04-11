using System.Collections;
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
}