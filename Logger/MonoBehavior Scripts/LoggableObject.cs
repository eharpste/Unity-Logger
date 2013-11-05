using UnityEngine;
using System.Collections;

/// <summary>
/// A very simple behavior script that can be applied to any objects that you 
/// wish to show up in State snaphots of the game throughout their existence in
/// the scene. Use of this script is entirely optional.
/// </summary>
public class LoggableObject : MonoBehaviour {
	/// <summary>
	/// The name of the prefab that this GameObject was a copy of.
	/// It is possible to capture this information from the Awake() method before other
	/// Behaviors get the chance to re-name the Object. Altrernatively this can be set
	/// through its public access in the Interactions pane of the editor.
	/// </summary>/
	public string prefabName = string.Empty;
	
    /// <summary>
    /// A list of additional tags that you want to annotate this object with.
    /// </summary>
    public string [] tags;
	
    /// <summary>
    /// The type of recording that the object should perform.
    /// StartState is only recorded at the beginnig of a level, NormalState is
    /// recorded on state snapshots, and Both with be recorded at both times.
    /// </summary>
	public enum RecordingType {
		StartState,
		NormalState,
		Both
	}
	
    /// <summary>
    /// The type of recording that should be performed on this object.
    /// </summary>
	public RecordingType recordIn = RecordingType.NormalState;
	
	void Awake() {
		if(string.IsNullOrEmpty(prefabName)) {
			prefabName = this.name.Substring(0,this.name.LastIndexOf("(Clone)"));
		}
		if(recordIn == RecordingType.Both || recordIn == RecordingType.StartState) {
			Logger.Instance.AddStartStateObject(this.gameObject);	
		}
	}
	
	// Use this for initialization
	void Start () {
		if(recordIn == RecordingType.Both || recordIn == RecordingType.NormalState) {
			Logger.Instance.AddLoggableObject(this.gameObject);
		}
	}
	
	void OnDestroy() {
		Logger.Instance.RemoveLoggableObject(this.gameObject);
		Logger.Instance.RemoveStartStateObject(this.gameObject);
	}
	
	public string LogDescription() {
		string ret = System.String.Format("<{2}>{0}</{2}><{3}>{1}</{3}>",prefabName,this.tag,Logger.PREFAB,Logger.UNITY_TAG);
		if(tags.Length > 0) {
			ret += "<"+Logger.EXTRA_TAGS+">";
			foreach(string s in tags){
				ret += System.String.Format("<{1}>{0}</{1}>",s,Logger.TAG);
			}
            ret += "</" + Logger.EXTRA_TAGS + ">";
		}
		return ret;
	}
}

