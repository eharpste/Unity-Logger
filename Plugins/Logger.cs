using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The singleton logger class used to log game play for follow-up analysis.
/// </summary>
/// <remarks>
/// Change History:
/// 2012/10/31: Created a generalized version of the Logger that has non-RumbleBlocks related methods.
/// </remarks>
public class Logger {
	#region ==============================|   Singleton Implementation   |==============================
	/// <summary>
	/// Instantiate a new Logger class on access.
	/// </summary>
	private static readonly Logger instance = new Logger();
	
	/// <summary>
	/// Returns the one and only instance of the Logger class.
	/// </summary>
	public static Logger Instance {
		get { return Logger.instance; }
	}
	
	/// <summary>
	/// The private construction prevents other classess from
	/// instantiating their own copy of the Logger class.
	/// </summary>
	private Logger() { 
		switch (this.Mode) {
		case LoggingMode.Disabled:
			this.Enabled = false;
			break;
		case LoggingMode.FileWriter:
			this.LogWriter = new FileWriter();
			break;
		case LoggingMode.HttpWriter:
			this.LogWriter = new HttpWriter();
			break;
		}		
	}
	#endregion ===========================|------------------------------|==============================
	
	#region ==============================|     Private Declarations     |==============================
	
	/// <summary>
	/// The interface used to output the log messages to persistent storage.
	/// </summary>
	private ILogWriter LogWriter;
	
	/// <summary>
	/// Comes from server - what to do in the case of files?
	/// </summary>
	private string SessionID;
	
	/// <summary>
	/// Turns off logging when the game does not have a valid session state.
	/// </summary>
	private bool InSession = false;	
	
	/// <summary>
	/// Turns off LevelEnd (or LevelAbort, or similar) logging when the game does not have a valid logged level state.
	/// Always assumed non-null (set to empty string when there is no logged level active).
	/// </summary>
	private string LoggedLevelName = "";	
	
	/// <summary>
	/// Cache for the system and demographics information (presumed to never be null; set to empty string when not in use)
	/// </summary>
	private string SystemAndDemographicsInfo = "";
	
	#endregion ===========================|------------------------------|==============================
	
	#region ==============================|        Private Methods       |==============================
	
	/// <summary>
	/// Formats a game object as an XML string for logging.
	/// </summary>
	/// <param name="gob">The game object to XMLify, assumed to verified by caller to be not null</param>
	/// <returns>
	/// A <see cref="System.String"/>An XML-like loggable string.</returns>
	private string FormatObject(GameObject gob) {
		return System.String.Format("<Object><Name>{0}</Name>{1}</Object>",
		                                gob.name, FormatTransform(gob));
	}
	
	/// <summary>
	/// Formats a transform as an XML string for logging.
	/// </summary>
	/// <param name="gob">The game object to stringify.</param>
	/// <returns>A loggable string containing the transform properties.</returns> 
	private string FormatTransform(GameObject gob) {
		if (gob.transform.rigidbody == null) {
			// Transform with no rigid body
			// Collision with non-rigid body object
			//return System.String.Format("Transform(Position({0},{1}),Rotation({2}))",
			return System.String.Format("<Transform><Position><X>{0}</X><Y>{1}</Y></Position><Rotation>{2}</Rotation></Transform>",
		                                gob.transform.position.x,
		                                gob.transform.position.y,
		                                gob.transform.rotation.z);			
		} else {
			// Transform with rigid body		
			return System.String.Format("<Transform><Position><X>{0}</X><Y>{1}</Y></Position><Rotation>{2}</Rotation><Velocity><X>{3}</X><Y>{4}</Y></Velocity><AngularVelocity>{5}</AngularVelocity></Transform>",
		                                gob.transform.position.x,
		                                gob.transform.position.y,
		                                gob.transform.rotation.z,
		                                gob.rigidbody.velocity.x,
		                                gob.rigidbody.velocity.y,
		                                gob.rigidbody.angularVelocity.z);		
		}
	}
	
	/// <summary>
	/// Write the specified selection, action and input.
	/// </summary>
	private void write(string selection, string action, string input)  {
		// transactionID defaults to empty string.
		write(selection, action, input, string.Empty);
	}
	
