using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;


public class TextFileWriter : AnalysisWriter {
    private TextWriter writer = null;
    
    private string logPath = "";
    private string screenShotPath = "";
    private string logFileName = "";

    private bool allowShots = true;


    private ReplayGUI mainGUI;
    private Calculator calculator;


    public override bool Opened {
        get { return writer != null || !writingEnabled; }
    }

    public override bool CaptureScreenShots {
        get {
            return allowShots;
        }
        set {
            this.allowShots = value;
        }
    }


    void Start() {
        mainGUI = GetComponent<ReplayGUI>();
        calculator = GetComponent<Calculator>();
    }

    void OnDestroy() {
        this.Close(calculator.FooterLine);
    }
    
    //TODO - capture a screenshot in the current writing directory
    public override void CaptureScreenShotPNG(string name) {
        if (CaptureScreenShots && writingEnabled) {
            string filePath = screenShotPath + "/" + name + ".png";
            if (!File.Exists(filePath))
                mainGUI.CaptureScreenShot(filePath);
        }
    }




    public override void Open(string header) {
        if (!writingEnabled) return;
        if (string.IsNullOrEmpty(logPath)) {
            logPath = Application.dataPath;
        }
        try {
            this.writer = new StreamWriter(logPath + "/"+logFileName);
        }
        catch {
            this.writer = null;
        }

        this.WriteLine(header);
    }

    public override void Close(string footer) {
        if (this.writer != null && writingEnabled) {
            this.WriteLine(footer);
            this.writer.Close();
        }
    }

    public override void Write(string line) {
        if(writingEnabled)
            this.WriteLine(line);
    }

    private void WriteLine(string line) {

        if (writingEnabled && this.writer != null) {
            if(this.calculator.AllowEmptyString) {
                this.writer.WriteLine(line);
                this.writer.Flush();
            }
            else if (!string.IsNullOrEmpty(line)) {
                this.writer.WriteLine(line);
                this.writer.Flush();
            }
        }
    }

    private Vector2 scrollPos = Vector2.zero;
    private bool screenShotTouched = false;
    private bool writingEnabled = true;

    public override void OptionsPane(Rect layoutRect) {
        float bWidth = mainGUI.standardButtonWidth;
        float bHeight = mainGUI.standardButtonHeight;
        
        GUILayout.BeginArea(layoutRect);
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.BeginVertical();

        writingEnabled = GUILayout.Toggle(writingEnabled,"Enable Writing", GUILayout.Width(bWidth * 2), GUILayout.Height(bHeight));
        bool bak = GUI.enabled;
        GUI.enabled &= writingEnabled;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Output File Name:", GUILayout.Width(bWidth));
        logFileName = GUILayout.TextField(logFileName, GUILayout.Width(bWidth * 4));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Output Directory:", GUILayout.Width(bWidth));
        logPath = GUILayout.TextField(logPath, GUILayout.Width(bWidth * 4)).Trim('"');
        if (!screenShotTouched)
            screenShotPath = logPath;
        GUILayout.EndHorizontal();

        if (logPath != string.Empty && !Directory.Exists(logPath)) {
            GUILayout.Label("The log path does not exist would you like to create it?", GUILayout.Width(bWidth * 4));
            GUILayout.BeginHorizontal();
            GUILayout.Space(bWidth / 2);
            if(GUILayout.Button("Yes",GUILayout.Width(bWidth))) {
                Directory.CreateDirectory(logPath);
            }
            GUILayout.Space(bWidth);
            if(GUILayout.Button("No",GUILayout.Width(bWidth))) {
                logPath = string.Empty;
            }
            GUILayout.EndHorizontal();
        }

        this.CaptureScreenShots = GUILayout.Toggle(CaptureScreenShots, "Take Screen Shots");

        bool guiBak = GUI.enabled;

        GUI.enabled &= CaptureScreenShots;

        string temp = screenShotPath;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Screenshot Directory:", GUILayout.Width(bWidth));
        screenShotPath = GUILayout.TextField(screenShotPath, GUILayout.Width(bWidth * 4));
        if (!screenShotTouched && screenShotPath != temp)
            screenShotTouched = true;
        GUILayout.EndHorizontal();

        if (screenShotTouched && logPath != string.Empty && !Directory.Exists(screenShotPath)) {
            GUILayout.Label("The log path does not exist would you like to create it?", GUILayout.Width(bWidth * 4));
            GUILayout.BeginHorizontal();
            GUILayout.Space(bWidth / 2);
            if (GUILayout.Button("Yes", GUILayout.Width(bWidth))) {
                Directory.CreateDirectory(screenShotPath);
            }
            GUILayout.Space(bWidth);
            if (GUILayout.Button("No", GUILayout.Width(bWidth))) {
                screenShotPath = logPath;
            }
            GUILayout.EndHorizontal();
        }

        GUI.enabled = guiBak;
        GUI.enabled = bak;
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    public override string GUIName {
        get { return "CSV Output Options"; }
    }

    private string FixPath(string path) {
        return path.Trim('"');
    }
}