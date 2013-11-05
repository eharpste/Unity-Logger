using UnityEngine;
using System.Collections;
using System.Text;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Student action.
/// </summary>
/// <remarks>
/// Change History:
/// 2012/11/08: Added commnents header. Removed lingering build warning.
/// </remarks>
public class StudentAction {
    public static readonly string OBJECT = "Object";
	public static readonly string TRANSFORM = "Transform";
	public static readonly string POSITION = "Position";
	public static readonly string ROTATION = "Rotation";
	public static readonly string VELOCITY = "Velocity";
	public static readonly string ANGULAR_VELOCITY = "AngularVelocity";
	public static readonly string SELECTION = "Selection";
	public static readonly string ACTION = "Action";
	public static readonly string INPUT = "Input";
	public static readonly string STATE = "State";
	public static readonly string TIME = "Time";
	public static readonly string SESSION_ID = "SessionID";
    public static readonly string TRANSACTION_ID = "TransactionID";
	public static readonly string USER_ID = "UserID";
 	public static readonly string ATTEMPT_NUMBER = "AttemptNumber";
	public static readonly string LEVEL_NAME = "LevelName";
	public static readonly string ACTION_NUMBER = "ActionNumber";

	private static StudentAction nullAction = new StudentAction();
	
	public static StudentAction NullAction {
		get {
			return nullAction;
		}
	}
	
	public int Attempt {
		get {
            return attemptNum;
		}
	}
	
	public string LevelName {
		get {
            return levelName;
		}
	}
	
	public string User {
		get {
            return userID;
		}
	}
	
	public string SessionID {
        get {
            return sessionID;
        }
	}

    public string TransactionID {
        get {
            return transactionID;
        }
    }
	
	public string Selection {
		get{
            return selection;
		}
	}
	
	public string Action {
		get {
            return action;
		}
	}
	
	public string Input {
		get {
            return inputString;
		}
	}

    public XmlNode InputXML {
        get {
            return inputXML;
        }
    }

    public System.DateTime Time {
        get {
            return time;
        }
    }

    public XmlNode StateXML {
        get {
            return this.state;
        }
    }

    private string selection = null;
    private string action = null;
    private XmlNode inputXML = null;
    private string inputString = null;
    private XmlNode state = null;
    private System.DateTime time = System.DateTime.MaxValue;
    private string userID = null;
    private string levelName = null;
    private int attemptNum = -1;
    private string sessionID = null;
    private string transactionID = null;

    private StudentAction() { }

    public StudentAction(string selection, string action, string input) :  this(selection, action, input, "", "", "", "", "", "", "") { }

    public StudentAction(string selection, string action, string input, string state, string time,
        string user, string levelName, string attemptNum, string sessionID, string transactionID) {

            this.selection = selection;
            this.action = action;

            if (input != null && input.Contains("<Transform>")) {
                if (!input.Contains("<Object>")) {
                    input = "<Object>" + input + "</Object>";
                }
                input = "<Input>" + input + "</Input>";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(input);
                this.inputXML = xmlDoc[INPUT];
                this.inputString = input;
            }
            else {
                this.inputString = input;
                this.inputXML = null;
            }

            if (state != null && state.Contains("<Object>")) {
                state = "<State>" + state + "</State>";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(state);
                this.state = xmlDoc[STATE];
            }
            else {
                this.state = null;
            }

            try {
                this.time = System.DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.CurrentCulture);
            }
            catch {
                this.time = System.DateTime.MaxValue;
            }

            this.userID = user;
            this.levelName = levelName;
            try {
                this.attemptNum = int.Parse(attemptNum);
            }
            catch  {
                this.attemptNum = -1;
            }
            this.sessionID = sessionID;
            this.transactionID = transactionID;
    }
	
	public bool HasObjectInput() {
        return inputXML != null;
	}
	
    /*
	public XmlNode GetState(){
        if (state == null)
            return null;
        try {
            return state[INPUT][STATE];
        }
        catch (System.Exception ex){
            Debug.Log(state.OuterXml);
            Debug.LogException(ex);
        }
        return null;
	}
     */ 
	
    public Vector3 GetObjectPosition() {
		if(HasObjectInput()) {
			float posX = float.Parse(inputXML[OBJECT][TRANSFORM][POSITION]["X"].InnerText);
            float posY = float.Parse(inputXML[OBJECT][TRANSFORM][POSITION]["Y"].InnerText);
			return new Vector3(posX,posY,0);
		}
		else
			return Vector3.zero;
	}
	
	public Quaternion GetObjectRotation() {
		if(HasObjectInput()) {
            float rotZ = float.Parse(inputXML[OBJECT][TRANSFORM][ROTATION].InnerText);
			float w = (float)System.Math.Sqrt(1-(rotZ*rotZ));
			return new Quaternion(0,0,rotZ,w);
		}
		else
			return Quaternion.identity;
	}
	
	public Vector3 GetObjectVelocity() {
		if(HasObjectInput()) {
            float posX = float.Parse(inputXML[OBJECT][TRANSFORM][VELOCITY]["X"].InnerText);
            float posY = float.Parse(inputXML[OBJECT][TRANSFORM][VELOCITY]["Y"].InnerText);
			return new Vector3(posX,posY,0);
		}
		else
			return Vector3.zero;
	}
	
	public Quaternion GetObjectAngularVelocity() {
		if(HasObjectInput()) {
            float rotZ = float.Parse(inputXML[OBJECT][TRANSFORM][ANGULAR_VELOCITY].InnerText);
			float w = (float)System.Math.Sqrt(1-(rotZ*rotZ));
			return new Quaternion(0,0,rotZ,w);
		}
		else
			return Quaternion.identity;
	}
	
	public override string ToString() {
        return System.String.Format("SELECTION:{0} ACTION:{1} INPUT:{2} STATE:{3}", selection, action, inputString, state != null ? state.OuterXml : "No State");
	}

    public bool IsSameAttempt(StudentAction other) {
        if (this == NullAction || other == NullAction)
            return this == other;
        return this.User == other.User
            && this.SessionID == other.SessionID
            && this.LevelName == other.LevelName
            && this.Attempt == other.Attempt;
    }
}