	/// <summary>
	/// A very DataShop-y way of doing things.  
	/// </summary>
	private void write(string selection, string action, string input, string transactionID) {
		// Make sure we have a log writer or we'll get a null reference.
		// Also, only log if we are in session.
		if (this.LogWriter == null || !this.InSession)
			return;
		if(transactionID == "")
			transactionID = System.Guid.NewGuid().ToString();
		// 0.Date-Time 1.SessionID 2.UserID  3.Selection 4.Action 5.Input
		// See: http://pslcdatashop.org/about/importverify.html#note-2
		// yyyy-MM-dd HH:mm:ss.SSS	2010-05-11 16:06:11.908 ** CTAT format
		this.LogWriter.Write(System.String.Format("<Event><Time>{0:yyyy-MM-dd HH:mm:ss.fff}</Time><SessionID>{1}</SessionID><UserID>{2}</UserID><TransactionID>{3}</TransactionID><LevelName>{4}</LevelName><Selection>{5}</Selection><Action>{6}</Action><Input>{7}</Input></Event>", 
					                               System.DateTime.Now.ToUniversalTime(),
					                               this.SessionID, this.UserID,transactionID,this.LoggedLevelName, selection, action, input));
	}
	#endregion ===========================|------------------------------|==============================
		
	#region ==============================|      Public Declarations     |==============================
	/// <summary>
	/// Possible logging modes used by Mode property.
	/// </summary>
	public enum LoggingMode {
		Disabled,
		FileWriter,
		HttpWriter
	}
	
	/// <summary>
	/// Determines if collision logging should be suppressed.
	/// </summary>
	public bool SuppressCollisions = false;
	
	/// <summary>
	/// Determines if logging is enabled or not.
	/// </summary>
	public bool Enabled = true;
	
	/// <summary>
	/// Determines if logger should use file or network for logging. 
	/// </summary>
	public LoggingMode Mode = LoggingMode.FileWriter;
	
	/// <summary>
	/// The user id which will be associated with this logging session.
	/// </summary>
	public string UserID = "anonymous";
	
	/// <summary>
	/// The name of the current level.
	/// </summary>
	public string LevelName = "unknown";
	#endregion ===========================|------------------------------|==============================
	
	#region ==============================|        Public Methods        |==============================	
	
	#region General Events
	
	/// <summary>
	/// Logs an action defined with the given selection, action, and input.
	/// </summary>
	/// <param name="selection">The selection of the action. This would be the thing being acted upon.</param>
	/// <param name="action">The action type of the action. This would be what is being done to the selection.</param>
	/// <param name="input">The input of the action. This would be a value of the action.</param>
	public void LogAction(string selection, string action, string input) {
		if (!this.Enabled) return;
		this.write(selection,action,input);
	}
	
	/// <summary>
	/// Logs an action defined with the given selection, action, and input.
	/// input will be converted to a string by a call to toString().
	/// </summary>
	/// <param name="selection">The selection of the action. This would be the thing being acted upon.</param>
	/// <param name="action">The action type of the action. This would be what is being done to the selection.</param>
	/// <param name="input">The input of the action. This would be a value of the action.</param>
	public void LogAction(string selection, string action, float input) {
		LogAction(selection,action,input.ToString());
	}
	
	/// <summary>
	/// Logs an action defined with the given selection, action, and input.
	/// input will be converted to a string by a call to toString().
	/// </summary>
	/// <param name="selection">The selection of the action. This would be the thing being acted upon.</param>
	/// <param name="action">The action type of the action. This would be what is being done to the selection.</param>
	/// <param name="input">The input of the action. This would be a value of the action.</param>
	public void LogAction(string selection, string action, int input) {
		LogAction(selection,action,input.ToString());
	}
	
	/// <summary>
	/// Logs an action defined with the given selection, action, and input.
	/// input will be converted to a string by a call to toString().
	/// </summary>
	/// <param name="selection">The selection of the action. This would be the thing being acted upon.</param>
	/// <param name="action">The action type of the action. This would be what is being done to the selection.</param>
	/// <param name="input">The input of the action. This would be a value of the action.</param>
	public void LogAction(string selection, string action, bool input) {
		LogAction(selection,action,input.ToString());
	}
	
	/// <summary>
	/// Logs an action defined with the given selection, action, and input.
	/// input will be converted to a string by a call to toString().
	/// </summary>
	/// <param name="selection">The selection of the action. This would be the thing being acted upon.</param>
	/// <param name="action">The action type of the action. This would be what is being done to the selection.</param>
	/// <param name="input">The input of the action. This would be a value of the action.</param>
	public void LogACtion(string selection, string action, object input) {
		LogAction(selection,action,input.ToString());
	}
	
