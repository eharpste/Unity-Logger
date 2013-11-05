using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;



[RequireComponent(typeof(PrefabDictionary),typeof(ReplayGUI))]
public class ReplayAnalysisEngine : MonoBehaviour {
    #region Flags

    private bool done = true;
    private bool initialized = false;
    public bool Initialized {
        get {
            return initialized;
        }
    }
    
    private bool running;
    public bool Running {
        get {
            return running;
        }
    }

    private bool addActions;
    public bool AddActions {
        get {
            return addActions;
        }
        set {
            addActions = value;
        }
    }


    private bool takeScreenShot;
    public bool TakeScreenShots {
        get {
            return takeScreenShot;
        }
        set {
            takeScreenShot = value;
        }
    }

    #endregion

    #region Iteration Settings

    public enum IterationMode {
        FinalStates=1,
        ActionByAction=2
    }

    public IterationMode iterationMode = IterationMode.ActionByAction;

    private long numberSinceRun = 0;
    private long pauseEvery = -1;
    public long PauseAfter {
        get {
            return pauseEvery;
        }
        set {
            if (value <= 0)
                pauseEvery = -1;
            else
                pauseEvery = value;
        }
    }

    #endregion

    #region Simulation Settings

    public enum StoppingCondition {
        Instant=0,
        WaitForStop=1,
        Time=2,
        Custom=3
    }

    public StoppingCondition stopCondition = StoppingCondition.Instant;

    private float timeAcceleraton = 1.0f;

    public float TimeAcceleration {
        get {
            return timeAcceleraton;
        }
        set {
            if (value > 1f)
                timeAcceleraton = value;
            else
                timeAcceleraton = 1f;
        }
    }

    private float timeOut = 20.0f;     

    public float TimeOut {
        get {
            return timeOut;
        }
        set {
            if (value > 0)
                timeOut = value;
            else
                timeOut = float.NaN;
        }
    }

    #endregion

    #region Component Pointers

    private PrefabDictionary dictionary;
    private LogReader reader;
    private AnalysisWriter writer;
    private Calculator calculator;
    private ReplayExtender extender;

    #endregion

    #region Unity Methods
    
    void Awake() {
        Logger.Instance.Enabled = false;
    }

    // Use this for initialization
	void Start () {
        dictionary = GetComponent<PrefabDictionary>();
        
        reader = GetComponent<LogReader>();
        if(reader == null)
            Debug.LogError("No LogReader attached to the Replay Analysis Engine.");
        
        calculator = GetComponent<Calculator>();
        if (calculator == null)
            Debug.LogError("No Calculator attached to the Replay Analysis Engine.");

        writer = GetComponent<AnalysisWriter>();
        if (writer == null)
            Debug.LogError("No LogWriter attached to the Replay Analysis Engine.");

        extender = GetComponent<ReplayExtender>();
        if (extender == null)
            Debug.LogWarning("No ReplayExtender attached to the Replay Analysis Engine. Adding a DummyExtender.");
        this.gameObject.AddComponent<ReplayExtender>();

        foreach (GameObject go in (FindObjectsOfType(typeof(GameObject)) as GameObject[])) {
            if (go != this.gameObject && go.GetComponent<ReplayBehavior>() == null) {
                ReplayBehavior rb = go.AddComponent<ReplayBehavior>();
                rb.ReplayTag = ReplayBehavior.RAETag.Given;
                rb.RecordTouching = false;
            }
        }

	}

    void Update() {
        if (running && done && initialized) {
            if(!extender.SkipAction(reader.Current))
                RunState(reader.Current);
            numberSinceRun++;
            if (this.pauseEvery > 0 && numberSinceRun > this.pauseEvery) {
                running = false;
            }
            if (reader.HasNext) {
                AdvanceReader();
            }
            else {
                running = false;
            }
        }
    }


    void OnDestroy() {
        if (initialized)
            writer.Close(calculator.FooterLine);
    }

    #endregion

    #region Replay Logic

    IEnumerator Prep() {
        if (initialized) yield break;
        writer.Open(calculator.HeaderLine);
        reader.Load();
        while (!writer.Opened && !calculator.Ready && !reader.Loaded && !extender.Ready) {
            running = false;
            yield return 0;
        }
        initialized = true;
        yield break;
    }

