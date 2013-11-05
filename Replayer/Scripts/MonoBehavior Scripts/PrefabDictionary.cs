using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[ExecuteInEditMode()]
public class PrefabDictionary : MonoBehaviour {
    public List<GameObject> prefabs;
    public List<string> names;

    public void Add(string label, GameObject prefab) {
        this.names.Add(label);
        this.prefabs.Add(prefab);
    }

    public GameObject GetPrefab(string name) {
        for (int i = 0; i < prefabs.Count; i++) {
            if (names[i] == name)
                return prefabs[i];
        }
        return null;
    }

    public GameObject GetPrefab(int i) {
        if (i < 0 || i > prefabs.Count - 1)
            return null;
        return prefabs[i];
    }
}
