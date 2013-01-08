using UnityEngine;
using System.Collections;

/// <summary>
/// A very simple behavior script that can be applied to an objects that you wish to show up in State snaphots of the game
/// throughout their existence in the scene. Use of this script is entirely optional.
/// </summary>
public class LoggableObject : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Logger.Instance.AddLoggableObject(this.gameObject);
	}
	
	void OnDestroy() {
		Logger.Instance.RemoveLoggableObject(this.gameObject);
	}
}

