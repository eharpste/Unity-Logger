using UnityEngine;
using System.Collections;

/// <summary>
/// This behavior scrit is designed to manage Session and Level information for the Logger. 
/// Its use is optional but its design can serve as a template for how and when to call the relevant Session and Level methods.
/// It is designed to be used as a DontDestroyOnLoad GameObject that is only Destroyed at the end of play.
/// </summary>
public class SessionManager : MonoBehaviour {
	private bool restarting = false;
	
	/// <summary>
	/// Begins the SessionLog and registers this GameObject to not be destroyed when new levels load.
	/// </summary>
	void Start () {
		Logger.Instance.SessionStart();
		DontDestroyOnLoad(this.gameObject);
	}
	
	/// <summary>
	/// This assumes the SessionManager is being destroyed at the end of the session, or when the game is being closed.
	/// A LevelAbort is logged incase this is an unintended exit, if a LevelEnd is called prior to this the LevelAborted call will have no effect.
	/// </summary>
	void OnDestroy() {
		Logger.Instance.LevelAborted("Session Being Terminated",Application.loadedLevelName);
		Logger.Instance.SessionEnd();
	}
	
	/// <summary>
	/// Logs the end of this level and then loads the level specified by the name levelName.
	/// </summary>
	/// <param name="levelName">The name of the level you wish to load next.</param>
	public void LoadLevel(string levelName) {
		restarting = false;
		Application.LoadLevel(levelName);
	}
	
	/// <summary>
	/// Logs the end of this level and then loads the level specified by the index level.
	/// </summary>
	/// <param name="level">The index of the level you wish to load next.</param>
	public void LoadLevel(int level) {
		Logger.Instance.LevelEnd(Application.loadedLevelName);
		restarting = false;
		Application.LoadLevel(level);
	}
	
	/// <summary>
	/// Registers the end of a level if that happens seperate from a scene transition.
	/// </summary>
	public void EndLevel() {
		Logger.Instance.LevelEnd(Application.loadedLevelName);
	}
	
	/// <summary>
	/// Restarts the current level by reloading the scene and logging it as a RestartLevel.
	/// </summary>
	public void RestartLevel() {
		restarting = true;
		Application.LoadLevel(Application.loadedLevel);
	}
	
	
	/// <summary>
	/// Logs the start of new levels taking into account whether they are a restart or not.
	/// Note, if the LoadLevel and RestartLevel methods of this script are not used it will 
	/// always assume it is loading a new level and never restarting.
	/// </summary>
	/// <param name="level">Level.</param>
	void OnLevelWasLoaded(int level) {
		if(!restarting) {
			Logger.Instance.LevelStart(Application.loadedLevelName);
		}
		
		else {
			Logger.Instance.LevelRestart(Application.loadedLevelName);	
		}
		
		restarting = false;
	}
}
