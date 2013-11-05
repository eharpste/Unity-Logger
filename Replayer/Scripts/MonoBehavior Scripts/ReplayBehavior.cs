using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;

class ReplayBehavior : MonoBehaviour {

    #region ==============================| Static Things For Fast Lookup |==============================
    private static Dictionary<string, HashSet<GameObject>> tagMap = new Dictionary<string,HashSet<GameObject>>();
    public const string UNTAGGED = "Untagged";
    public const string NO_PREFAB_TAG = "NoPrefabTag";
    public const string NO_UNITY_TAG = "NoUnityTag";

    private static Dictionary<RAETag, HashSet<GameObject>> raeMap = new Dictionary<RAETag, HashSet<GameObject>>();

    private static void PrepTagMap(string key) {
        if (!tagMap.ContainsKey(key)) {
            tagMap[key] = new HashSet<GameObject>();
        }
    }

    private static void MoveTag(string oldTag, string newTag, GameObject gob) {
        if (oldTag == newTag)
            return;
        if (tagMap[oldTag].Remove(gob)) {
            AddToTagMap(newTag, gob);
        }
    }

    private static void MoveTag(RAETag oldTag, RAETag newTag, GameObject gob) {
        if (oldTag == newTag)
            return;
        if (raeMap[oldTag].Remove(gob)) {
            AddToRAEMap(newTag, gob);
        }
    }

    private static void AddToRAEMap(RAETag tag, GameObject gob) {
        if (!raeMap.ContainsKey(tag)) {
            raeMap[tag] = new HashSet<GameObject>();
        }
        raeMap[tag].Add(gob);
    }

    private static void AddToTagMap(string tag, GameObject gob) {
        PrepTagMap(tag);
        tagMap[tag].Add(gob);
    }

    public static GameObject[] GetGameObjectsWithTag(string tag) {
        if(tagMap.ContainsKey(tag))
            return tagMap[tag].ToArray<GameObject>();
        return new GameObject [0];
    }

    public static GameObject GetFirstGameObjectWithTag(string tag) {
        if(tagMap.ContainsKey(tag))
            return tagMap[tag].First<GameObject>();
        return null;
    }

    public static GameObject[] GetGameObjectsWithRAETag(RAETag tag) {
        if(raeMap.ContainsKey(tag))
            return raeMap[tag].ToArray<GameObject>();
        return new GameObject[0];
    }

    public enum RAETag {
        Untagged,
        State,
        Action,
        StartState,
        Given
    }

    #endregion ===========================|-------------------------------|==============================

    #region ==============================|           Tag things          |==============================

    private string prefabName = null;
    public string PrefabName {
        get {
            return prefabName;
        }
        set {
            if (prefabName != null) {
                if (value == null)
                    value = NO_PREFAB_TAG;
                MoveTag(prefabName, value, this.gameObject);
            }
            else if (value != null) {
                prefabName = value;
                AddToTagMap(prefabName, this.gameObject);
            }
            else {
                prefabName = NO_PREFAB_TAG;
                AddToTagMap(prefabName, this.gameObject);
            }
        }
    }
    
    private string unityTag = null;
    public string UnityTag {
        get {
            return unityTag;
        }
        set {
            if (unityTag != null) {
                if (value == null)
                    value = NO_UNITY_TAG;
                MoveTag(unityTag, value, this.gameObject);
            }
            else if (value != null) {
                unityTag = value;
                AddToTagMap(unityTag, this.gameObject);
            }
            else {
                unityTag = NO_UNITY_TAG;
                AddToTagMap(unityTag, this.gameObject);
            }
        }
    }

    private HashSet<string> tags = new HashSet<string>();

    private RAETag raeTag = RAETag.Untagged;
    public RAETag ReplayTag {
        get {
            return raeTag;
        }
        set {
            if (raeTag != RAETag.Untagged) {
                if (value == RAETag.Untagged)
                    raeMap[raeTag].Remove(this.gameObject);
                else if(value != raeTag)
                    MoveTag(raeTag, value, this.gameObject);
            }
            else if(value != RAETag.Untagged)
                AddToRAEMap(value, this.gameObject);

            this.raeTag = value;
        }
    }

    public void AddTags(string [] newTags) {
        IEnumerable<string> query = newTags.Where(tag => !this.tags.Contains(tag));
        foreach (string tag in query) {
            AddToTagMap(tag, this.gameObject);
            this.tags.Add(tag);
        }
    }

    public void AddTags(List<string> newTags) {
        IEnumerable<string> query = newTags.Where(tag => !this.tags.Contains(tag));
        foreach (string tag in query) {
            AddToTagMap(tag, this.gameObject);
            this.tags.Add(tag);
        }
    }

    public void AddTag(string tag) {
        this.tags.Add(tag);
        AddToTagMap(tag, this.gameObject);
    }

    public bool HasTag(string tag) {
        return this.tags.Contains(tag);
    }

    #endregion ===========================|-------------------------------|==============================

    #region ==============================|   Highlighting and Flashing   |==============================

    private Color bak;
    private bool highlighted = false;
    private bool flashing = false;
    private const float DEFAULT_FLASH_SPEED = .2f;

    private Color NegateColor(Color c) {
        return new Color(1 - c.r, 1 - c.g, 1 - c.b);
    }

    public void Highlight(Color highlight) {
        if (this.renderer == null || this.renderer.material == null)
            return;
        bak = this.renderer.material.color;
        this.renderer.material.color = highlight;
        highlighted = true;
    }

