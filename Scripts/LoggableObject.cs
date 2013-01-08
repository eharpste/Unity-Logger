using UnityEngine;
using System.Collections;

public class LoggableObject : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Logger.Instance.AddLoggableObject(this.gameObject);
	}
	
	void OnDestroy() {
		Logger.Instance.RemoveLoggableObject(this.gameObject);
	}
}

