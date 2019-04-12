using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


[CustomEditor(typeof(Actor))]
public class ActorEditor : Editor
{
    private static GUIStyle errorBoxStyle = null;

    private bool showDerivedProperties = false;

    private static void InitializeStyles()
    {
        errorBoxStyle = new GUIStyle(EditorStyles.textField);

        errorBoxStyle.normal.background = Resources.Load<Texture2D>("Textures/txErrorBackground");

    }

    private DerivedStatList derivedStats;
    public override void OnInspectorGUI()
    {
        if (errorBoxStyle == null) InitializeStyles();

        showDerivedProperties = EditorGUILayout.Foldout(showDerivedProperties, new GUIContent("Derived Properties", "Properties based on static unit stats."));
        if (showDerivedProperties)
        {
            derivedStats = AssetDatabase.LoadAssetAtPath("Assets/DerivedProperties.asset", typeof(DerivedStatList)) as DerivedStatList;
            if (derivedStats == null)
            {
                UnityEngine.MonoBehaviour.print("Nope, not there");
                derivedStats = ScriptableObject.CreateInstance<DerivedStatList>();
                AssetDatabase.CreateAsset(derivedStats, "Assets/DerivedProperties.asset");
                AssetDatabase.SaveAssets();
            }

            int nameFieldWidth = 80;
            for (int i = 0; i < derivedStats.Length; i++)
            {
                int statNameWidth = (int)EditorStyles.textField.CalcSize(new GUIContent(derivedStats.list[i].statName + " ")).x;
                if (statNameWidth > nameFieldWidth) nameFieldWidth = statNameWidth;
            }

            bool derivedPropEquationChanged = false;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(nameFieldWidth));
            EditorGUILayout.LabelField("Expression");
            EditorGUILayout.LabelField("Current Value", GUILayout.MaxWidth(90));
            EditorGUILayout.EndHorizontal();
            


            for (int i = 0; i < (derivedStats != null ? derivedStats.Length : 0); i++)
            {
                EditorGUILayout.BeginHorizontal();

                // TODO: Watch out for repeats
                derivedStats.list[i].statName = EditorGUILayout.TextField(derivedStats.list[i].statName, GUILayout.Width(nameFieldWidth));

                int derivedValue = 0;
                string derivedEquationErrorMessage = string.Empty;

                // How to detect expression errors vs. circular definitions?
                if (!derivedStats.list[i].TryEvaluate((target as Actor), derivedStats, out derivedValue)) derivedEquationErrorMessage = "Invalid expression.";

                string newDerivedPropEquation = EditorGUILayout.TextField(derivedStats.list[i].expression, derivedEquationErrorMessage.Length == 0 ? EditorStyles.textField : errorBoxStyle);
                if (derivedEquationErrorMessage.Length > 0)
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent(string.Empty, derivedEquationErrorMessage));
                else
                    EditorGUILayout.LabelField(derivedValue.ToString(), GUILayout.MaxWidth(90));

                if (newDerivedPropEquation != derivedStats.list[i].expression) derivedPropEquationChanged = true;

                derivedStats.list[i].expression = newDerivedPropEquation;

                EditorGUILayout.EndHorizontal();
            }



            EditorGUILayout.EndVertical();

            bool added = false;
            if (GUILayout.Button("Add"))
            {
                added = true;
                if (derivedStats == null) derivedStats = new DerivedStatList();

                List<DerivedStat> newDerivedStats;
                if (derivedStats.list != null)
                    newDerivedStats = new List<DerivedStat>(derivedStats.list);
                else
                    newDerivedStats = new List<DerivedStat>();
                newDerivedStats.Add(new DerivedStat());
                
                derivedStats.list = newDerivedStats.ToArray();
                this.Repaint();
            }


            if (GUI.changed || added)
            {
                EditorUtility.SetDirty(derivedStats);
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }

            if (derivedPropEquationChanged) this.Repaint();

        }

        DrawDefaultInspector();



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