    public void Highlight(Color highlight, bool additive) {
        if (this.renderer == null || this.renderer.material == null)
            return;
        bak = this.renderer.material.color;
        if(additive)
            this.renderer.material.color += highlight;
        else
            this.renderer.material.color = highlight;
        highlighted = true;
    }

    public void Highlight() {
        if (this.renderer == null || this.renderer.material == null)
            return;
        bak = this.renderer.material.color;
        this.renderer.material.color = NegateColor(bak);
        highlighted = true;
    }

    public void Unhighlight() {
        if (this.renderer == null || this.renderer.material == null)
            return;
        this.renderer.material.color = bak;
        highlighted = false;
    }

    public void Flash() {
        if(this.renderer == null || this.renderer.material == null) return;
        Color c = NegateColor(this.renderer.material.color);
        Flash(c);
    }

    public void Flash(Color color) {
        if (this.renderer == null || this.renderer.material == null) return;
        StartCoroutine(FlashCoroutine(color,DEFAULT_FLASH_SPEED,-1,-1,false));
    }

    public void Flash(Color color, bool additive) {
        if (this.renderer == null || this.renderer.material == null) return;
        StartCoroutine(FlashCoroutine(color, DEFAULT_FLASH_SPEED, -1, -1,additive));
    }

    public void Flash(Color color, int numberOfTimes) {
        if (this.renderer == null || this.renderer.material == null) return;
        StartCoroutine(FlashCoroutine(color, DEFAULT_FLASH_SPEED, numberOfTimes, -1,false));
    }

    public void Flash(Color color, int numberOfTimes, bool additive) {
        if (this.renderer == null || this.renderer.material == null) return;
        StartCoroutine(FlashCoroutine(color, DEFAULT_FLASH_SPEED, numberOfTimes, -1,additive));
    }

    public void Flash(int numberOfTimes) {
        if (this.renderer == null || this.renderer.material == null) return;
        Color c = NegateColor(this.renderer.material.color);
        Flash(c, numberOfTimes);
    }

    public void Flash(Color color, float duration) {
        if (this.renderer == null || this.renderer.material == null) return;
        StartCoroutine(FlashCoroutine(color, DEFAULT_FLASH_SPEED, -1, duration,false));
    }

    public void Flash(Color color, float duration, bool additive) {
        if (this.renderer == null || this.renderer.material == null) return;
        StartCoroutine(FlashCoroutine(color, DEFAULT_FLASH_SPEED, -1, duration,additive));
    }

    public void Flash(float duration) {
        if (this.renderer == null || this.renderer.material == null) return;
        Color c = NegateColor(this.renderer.material.color);
        Flash(c, duration);
    }

    IEnumerator FlashCoroutine(Color c, float speed, int numTime, float duration, bool additive) {
        if (this.renderer == null || this.renderer.material == null)
            yield break;
        int count = 0;
        float dur = 0f;
        float timeSinceFlash = 0f;
        bak = this.renderer.material.color;
        while (flashing) {            
            if (timeSinceFlash >= speed) {
                if (!highlighted) {
                    Highlight(c,additive);
                }
                else {
                    Unhighlight();
                    count++;
                }
                timeSinceFlash = 0f;
            }
            timeSinceFlash += Time.deltaTime;
            dur += Time.deltaTime;
            if (numTime > 0 && count >= numTime)
                break;
            if (duration > 0 && dur >= duration)
                break;
            yield return new WaitForEndOfFrame();
        }
        StopFlash();
        yield break;
    }

    public void StopFlash() {
        this.flashing = false;
        Unhighlight();
    }

    #endregion ===========================|-------------------------------|==============================

    #region ==============================|   GetTouching Functionality   |==============================

    private bool recordTouching = true;

    public bool HasTouching {
        get {
            return this.collider != null && touching.Count > 0 && recordTouching;
        }
    }

    public bool RecordTouching {
        get {
            return recordTouching;
        }
        set {
            if (!value)
                touching.Clear();
            this.recordTouching = value;
        }
    }


    private HashSet<GameObject> touching = new HashSet<GameObject>();

    void OnCollisionEnter(Collision collision) {
        if(recordTouching)
            touching.Add(collision.gameObject);
    }

    void OnCollisionExit(Collision collision) {
        if(recordTouching)
            touching.Remove(collision.gameObject);
    }

    void OnCollisionStay(Collision collision) {
        if(recordTouching)
            touching.Add(collision.gameObject);
    }

    public GameObject[] GetTouchingObjects() {
        return touching.ToArray<GameObject>();
    }

    #endregion ===========================|-------------------------------|==============================

    #region ==============================| Property Window Functionality |==============================

    #endregion ===========================|-------------------------------|==============================

    #region ==============================|          Unity Methods        |==============================

    void Start() {
        LoggableObject lo = GetComponent<LoggableObject>();
        if (lo != null) {
            this.AddTags(lo.tags);
            Destroy(lo);
        }
    }

    void OnDestory() {
        touching.Clear();
        foreach (string s in tags) {
            if (tagMap.ContainsKey(s)) {
                tagMap[s].Remove(this.gameObject);
            }
        }
        if (tagMap.ContainsKey(unityTag)) {
            tagMap[unityTag].Remove(this.gameObject);
        }
        if (tagMap.ContainsKey(prefabName)) {
            tagMap[prefabName].Remove(this.gameObject);
        }
        if (raeMap.ContainsKey(raeTag)) {
            raeMap[raeTag].Remove(this.gameObject);
        }
    }

    #endregion ===========================|-------------------------------|==============================
}

