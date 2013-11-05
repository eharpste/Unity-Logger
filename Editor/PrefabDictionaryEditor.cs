using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[CustomEditor(typeof(PrefabDictionary))]
class PrefabDictionaryEditor : Editor {


    public override void OnInspectorGUI() {
        PrefabDictionary dict = target as PrefabDictionary;
        
        EditorGUILayout.BeginVertical();
        
        int tsize = EditorGUILayout.IntField("Size", dict.names.Count);
        if (tsize != dict.names.Count) {
            resizeArrays(tsize);
        }

        int digits = (dict.prefabs.Count > 0 ? (dict.prefabs.Count -1)+"" : "0").Length;
        int pixels = 12;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", GUILayout.Width(digits * pixels));
        EditorGUILayout.LabelField("Key",GUILayout.ExpandWidth(true));
        
        EditorGUILayout.LabelField("Prefab", GUILayout.ExpandWidth(true));
        
        EditorGUILayout.EndHorizontal();

        

        for (int i = 0; i < dict.prefabs.Count; i++) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(i+"",GUILayout.Width(digits * pixels));
            dict.names[i] = EditorGUILayout.TextField(dict.names[i]);
            dict.prefabs[i] = EditorGUILayout.ObjectField(dict.prefabs[i], typeof(GameObject), false) as GameObject;
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("+",GUILayout.Width(digits * pixels));
        string newLabel = EditorGUILayout.TextField(string.Empty);
        if (!string.IsNullOrEmpty(newLabel)) {
           dict.Add(newLabel, null);
        }
        GameObject newPrefab = EditorGUILayout.ObjectField(null, typeof(GameObject), false) as GameObject;
        if (newPrefab != null) {
           dict.Add("key", newPrefab);
        }
        EditorGUILayout.EndHorizontal();
        

        EditorGUILayout.EndVertical();
        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    private void resizeArrays(int newSize) {
        List<GameObject> tp = (target as PrefabDictionary).prefabs;
        List<string> tn = (target as PrefabDictionary).names;

        if (newSize == 0) {
            tp.Clear();
            tn.Clear();
        }
        else if(newSize == tn.Count) {
            return;
        }
        else if (newSize < tn.Count) {
            tn.RemoveRange(newSize, tn.Count - newSize);
            tp.RemoveRange(newSize, tp.Count - newSize);
        }
        else {
            for (int i = tn.Count; i < newSize; i++) {
                tn.Add(string.Empty);
                tp.Add(null);
            }
        }
    }
}

