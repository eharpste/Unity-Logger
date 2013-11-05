using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


public class ReplayGUI : MonoBehaviour {

    #region Flags

    public bool Showing {
        get {
            return guiUp;
        }
        set {
            guiUp = value;
        }
    }

    public bool ScreenshotMode {
        get {
            return takingScreenShot;
        }
        set {
            this.takingScreenShot = value;
            guiUp = !value;
        }
    }

    bool takingScreenShot = false;
    bool guiUp = true;
    bool optionUp = false;

    #endregion

    #region Size and Layout Properties

    public float standardButtonWidth = 100;
    public float standardButtonHeight = 25;

    private float originX;
    public  float gameWidth = float.NaN;
    public float gameHeight = float.NaN;
    public float borderSpacing = 5;

    private Rect overalOptionsRect;
    private Rect optionsLabelRect;
    private Rect optionToolbarRect;
    private Rect mainOptionsRect;

    #endregion

    #region FieldValues

    private List<string> optionNames = new List<string>();
    private int currentOption = 0;
    private Vector2 scrollPosition = Vector2.zero;

    #endregion 

    #region Component Pointers

    LogReader reader;
    AnalysisWriter writer;
    Calculator calulator;
    ReplayAnalysisEngine rae;
    ReplayExtender extender;

    #endregion

    void Start() {
        reader = GetComponent<LogReader>();
        writer = GetComponent<AnalysisWriter>();
        calulator = GetComponent<Calculator>();
        rae = GetComponent<ReplayAnalysisEngine>();
        extender = GetComponent<ReplayExtender>();
        
        gameWidth = float.IsNaN(gameWidth) ?  Screen.height * Camera.mainCamera.aspect : gameWidth;
        originX = (Screen.width - gameWidth) / 2f;
        gameHeight = float.IsNaN(gameHeight) ? Screen.height : gameHeight;

        overalOptionsRect = new Rect(originX, 0, gameWidth, gameHeight);
        optionsLabelRect = new Rect(overalOptionsRect.x + overalOptionsRect.width / 2 - standardButtonWidth / 2,
            overalOptionsRect.y + borderSpacing,
            standardButtonWidth,
            standardButtonHeight);
        optionToolbarRect = new Rect(originX + borderSpacing,
            optionsLabelRect.yMax + borderSpacing,
            overalOptionsRect.width - 2 * borderSpacing,
            standardButtonHeight);

        mainOptionsRect = new Rect(overalOptionsRect.x + borderSpacing,
            optionToolbarRect.yMax + borderSpacing,
            overalOptionsRect.width - 2 * borderSpacing,
            overalOptionsRect.yMax - optionToolbarRect.yMax - 2 * borderSpacing - standardButtonHeight);

        optionNames.Add("Replay Options");
        optionNames.Add(reader.GUIName);
        optionNames.Add(writer.GUIName);
        optionNames.Add(calulator.GUIName);
        optionNames.Add(extender.GUIName);
    }

    void OnGUI() {
        if (guiUp) {
            if (optionUp)
                OptionsGUI();
            MainGUI(!optionUp);
        }
        else if(takingScreenShot) {
            InfoBox();
        }
    }