    private void AdvanceReader() {
        if (reader.HasNext) {
            switch (iterationMode) {
                case IterationMode.ActionByAction:
                    reader.GetNextStudentAction();
                    break;
                case IterationMode.FinalStates:
                    reader.GetNextStudentAction();
                    while (reader.HasNext && reader.Current.IsSameAttempt(reader.Previous)) {
                        reader.GetNextStudentAction();
                    }
                    Debug.Log("Current: " + reader.Current);
                    break;
            }
        }
        
    }

    public void RunNextAction() {
        if (initialized && done) {
            if (reader.HasNext) {
                reader.GetNextStudentAction();
                if (reader.IsNewAttempt) {
                    extender.OnNewLevel(reader.LastStartState);
                }
                if (!extender.SkipAction(reader.Current))
                    RunState(reader.Current);
            }
        }
        else {
            StartCoroutine(Prep());
        }
    }

    public void Run() {
        if (running)
            return;
        if (!initialized)
            StartCoroutine(Prep());
        running = reader.HasNext;
        numberSinceRun = 0;
    }

    public void Pause() {
        running = false;
    }

    public void RunState(StudentAction action) {
        //Debug.Log(action);
        Time.timeScale = 1.0f;
        done = false;
        calculator.ResetScores();
        extender.OnActionPre(action);
        ClearState();
        ClearAction();
        InstantiateState(action);
        if (addActions)
            InstantiateAction(action);
        extender.OnActionPost(action);
        Time.timeScale = timeAcceleraton;     
        StartCoroutine(WaitForStop(action));
    }


    IEnumerator WaitForStop(StudentAction action) {
        List<GameObject> state = new List<GameObject>(ReplayBehavior.GetGameObjectsWithRAETag(ReplayBehavior.RAETag.State));
        state.AddRange(ReplayBehavior.GetGameObjectsWithRAETag(ReplayBehavior.RAETag.Action));
        float callTime = Time.time;
        switch (stopCondition) {
            case StoppingCondition.Instant:
                break;
            case StoppingCondition.WaitForStop:
                bool allSleeping = false;
                while (!allSleeping) {
                    allSleeping = true;
                    if (Time.time - callTime > timeOut) {
                        break;
                    }

                    foreach (GameObject go in state) {
                        if (go != null && go.rigidbody != null & !go.rigidbody.IsSleeping()) {
                            allSleeping = false;
                            yield return null;
                            break;
                        }
                    }
                }
                break;
            case StoppingCondition.Time :
                while (Time.time - callTime < timeOut) {
                    yield return null;
                }
                break;
            case StoppingCondition.Custom :
                yield return extender.StoppingCoroutine();
                break;

        }
        Stop(action);
    }

    private void Stop(StudentAction action) {
        Time.timeScale = 0f;
        extender.OnStop(action);
        //calculate
        calculator.CalculateScores(action);
        //screenshot
        if (takeScreenShot) {
            writer.CaptureScreenShotPNG(calculator.ScreenShotName);
        }
        //record
        writer.Write(calculator.CurrentLine);
        done = true;
    }

    private void InstantiateStartState(StudentAction startState) {
        if (startState.Equals(StudentAction.NullAction) ||  startState.Action != Logger.START_STATE)
            return;
        ClearState();
        ClearAction();
        ClearStartState();
        if (startState.StateXML != null) {
            XmlNodeList objects = startState.StateXML.SelectNodes(Logger.OBJECT);
            foreach (XmlNode entry in objects) {
                GameObject gob = SpawnObject(entry, startState);
                if (gob != null) {
                    gob.GetComponent<ReplayBehavior>().ReplayTag = ReplayBehavior.RAETag.StartState;
                    extender.SpecializeNewObject(gob, entry, startState);
                }
            }
        }
    }

    private void InstantiateState(StudentAction action) {
        if (action.Equals(StudentAction.NullAction))
            return;
        ClearState();
        ClearAction();
        if (action.StateXML != null) {
            XmlNodeList objects = action.StateXML.SelectNodes(Logger.OBJECT);
            foreach (XmlNode entry in objects) {
                if (entry[Logger.NAME].InnerText != action.Selection) {
                    GameObject gob = SpawnObject(entry, action);
                    if (gob != null) {
                        gob.GetComponent<ReplayBehavior>().ReplayTag = ReplayBehavior.RAETag.State;
                        extender.SpecializeNewObject(gob, entry, action);
                    }
                }
            }
        }
    }

