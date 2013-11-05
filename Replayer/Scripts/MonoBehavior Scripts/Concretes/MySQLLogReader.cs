using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;


public class MySQLLogReader : LogReader {

    private ReplayGUI mainGUI;    
    private MySqlConnection conn;
    private MySqlDataReader reader;

    void Start() {       
        mainGUI = GetComponent<ReplayGUI>();
    }

    private string sqlErrorString = null;

    void OnGUI() {
        if (!string.IsNullOrEmpty(sqlErrorString)) {
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            GUILayout.BeginVertical();

            GUILayout.Label("There was an SQL error:\n"+sqlErrorString);
            if (GUILayout.Button("OK", GUILayout.Width(mainGUI.standardButtonWidth), 
                GUILayout.Height(mainGUI.standardButtonHeight))) {
                sqlErrorString = null;
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
            
        }
    }



    #region ==============================|       LogReader API       |==============================

    private StudentAction curr = StudentAction.NullAction;
    private StudentAction prev = StudentAction.NullAction;
    private StudentAction next = StudentAction.NullAction;
    private StudentAction lastStartState = StudentAction.NullAction;
    private bool loaded = false;
    
    public override void Load() {
        ConnectionString = System.String.Format("Server={0};Port={1};Database={2};Uid={3};password={4}",
                                        server,
                                        portNum,
                                        database,
                                        userID,
                                        password);
        conn = new MySqlConnection(ConnectionString);
        
        MySqlCommand command = conn.CreateCommand();

        command.CommandText = CommandString;

        try {
            conn.Open();
        }
        catch (System.Exception ex) {
            Debug.LogException(ex);
            sqlErrorString = ex.Message;
            sqlErrorString += "\n"+ ex.StackTrace;
        }

        reader = command.ExecuteReader();
        loaded = true;
        GetNextStudentAction();
    }


    public override StudentAction Current {
        get { return curr; }
    }

    public override StudentAction Next {
        get { return next; }
    }

    public override StudentAction Previous {
        get { return prev; }
    }

    public override StudentAction LastStartState {
        get { return lastStartState; }
    }

    public override bool IsNewAttempt {
        get { return false; }
    }

    public override bool Loaded {
        get { return loaded; }
    }

    public override bool HasNext {
        get { return next != StudentAction.NullAction; }
    }

    public override StudentAction GetNextStudentAction() {
        prev = curr;
        curr = next;
        if (reader.Read()) {
            //Debug.Log("Selection: " + reader["selection"]);
            next = new StudentAction(
                reader["selection"].ToString(),
                reader["action"].ToString(),
                reader["input"].ToString(),
                reader["state"].ToString(),
                reader["eventTime"].ToString(),
                reader["userID"].ToString(),
                reader["levelName"].ToString(),
                reader["attemptNumber"].ToString(),
                reader["sessionID"].ToString(),
                reader["transactionID"].ToString()
                );
        }
        else {
            next = StudentAction.NullAction;
        }
        return curr;
    }

    void OnDestroy() {
        if(conn != null)
            conn.Close();
    }

    private string GetField(string field) {
        string ret = null;
        try {
            ret = reader[field].ToString();
        }
        catch (System.Exception ex) {
            Debug.LogException(ex);
            ret = null;
        }
        return ret;
    }

    private Vector2 scrollPos = Vector2.zero;
    public override void OptionsPane(UnityEngine.Rect optionArea) {
        GUILayout.BeginArea(optionArea);
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.BeginVertical();

        float bWidth = mainGUI.standardButtonWidth;
        float bHeight = mainGUI.standardButtonHeight;


        GUILayout.BeginHorizontal();
        GUILayout.Label("Connection Settings", GUILayout.Width(bWidth * 1.2f));
       // GUILayout.Space(20);
        if(GUILayout.Button(connOptionsOpen ? "-" : "+", GUILayout.Width(bWidth/6)))
            connOptionsOpen = !connOptionsOpen;
        GUILayout.EndHorizontal();
        if(connOptionsOpen) {
            ConnectionGUI(bWidth,bHeight);
        }
        GUILayout.Label("Command Settings", GUILayout.Width(bWidth * 1.2f));
        CommandGUI(bWidth, bHeight);

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    public override string GUIName {
        get { return "Database Options"; }
    }

    #endregion ============================|------------------------------|==============================

    #region  ==============================|       Command Settings       |==============================

    public string CommandString {
        get {
            return System.String.Format("SELECT {0} FROM {1} WHERE {2} {3};", selectClause, fromClause, whereClause,otherClauses);
        }
    }


   // List<string[]> args = new List<string[]>();
    
    private bool directEditCommString = true;
    private bool lockSelect = true;
    private const string DEFAULT_SELECT = "selection, action, input, state, eventTime, userID, levelName, attemptNumber, sessionID, transactionID";
    private string selectClause = DEFAULT_SELECT;
    public string fromClause = "";
    public string whereClause = "";
    private string otherClauses = string.Empty;

    enum Field {
        UserID = 0,
        SessionID = 1,
        TransactionID = 2,
        School =3 ,
        Age=4,
        Attempt=5,
        Gender=6,
        Time=7,
        Selection=8,
        Action=9,
        Input=10,
        Custom=11
    }

    void CommandGUI(float bWidth, float bHeight) {
        

        if (directEditCommString) {
            GUILayout.BeginHorizontal();
            GUILayout.Space(bWidth / 4);
            GUILayout.Label("SELECT",GUILayout.Width(bWidth));
            bool bak = GUI.enabled;
            GUI.enabled &= !lockSelect;
            selectClause = GUILayout.TextField(selectClause,GUILayout.MinWidth(bWidth * 3));
            GUI.enabled = bak;
            lockSelect = GUILayout.Toggle(lockSelect,new GUIContent(lockSelect ? "unlock" : "lock","Altering the SELECT clause can break everything!"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(bWidth / 4);
            GUILayout.Label("FROM", GUILayout.Width(bWidth));
            fromClause = GUILayout.TextField(fromClause, GUILayout.MinWidth(bWidth * 3));
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Space(bWidth / 4);
            GUILayout.Label("WHERE", GUILayout.Width(bWidth));
            whereClause = GUILayout.TextField(whereClause, GUILayout.MinWidth(bWidth * 3));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(bWidth / 4);
            GUILayout.Label("misc", GUILayout.Width(bWidth));
            otherClauses = GUILayout.TextField(otherClauses, GUILayout.MinWidth(bWidth * 3));
            GUILayout.EndHorizontal();
        }
        else {
            GUILayout.BeginHorizontal();
            GUILayout.Space(bWidth / 4);
            GUILayout.Label("Indirect command editing is not supported yet.");
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Space(bWidth / 4);
        //bool before = directEditConnString;
        directEditCommString = GUILayout.Toggle(directEditCommString, "Direct Edit Command String", GUILayout.Width(bWidth * 2));
        GUILayout.EndHorizontal();
    }

    #endregion ============================|--------------------------------|==============================

    #region  ==============================|       Connection Settings       |==============================

    public string ConnectionString {
        get {
            return connString;
        }
        set {
            connString = value;
        }
    }

    private string connString = "Server=127.0.0.1;Port=3306;Database=rae;Uid=root;password=password";

    private string server = "127.0.0.1";
    private string portNum = "3306";
    private string database = "rae";
    private string userID = "root";
    public string password = "password";



    private bool connOptionsOpen = false;
    private bool directEditConnString = false;
    private bool showPassword = false;

    private void ParseConnString(string conn) {
        server = string.Empty;
        portNum = string.Empty;
        database = string.Empty;
        userID = string.Empty;
        password = string.Empty;

        foreach(string ent in conn.Split(';')) {
            string[] sp = ent.Split('=');
            
            switch(sp[0].ToLower()) {
                case "server" :
                    server = sp[1];
                    break;
                case "port" :
                    portNum = sp[1];
                    break;
                case "database" :
                    database = sp[1];
                    break;
                case "uid" :
                    userID = sp[1];
                    break;
                case "password" :
                    password = sp[1];
                    break;
                default :
                    Debug.Log("Parsing connection string don't understand field: " + sp[0]);
                    break;
            }       
        }
    }
    

    private void ConnectionGUI(float bWidth, float bHeight) {
        if (directEditConnString) {
            GUILayout.BeginHorizontal();
            GUILayout.Space(bWidth / 4);
            GUILayout.Label("Connection:" ,GUILayout.Width(bWidth));
            ConnectionString = GUILayout.TextField(ConnectionString, GUILayout.MinWidth(bWidth * 6));
            GUILayout.EndHorizontal();
        }
        else {
            //The server name
            GUILayout.BeginHorizontal();
            GUILayout.Space(bWidth / 4);
            GUILayout.Label("Server Name:", GUILayout.Width(bWidth));
            server = GUILayout.TextField(server, GUILayout.MinWidth(bWidth * 2));
            GUILayout.EndHorizontal();

            //The port number
            GUILayout.BeginHorizontal();
            GUILayout.Space(bWidth / 4);
            GUILayout.Label("Port Number:", GUILayout.Width(bWidth));
            portNum = GUILayout.TextField(portNum, GUILayout.MinWidth(bWidth * 2));
            GUILayout.EndHorizontal();

            //The database name
            GUILayout.BeginHorizontal();
            GUILayout.Space(bWidth / 4);
            GUILayout.Label("Database Name:", GUILayout.Width(bWidth));
            database = GUILayout.TextField(database, GUILayout.MinWidth(bWidth * 2));
            GUILayout.EndHorizontal();

            //The user ID
            GUILayout.BeginHorizontal();
            GUILayout.Space(bWidth / 4);
            GUILayout.Label("USer ID:", GUILayout.Width(bWidth));
            userID = GUILayout.TextField(userID, GUILayout.MinWidth(bWidth * 2));
            GUILayout.EndHorizontal();

            //The password for the DB
            GUILayout.BeginHorizontal();
            GUILayout.Space(bWidth / 4);
            GUILayout.Label("Password:", GUILayout.Width(bWidth));
            if(!showPassword)
                password = GUILayout.PasswordField(password, '*', GUILayout.MinWidth(bWidth * 2));
            else
                password = GUILayout.TextField(password, GUILayout.MinWidth(bWidth * 2));
            showPassword = GUILayout.Toggle(showPassword, showPassword ? "hide" : "show");
            GUILayout.EndHorizontal();

            ConnectionString = System.String.Format("Server={0};Port={1};Database={2};Uid={3};password={4}",
                                        server,
                                        portNum,
                                        database,
                                        userID,
                                        password);
        }

        GUILayout.BeginHorizontal();
        GUILayout.Space(bWidth / 4);
        bool before = directEditConnString;
        directEditConnString = GUILayout.Toggle(directEditConnString, "Direct Edit Conncetion String", GUILayout.Width(bWidth * 2));
        GUILayout.EndHorizontal();

        if (before && !directEditConnString) {
            ParseConnString(ConnectionString);
        }
    }

    #endregion ============================|------------------------------|==============================
}