    /// <summary>
    /// Draws the main runtime GUI.
    /// </summary>
    void MainGUI(bool enabled) {
        bool bak = GUI.enabled;
        GUI.enabled = enabled;
        GUILayout.BeginArea(new Rect(originX, gameHeight - standardButtonHeight, gameWidth, standardButtonHeight));
        GUILayout.BeginHorizontal();
        GUI.enabled = bak;
        if (GUILayout.Button(optionUp ? "OK" : "Options", GUILayout.Width(standardButtonWidth), GUILayout.Height(standardButtonHeight))) {
            optionUp = !optionUp;
        }
        GUI.enabled = enabled;
        if (GUILayout.Button(rae.Initialized ? "Next Action" : "Initialize", GUILayout.Width(standardButtonWidth), GUILayout.Height(standardButtonHeight))) {
            rae.RunNextAction();
            /*if (!reader.Loaded) {
                reader.Load();
            }
            StudentAction p = reader.GetNextStudentAction();
            rae.RunState(p);*/
        }
        if (GUILayout.Button(rae.Running ? "Pause" : "Run", GUILayout.Width(standardButtonWidth), GUILayout.Height(standardButtonHeight))) {
            if (rae.Running)
                rae.Pause();
            else
                rae.Run();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUI.enabled = bak;
    }

    /// <summary>
    /// Draws the additional options GUI.
    /// </summary>
    void OptionsGUI() {
        //draw the otherall rectangle
        GUI.Box(overalOptionsRect,"");
        GUI.Box(overalOptionsRect, "");
        GUI.Box(overalOptionsRect, "");
        //draw the "Options" label
        GUI.Label(optionsLabelRect, "Options");
        //draw the toolbar
        currentOption = GUI.Toolbar(optionToolbarRect,currentOption, this.optionNames.ToArray<string>());

        switch (currentOption) {
            case 0 :
                StandardOptions();
                break;
            case 1 :
                FilterOptions();
                break;
            case 2 :
                WriterOptions();
                break;
            case 3 :
                CalculatorOptions();
                break;
            case 4 :
                ExtenderOptions();
                break;
            default :
                Debug.LogError("No behavior defined for option:" + optionNames[currentOption]);
                break;
        }
    }

    private Regex floatRegEx = new Regex(@"\d*\.?\d*?");
    private Regex intRegEx = new Regex(@"\d*");
    private float FormatFloat(string s) {
        return float.Parse(floatRegEx.IsMatch(s) ? s : "0");
    }

    private int FormatInt(string s) {
        if (string.IsNullOrEmpty(s))
            return 0;
        return int.Parse(intRegEx.IsMatch(s) ? s : "0");
    }

    void StandardOptions() {
        GUILayout.BeginArea(mainOptionsRect);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Stopping Mode", GUILayout.Width(standardButtonWidth));
        if (GUILayout.Button(rae.stopCondition.ToString(), GUILayout.Width(standardButtonWidth))) {
            rae.stopCondition++;
            if (rae.stopCondition > ReplayAnalysisEngine.StoppingCondition.Custom)
                rae.stopCondition = ReplayAnalysisEngine.StoppingCondition.Instant;
        }
        GUILayout.EndHorizontal();

        switch (rae.stopCondition) {
            case ReplayAnalysisEngine.StoppingCondition.Instant :
                break;
    
            case ReplayAnalysisEngine.StoppingCondition.WaitForStop :
            case ReplayAnalysisEngine.StoppingCondition.Time :
            case ReplayAnalysisEngine.StoppingCondition.Custom :
                GUILayout.BeginHorizontal();
                GUILayout.Label("Time Acceleration",GUILayout.Width(standardButtonWidth));
                rae.TimeAcceleration = FormatFloat(GUILayout.TextField(rae.TimeAcceleration+"",GUILayout.Width(standardButtonWidth)));
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Time Out After",GUILayout.Width(standardButtonWidth));
                rae.TimeOut = FormatFloat(GUILayout.TextField(rae.TimeOut+"",GUILayout.Width(standardButtonWidth)));
                GUILayout.EndHorizontal();
                break;
        }



        rae.AddActions = GUILayout.Toggle(rae.AddActions,"Add Action Objects",GUILayout.Width(standardButtonWidth *1.5f));
        //rae.TakeScreenShots = GUILayout.Toggle(rae.TakeScreenShots,"Take Screen Shots",GUILayout.Width(standardButtonWidth * 1.5f));
        //maybe put some options in here
        GUILayout.BeginHorizontal();
        GUILayout.Label("Iteration Mode",GUILayout.Width(standardButtonWidth));
        if(GUILayout.Button(rae.iterationMode.ToString(),GUILayout.Width(standardButtonWidth))) {
            rae.iterationMode++;
            if (rae.iterationMode > ReplayAnalysisEngine.IterationMode.ActionByAction)
                rae.iterationMode = ReplayAnalysisEngine.IterationMode.FinalStates;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Pause After",GUILayout.Width(standardButtonWidth));
        string temp = GUILayout.TextField(rae.PauseAfter < 0 ? "Don't Pause" : rae.PauseAfter+"",GUILayout.Width(standardButtonWidth));
        if(temp != "Don't Pause")
            rae.PauseAfter = FormatInt(temp);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();

    }

    void FilterOptions() {
        if (reader != null) {
            reader.OptionsPane(mainOptionsRect);
        }
        else {
            GUILayout.BeginArea(mainOptionsRect);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("No LogReader Options Available");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }

    void CalculatorOptions() {
        if (calulator != null)
            calulator.OptionsPane(mainOptionsRect);
        else {
            GUILayout.BeginArea(mainOptionsRect);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("No Calculator Options Available");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }


    void WriterOptions() {
        if (writer != null)
            writer.OptionsPane(mainOptionsRect);
        else {
            GUILayout.BeginArea(mainOptionsRect);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("No Writer Options Available");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }

    void ExtenderOptions() {
        if (extender != null)
            extender.OptionsPane(mainOptionsRect);
        else {
            GUILayout.BeginArea(mainOptionsRect);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("No Extension Options Available");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }


    void InfoBox() {
        GUILayout.BeginArea(new Rect(originX,0,standardButtonWidth*2,standardButtonHeight*4));
        GUILayout.BeginVertical();
        GUILayout.Label("User: "+reader.Current.User,GUILayout.Width(standardButtonWidth));
        GUILayout.Label("Level: "+reader.Current.LevelName,GUILayout.Width(standardButtonWidth));
        GUILayout.Label("Attempt: "+reader.Current.Attempt,GUILayout.Width(standardButtonWidth));
        GUILayout.Label("Transaction ID: "+reader.Current.TransactionID,GUILayout.Width(standardButtonWidth));
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    public void CaptureScreenShot(string filePath) {
        StartCoroutine(ScreenShotCoroutine(filePath));
    }

    IEnumerator ScreenShotCoroutine(string filePath) {
        this.ScreenshotMode = true;
        yield return new WaitForEndOfFrame();
        Application.CaptureScreenshot(filePath);
        this.ScreenshotMode = false;
        yield break;
    }

}