    private void InstantiateAction(StudentAction action) {
        if (action.Equals(StudentAction.NullAction))
            return;
        if (!action.HasObjectInput())
            return;
        
        //Debug.Log(action);

        XmlNodeList objects = action.InputXML.SelectNodes(Logger.OBJECT);
        if (objects.Count == 0) {
            return;
        }
        else foreach (XmlNode node in objects) {
            GameObject gob = SpawnObject(node, action);
            if (gob != null) {
                gob.GetComponent<ReplayBehavior>().ReplayTag = ReplayBehavior.RAETag.Action;
                extender.SpecializeNewObject(gob, node, action);
            }
        }
    }

    private GameObject SpawnObject(XmlNode node, StudentAction action) {
        string key = extender.LookupPrefab(node, action);

        if (string.IsNullOrEmpty(key)) {
            if (node[Logger.PREFAB] == null) {
                Debug.LogWarning("XmlNode contains no <Prefab> element, attempting to use the <Name> element instead.");
                if (node[Logger.NAME] == null) {
                    Debug.LogWarning("XmlNode contains no <Name> element. Giving up. Consider writing an extension to the LookupPrefab() "+ 
                    "method in the ReplayExtender class if your log files do not conform to the standard format.\nNode:" + node.OuterXml);
                    return null;
                }
                key = node[Logger.NAME].InnerText;
            }
            else {
                key = node[Logger.PREFAB].InnerText;
            }
        }

        GameObject prefab = dictionary.GetPrefab(key);

        if (prefab == null) {
            Debug.LogWarning("No Prefab found in dictionary for name:" + key + ", did you forget to add it, or spell the (case-sensative) key wrong? Giving up.");
            return null;
        }
  
        GameObject gob = null;

        float posX = float.Parse(node[Logger.TRANSFORM][Logger.POSITION]["X"].InnerText);
        float posY = float.Parse(node[Logger.TRANSFORM][Logger.POSITION]["Y"].InnerText);
        Vector3 pos = new Vector3(posX, posY, 0f);
        
        float rotZ = float.Parse(node[Logger.TRANSFORM][Logger.ROTATION].InnerText);
        Quaternion rot = Quaternion.Euler(0f, 0f, rotZ);

        gob = GameObject.Instantiate(prefab, pos, rot) as GameObject;

        gob.name = node[Logger.NAME].InnerText;
        ReplayBehavior rb = gob.AddComponent<ReplayBehavior>();
        
        if (node[Logger.PREFAB] != null)
            rb.PrefabName = node[Logger.PREFAB].InnerText;
        else
            rb.PrefabName = ReplayBehavior.NO_PREFAB_TAG;

        if (node[Logger.UNITY_TAG] != null)
            rb.UnityTag = node[Logger.UNITY_TAG].InnerText;
        else
            rb.UnityTag = ReplayBehavior.NO_UNITY_TAG;

        if (node[Logger.EXTRA_TAGS] != null) {
            List<string> newTags = new List<string>();
            foreach(XmlNode subNode in node[Logger.EXTRA_TAGS]) {
                if (subNode[Logger.TAG] != null) {
                    newTags.Add(subNode[Logger.TAG].InnerText);
                }
            }
            rb.AddTags(newTags);
        }

        return gob;
    }

    private void ClearStartState() {
        extender.ClearStartState();
        foreach (GameObject go in ReplayBehavior.GetGameObjectsWithRAETag(ReplayBehavior.RAETag.StartState)) {
            GameObject.Destroy(go);
        }
    }

    private void ClearState() {
        extender.ClearState();
        foreach (GameObject go in ReplayBehavior.GetGameObjectsWithRAETag(ReplayBehavior.RAETag.State)) {
            GameObject.Destroy(go);
        }
    }

    private void ClearAction() {
        extender.ClearAction();
        foreach (GameObject go in ReplayBehavior.GetGameObjectsWithRAETag(ReplayBehavior.RAETag.Action)) {
            GameObject.Destroy(go);
        }
    }
    #endregion
}
