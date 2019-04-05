using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(Actor))]
public class ActorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        Actor myActor = target as Actor;

        Actor.ActionSource[] sourceValues = Enum.GetValues(typeof(Actor.ActionSource)) as Actor.ActionSource[];
        string[] sourceNames = Enum.GetNames(typeof(Actor.ActionSource));
        for (int i = 0; i < sourceNames.Length; i++) sourceNames[i] += '\t';

        SelectionList<Actor.ActionSource> sources = new SelectionList<Actor.ActionSource>(sourceValues, sourceNames);
        myActor.actionEffectSource = sources.RadioList("Action Source", myActor.actionEffectSource, 3);
        myActor.immunities = sources.CheckboxList("Immunities", myActor.immunities, 3);

    }

}

// Repo URL: https://github.com/onecrane/Game131_BasicTactics

class SelectionList<T> where T : IComparable
{
    int f = 9;
    T[] _values;
    string[] _labels;
    T _selectedValue;


    public T[] CheckboxList(string label, T[] initialSelections, int itemsPerRow)
    {
        List<T> selectedValues = new List<T>();
        List<int> initialSelectedIndexes = new List<int>();
        for (int i = 0; i < _values.Length; i++)
        {
            for (int j = 0; j < initialSelections.Length; j++)
            {
                if (_values[i].CompareTo(initialSelections[j]) == 0) initialSelectedIndexes.Add(i);
            }
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.MaxWidth(100));

        EditorGUILayout.BeginVertical();
        for (int r = 0; r < _values.Length; r += itemsPerRow)
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = r; i < r + itemsPerRow && i < _values.Length; i++)
            {
                if (GUILayout.Toggle(initialSelectedIndexes.Contains(i), _labels[i]))
                {
                    selectedValues.Add(_values[i]);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        return selectedValues.ToArray();

    }

    public T RadioList(string label, T initialSelection, int itemsPerRow)
    {
        T originalSelectedValue = _selectedValue;
        _selectedValue = initialSelection;
        bool anyChecked = false;

        // TWo controls rendered: The label, and a vertical section
        EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(label, GUILayout.MaxWidth(100));

            EditorGUILayout.BeginVertical();
            for (int r = 0; r < _values.Length; r += itemsPerRow)
            {
                EditorGUILayout.BeginHorizontal();
                for (int i = r; i < r + itemsPerRow && i < _values.Length; i++)
                {
                    if (_values[i].CompareTo(initialSelection) == 0) originalSelectedValue = initialSelection;
                    if (GUILayout.Toggle(_values[i].CompareTo(_selectedValue) == 0, _labels[i]))
                    {
                        _selectedValue = _values[i];
                        anyChecked = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        if (!anyChecked) _selectedValue = originalSelectedValue;
        return _selectedValue;
    }

    public SelectionList(T[] values, string[] labels)
    {
        _values = new T[values.Length];
        _labels = new string[labels.Length < values.Length ? values.Length : labels.Length];
        for (int i = 0; i < _values.Length; i++) _values[i] = values[i];
        for (int i = 0; i < _labels.Length; i++) _labels[i] = (i < labels.Length) ? labels[i] : values[i].ToString();
        _selectedValue = _values[0];
    }
}