	/// <summary>
	/// Logs an action defined with the given selection, action, and input.
	/// Additionally accepts a flag for whether or not to log the state along with the action.
	/// </summary>
	/// <param name="selection">The selection of the action. This would be the thing being acted upon.</param>
	/// <param name="action">The action type of the action. This would be what is being done to the selection.</param>
	/// <param name="input">The input of the action. This would be a value of the action.</param>
	/// <param name="withState">A flag for whether or not to log a State along with the action.</para>
	public void LogAction(string selection, string action, string input, bool withState) {
		if (!this.Enabled) return;
		
		string guid = System.Guid.NewGuid().ToString();
		if(withState)
			this.RecordGameState();
		this.write(selection,action,input,guid);
	}
	
	/// <summary>
	/// Logs an action defined with the given selection, action, and input.
	/// Additionally accepts a flag for whether or not to log the state along with the action.
	/// input will be converted to a string by a call to toString().
	/// </summary>
	/// <param name="selection">The selection of the action. This would be the thing being acted upon.</param>
	/// <param name="action">The action type of the action. This would be what is being done to the selection.</param>
	/// <param name="input">The input of the action. This would be a value of the action.</param>
	/// <param name="withState">A flag for whether or not to log a State along with the action.</para>
	public void LogAction(string selection, string action, float input, bool withState) {
		LogAction(selection,action,input.ToString(),withState);
	}
	
	/// <summary>
	/// Logs an action defined with the given selection, action, and input.
	/// Additionally accepts a flag for whether or not to log the state along with the action.
	/// input will be converted to a string by a call to toString().
	/// </summary>
	/// <param name="selection">The selection of the action. This would be the thing being acted upon.</param>
	/// <param name="action">The action type of the action. This would be what is being done to the selection.</param>
	/// <param name="input">The input of the action. This would be a value of the action.</param>
	/// <param name="withState">A flag for whether or not to log a State along with the action.</para>
	public void LogAction(string selection, string action, int input, bool withState) {
		LogAction(selection,action,input.ToString(),withState);
	}
	
	/// <summary>
	/// Logs an action defined with the given selection, action, and input.
	/// Additionally accepts a flag for whether or not to log the state along with the action.
	/// input will be converted to a string by a call to toString().
	/// </summary>
	/// <param name="selection">The selection of the action. This would be the thing being acted upon.</param>
	/// <param name="action">The action type of the action. This would be what is being done to the selection.</param>
	/// <param name="input">The input of the action. This would be a value of the action.</param>
	/// <param name="withState">A flag for whether or not to log a State along with the action.</para>
	public void LogAction(string selection, string action, bool input, bool withState) {
		LogAction(selection,action,input.ToString(),withState);
	}
	
	/// <summary>
	/// Logs an action defined with the given selection, action, and input.
	/// Additionally accepts a flag for whether or not to log the state along with the action.
	/// input will be converted to a string by a call to toString().
	/// </summary>
	/// <param name="selection">The selection of the action. This would be the thing being acted upon.</param>
	/// <param name="action">The action type of the action. This would be what is being done to the selection.</param>
	/// <param name="input">The input of the action. This would be a value of the action.</param>
	/// <param name="withState">A flag for whether or not to log a State along with the action.</para>
	public void LogAction(string selection, string action, object input, bool withState) {
		LogAction(selection,action,input.ToString(),withState);
	}
	
	#endregion General Events
	
	#region Button Events
	
	/// <summary>
	/// Logs a button click event.
	/// </summary>
	/// <param name="buttonName">The name of the button which was clicked.</param>
	public void ButtonClicked(string buttonName) {
		if (!this.Enabled) return;
		
		this.write(buttonName, "Clicked", "-1");
	}
	#endregion Button Events
	
	#region Level Events
	
	/// <summary>
	/// Records the start of a new level.  If there was a logged level before,
	/// closes it out first.
	/// </summary>
	/// <param name="level">The name of the level being started.</param>
	public void LevelStart(string level) {
		if (!this.Enabled) return;
		
		if (LoggedLevelName.Length > 0) {
			LevelEnd(LoggedLevelName);
		}
		
		if (level != null) {
			LoggedLevelName = level;
			this.write("System", "Level_Start", level);
		}
		else
			LoggedLevelName = "";
	}
	
	/// <summary>
	/// Records the restart of a level by the user.
	/// </summary>
	/// <param name="level">The level being restarted.</param>
	public void LevelRestart(string level) {
		if (!this.Enabled) return;
		
		this.RecordGameState();
		this.LoggableObjects.Clear();
		
		if (level != null)
		{
			// Protect against unexpected: restarting a different level from what is active
			if (level != LoggedLevelName) {
				if (LoggedLevelName.Length > 0) {
					LevelEnd(LoggedLevelName);
				}
				LoggedLevelName = level;
			}
				
			this.write("System", "Level_Restart", level);
		}
		else { // again, an unexpected case: restarting on null
			if (LoggedLevelName.Length > 0) {
				LevelEnd(LoggedLevelName);
			}
			LoggedLevelName = "";
		}
	}
	
