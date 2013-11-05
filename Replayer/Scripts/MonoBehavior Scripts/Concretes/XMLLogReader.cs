using UnityEngine;
using System.Collections;
using System.Xml;

public class XMLLogReader : LogReader {
    private StudentAction curr = StudentAction.NullAction;
    private StudentAction prev = StudentAction.NullAction;
    private StudentAction next = StudentAction.NullAction;
    private StudentAction lastStart = StudentAction.NullAction;

    public string xmlFilePath = string.Empty;
    private string errorMessage = null;
    private XmlNodeEnumerator nodes = null;
    private bool hasNext = true;
    private ReplayGUI mainGUI;

	// Use this for initialization
	void Start () {
        mainGUI = GetComponent<ReplayGUI>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public override StudentAction Current {
        get { return curr; }
    }

    public override StudentAction Previous {
        get { return prev; }
    }

    public override StudentAction Next {
        get { return next; }
    }

    public override StudentAction LastStartState {
        get { return lastStart; }
    }

    public override bool IsNewAttempt {
        get { return false; }
    }

    public override bool Loaded {
        get { return nodes != null; }
    }

    public override bool HasNext {
        get { return hasNext; }
    }

    public override void Load() {
        Debug.Log("Load()");
        nodes = new XmlNodeEnumerator(xmlFilePath);
    }

    public override StudentAction GetNextStudentAction() {
        Debug.Log("GetNextStudentAction()");
        prev = curr;
        curr = next;
        XmlNode node = GetNextEvent();
        Debug.Log(node.OuterXml);
        if (node != null) {
            string selection = node[StudentAction.SELECTION].InnerText;
            string action = node[StudentAction.ACTION].InnerText;
            string input = node[StudentAction.INPUT].InnerText;
            string levelName = node[StudentAction.SELECTION].InnerText;
            string state = node[StudentAction.STATE].InnerText;
            string time = node[StudentAction.TIME].InnerText;
            string user = node[StudentAction.USER_ID].InnerText;
            string attemptNum = node[StudentAction.ATTEMPT_NUMBER].InnerText;
            string sessionID = node[StudentAction.SESSION_ID].InnerText;
            string transactionID = node[StudentAction.TRANSACTION_ID].InnerText;
            next = new StudentAction(selection, action, input, state, time, user, levelName, attemptNum, sessionID, transactionID);
        }
        else {
            next = StudentAction.NullAction;
        }
        return curr;
    }

    private XmlNode GetNextEvent() {
        if (nodes.MoveNext()) {
            XmlNode ret = nodes.Current as XmlNode;
            if (ret == null) {
                hasNext = false;
                return null;
            }
            return ret;
        }
        else {
            Debug.Log("nodes.MoveNext() == false");
            hasNext = false;
            return null;
        }
    }

    private Vector2 scrollPos = Vector2.zero;
    public override void OptionsPane(Rect optionArea) {
        float bWidth = mainGUI.standardButtonWidth, bHeight = mainGUI.standardButtonHeight;
        GUILayout.BeginArea(optionArea);
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label("XML File Path:", GUILayout.Height(bHeight), GUILayout.Width(bWidth));
        xmlFilePath = FixPath(GUILayout.TextField(xmlFilePath, GUILayout.Height(bHeight), GUILayout.Width(bWidth * 4)));
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    public override string GUIName {
        get { return "XML Reader Options"; }
    }

    void OnGUI() {
        if (!string.IsNullOrEmpty(errorMessage)) {
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            GUILayout.BeginVertical();

            GUILayout.Label("There was an error:\n" + errorMessage);
            if (GUILayout.Button("OK", GUILayout.Width(mainGUI.standardButtonWidth), 
                GUILayout.Height(mainGUI.standardButtonHeight))) {
                errorMessage = null;
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();

        }
    }

    private string FixPath(string path) {
        return path.Trim('"');
    }

   class XmlNodeEnumerator : IEnumerator {
        private XmlReader reader;
        public object Current {
            get {
                return currentNode;
            }
        }

        private XmlNode currentNode;
        private string filePath;

        public XmlNodeEnumerator(string filePath) {
            this.filePath = filePath;
            Reset();
        }

        public bool MoveNext() {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode testNode;

            try {
                testNode = xmlDoc.ReadNode(reader);
            }
            catch (System.Exception ex) {
                Debug.LogException(ex);
                return false;
            }
            if (testNode != null) {
                currentNode = testNode;
                return true;
            }
            return false;
        }

        public void Reset() {
            reader = new XmlTextReader(filePath);
            reader.ReadToDescendant("Event");
        }

        public void Dispose() {
            reader.Close();
            currentNode = null;
        }
    }

}
