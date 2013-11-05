using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;

public class ReplayExtender : RAEComponent {
    ReplayGUI mainGUI;

    void Start() {
        mainGUI = GetComponent<ReplayGUI>();
    }

    public virtual bool Ready { get { return true; } }


    /// <summary>
    /// This method will be called by the ReplayAnalysisEngine whenever the RAE's
    /// ClearState method is called. The execution order calls this method FIRST
    /// before the RAE performs its own State deletion.
    /// </summary>
    public virtual void ClearState() { }

    /// <summary>
    /// This method will be called by the ReplayAnalysisEngine whenever the RAE's
    /// ClearAction method is called. The execution order calls this method FIRST
    /// before the RAE performs its own Action deletion.
    /// </summary>
    public virtual void ClearAction() { }


    public virtual void ClearStartState() { return; }

    /// <summary>
    /// This method is used to do any particular specialization that may be necessary for your game.
    /// It will be called after the Prefab of the GameObject is instantiated. The method should 
    /// return the GameObject post-specialization. It is guaranteed that this method will never be 
    /// passed a <code>null</code> GameObject.
    /// </summary>
    /// <param name="gob">The instantiated GameObject as read from the log file.</param>
    /// <param name="node">The node from the log file that describes the GameObject.</param>
    /// <param name="action">The StudentAction that contains the current action and state information.</param>
    /// <returns></returns>
    public virtual GameObject SpecializeNewObject(GameObject gob, XmlNode node, StudentAction action) { return gob; }


    public virtual IEnumerator StoppingCoroutine() { yield break; }

    public virtual void OnActionPre(StudentAction action) { }
    
    public virtual void OnActionPost(StudentAction action) { }

    public virtual void OnStop(StudentAction action) { }

    public virtual void OnNewLevel(StudentAction startState) { }

    public virtual string LookupPrefab(XmlNode objectNode, StudentAction action) { return null; }

    //protected List<string> seeActionTypes = new List<string>();
    protected List<string> skipActionTypes = new List<string>();
    protected OrderedDictionary seeActionTypes = new OrderedDictionary();

    public virtual bool SkipAction(StudentAction action) {
        if (action != null && action.Action != null) {
            if (seeActionTypes.Contains(action.Action)) {
                int holder = ((int)seeActionTypes[action.Action]);
                holder += 1;
                seeActionTypes[action.Action] = holder;
            }
            else {
                seeActionTypes.Add(action.Action, 1);
            }
        }
        return skipActionTypes.Contains(action.Action);
    
    }

    private Vector2 scrollPos = Vector2.zero;
    private float charButtonWidth = 24;

    public override void OptionsPane(Rect layoutRect) {
        if (mainGUI == null) {
            mainGUI = GetComponent<ReplayGUI>();
        }


        GUILayout.BeginArea(layoutRect);
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.BeginVertical();
        GUILayout.Label("Action Filtering");
        
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        int removeDex = -1;
        for (int i = 0; i < skipActionTypes.Count; i++) {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.Width(charButtonWidth), GUILayout.Height(mainGUI.standardButtonHeight))) {
                removeDex = i;
            }
            skipActionTypes[i] = GUILayout.TextField(skipActionTypes[i],GUILayout.Width(mainGUI.standardButtonWidth*2),GUILayout.Height(mainGUI.standardButtonHeight));
            GUILayout.EndHorizontal();
        }
        if(removeDex >= 0) {
            skipActionTypes.RemoveAt(removeDex);
        }

        GUILayout.Space(mainGUI.standardButtonHeight/2);
        
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("+", GUILayout.Width(charButtonWidth), GUILayout.Height(mainGUI.standardButtonHeight))) {
            AddSkipActionType(NEW_TYPE);
        }
        bool bak = GUI.enabled;
        GUI.enabled = false;
        GUILayout.TextField(NEW_TYPE, GUILayout.Width(mainGUI.standardButtonWidth * 3), GUILayout.Height(mainGUI.standardButtonHeight));
        GUI.enabled = bak;
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUILayout.Space(mainGUI.standardButtonWidth / 2);

        GUILayout.BeginVertical();
        if (seeActionTypes.Count == 0) {
            GUILayout.Label("No Actions Seen Yet.", GUILayout.Width(mainGUI.standardButtonWidth * 3), GUILayout.Height(mainGUI.standardButtonHeight));
        }
        else {
            foreach (string s in seeActionTypes.Keys) {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(charButtonWidth), GUILayout.Height(mainGUI.standardButtonHeight))) {
                    AddSkipActionType(s);
                }
                GUILayout.Label(s + " : "+seeActionTypes[s], GUILayout.Width(mainGUI.standardButtonWidth *3), GUILayout.Height(mainGUI.standardButtonHeight));
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    public override string GUIName {
        get { return "Filter Options"; }
    }

    private const string NEW_TYPE = "**Add New Type**";

    private void AddSkipActionType(string type) {
        foreach (string s in skipActionTypes) {
            if (s == type && type != NEW_TYPE) {
                return;
            }
        }
        skipActionTypes.Add(type);
    }
}