	/// <summary>
	/// Records the end of a level.
	/// </summary>
	/// <param name="level">The level being completed.</param>
	public void LevelEnd(string level) {
		if (!this.Enabled) return;
		
		this.RecordGameState();
		this.LoggableObjects.Clear();
		
		// Only proceed with output if that's what we have for Level_Start
		// or LevelRestart as cached in LoggedLevelName, i.e., 
		// if we didn't care to log this level's start, don't bother logging its end either.
		if (level == LoggedLevelName) {
			this.write("System", "Level_End", level);
			LoggedLevelName = "";
		}
	}
	
	/// <summary>
	/// Records the abort action for ending a level (follows up
	/// by calling LevelEnd).
	/// </summary>
	/// <param name="reasonForAbort">Reason for abort.</param>
	/// <param name="level">The level being completed.</param>
	public void LevelAborted(string reasonForAbort, string level) {
		if (!this.Enabled) return;
		
		// Only proceed with additional "abort" output if this level is what we have for Level_Start
		// or LevelRestart as cached in LoggedLevelName, i.e., 
		// if we didn't care to log this level's start, don't bother logging its abort either.
		if (level == LoggedLevelName)
			this.write("System", "Level_Aborted", reasonForAbort);
		
		// Always proceed with additional LevelEnd logic:
		LevelEnd(level);
	}
	
	#endregion Level Events
		
	#region [Game]Object Events
	
	/// <summary>
	/// Records object collision events.
	/// </summary>
	/// <param name="gob">The game object initiating the collision.</param>
	/// <param name="col">The game object receiving the collision.</param>
	public void ObjectCollision(GameObject gob, GameObject col) {
		if (!this.Enabled || gob == null || col == null) return;
		
		// Do not record collisions during earthquakes or if explicitly suppressed.
		if (this.SuppressCollisions) return;
		
		this.write(gob.name, "Collision",
		           System.String.Format("<Collision>{0}{1}</Collision>",
		                                this.FormatObject(gob),
		                                this.FormatObject(col)));
	}
	
	/// <summary>
	/// Records an action performed on an object using the object's transform as the Input.
	/// </summary>
	/// <param name="gob">The GameObject being acted upon.</param>
	/// <param name="action">A string describing the action being taken.</param>
	public void ObjectAction(GameObject gob, string action) {
		if (!this.Enabled || gob == null) return;
		this.write(gob.name,action,this.FormatTransform(gob));
	}
	
	/// <summary>
	/// Records an action performed on an object using the object's transform as the Input.
	/// Additionally accepts a flag for whether or not to log the state along with the action.
	/// </summary>
	/// <param name="gob">The GameObject being acted upon.</param>
	/// <param name="action">A string describing the action being taken.</param>
	/// <param name="withState">A flag for whether or not to log a State along with the action.</para>
	public void ObjectAction(GameObject gob, string action, bool withState) {
		if (!this.Enabled || gob == null) return;
		string guid = System.Guid.NewGuid().ToString();
		if(withState)
			this.RecordGameState(guid);
		this.write(gob.name, action, this.FormatTransform(gob),guid);
	}
		
	#endregion [Game]Object Events
	
	#region Session Events
	
	/// <summary>
	/// Logs the end of the session and disposes of the session and user ids.  
	/// Resets session state
	/// </summary>
	public void SessionEnd() {
		if (!this.Enabled || !this.InSession) return;
		
		this.write("System", "Session_End", this.SessionID);
		
		this.SessionID = string.Empty;
		this.UserID = string.Empty;
		this.InSession = false;
		this.LoggedLevelName = "";
	}
	
	/// <summary>
	/// Writes the sequence load ID (how levels are ordered to the user), which
	/// comes from the caller.  Before doing so, closes out any prior session
	/// and starts a new one that will be associated with this sequence info.
	/// </summary>
	/// <param name='sequenceInfo'>
	/// Sequence information clear enough to distinguish different sequence options.
	/// </param>
	public void SessionSequenceLoaded(string sequenceInfo) {
		if (!this.Enabled) return;
		
		SessionStart(); // start new session first
		
		this.write("System", "Session_SequenceLoaded", sequenceInfo);
	}
	
	/// <summary>
	/// Sets the demographics, logging them if we are InSession. 
	/// If not InSession, they are cached and logged when InSession is set to true.
	/// </summary>
	/// <param name="givenSystemAndDemographicsInfo"> Given system and demographics information</param>
	public void SetSystemAndDemographicsInfo(string givenSystemAndDemographicsInfo) {
		if (givenSystemAndDemographicsInfo == null)
			SystemAndDemographicsInfo = "";
		else
			SystemAndDemographicsInfo = givenSystemAndDemographicsInfo;
		if (this.InSession && SystemAndDemographicsInfo.Length > 0)
			this.write("System", "Session_SystemAndDemographics", givenSystemAndDemographicsInfo);
	}
	
	/// <summary>
	/// Gets a new session id from the server or generates one if using text logging
	/// then logs the start of the session to the log file.
	/// </summary>
	/// <remarks>Called only from SessionSequenceLoaded so that every active
	/// session will always have information on sequence loading, and will have
	/// nothing else prior to sequence loading.  Also ends any existing session first
	/// before starting a new session.</remarks>
	private void SessionStart() {
		if (!this.Enabled) return;

		if (this.InSession)
			SessionEnd(); // end prior session
		
		// SessionID is defined as a GUID so that all sessions across
		// all players can be uniquely identified.
		this.SessionID = System.Guid.NewGuid().ToString();
		
		this.InSession = true;
		this.write("System", "Session_Start", this.SessionID);
		if (SystemAndDemographicsInfo.Length > 0)
			this.write("System", "Session_SystemAndDemographics", SystemAndDemographicsInfo);
	}
	
	#endregion Session Events
	
	#endregion ===========================|------------------------------|==============================	
	
	/// <summary>
	/// The list of game objects to include in the data snapshot.
	/// </summary>
	private List<GameObject> LoggableObjects = new List<GameObject>();
	
	/// <summary>
	/// Adds the given game object to the list of items to be included
	/// in a the game state snapshot.
	/// </summary>
	/// <param name="gob">A game object to be included in the data snapshot.</param>
	public void AddLoggableObject(GameObject gob) {
		if (!this.Enabled) return;
		
		if (this.LoggableObjects != null && !this.LoggableObjects.Contains(gob)) {
			this.LoggableObjects.Add(gob);
		}
	}
	
	/// <summary>
	/// Removes the given game object from the list of items to be included
	/// in a the game state snapshot.
	/// </summary>
	/// <param name="gob">A game object to be removed from future data snapshots.</param>
	public void RemoveLoggableObject(GameObject gob) {
		if(!this.Enabled) return;
		
		if(this.LoggableObjects != null && this.LoggableObjects.Contains(gob)) {
			this.LoggableObjects.Remove(gob);
		}
	}
	
	/// <summary>
	/// Clears the list of games objects being tracked for data snapshots.
	/// </summary>
	public void ClearLoggableObjects() {
		if(!this.Enabled) return;
		
		LoggableObjects = new List<GameObject>();
	}
	
	/// <summary>
	/// Records the state of the game.
	/// </summary>
	public void RecordGameState() {
		RecordGameState(string.Empty);
	}
		
	/// <summary>
	/// Records a data state snapshot of all game objects in the scene.
	/// </summary>	
	public void RecordGameState(string transactionID) {
		if (!this.Enabled) return;
		
		if (this.LoggableObjects == null) {
			this.write("System", "State", "Error, state object null.");
		} else {
			string state = "<State>";
			List<GameObject> newLoggableObjects = new List<GameObject>();
			foreach (GameObject gob in LoggableObjects) {
				if (gob != null) {
					state += this.FormatObject(gob);
					newLoggableObjects.Add(gob);
				}
			}
			LoggableObjects = newLoggableObjects;
			state += "</State>";
			this.write("System", "State", state, transactionID);
		}
	}	
	
	/// <summary>
	/// Records the players success or failure for a particular level.
	/// </summary>
	/// <param name="success">
	/// A <see cref="System.Boolean"/>
	/// </param>
	public void EndState(bool success) {
		if (!this.Enabled) return;
		
		this.write("System", "End_State", (success)?"Success":"Failure"); 
	}
	
	/// <summary>
	/// Records the start state of a level.
	/// </summary>
	/// <param name='startStateObjects'>
	/// A List of objects to record in the start state.
	/// </param>
	public void RecordStartState(List<GameObject> startStateObjects) {
		if(!this.Enabled) return;
		
		string startState = "<StartState>";
		foreach(GameObject go in startStateObjects) {
			if (go != null)
				startState += this.FormatObject(go);
		}
		startState+="</StartState>";
		this.write("System","StartState",startState);
	}
}

