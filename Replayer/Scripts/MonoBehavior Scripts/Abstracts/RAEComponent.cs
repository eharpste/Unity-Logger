using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ReplayAnalysisEngine))]
public abstract class RAEComponent : MonoBehaviour {

    public abstract string GUIName { get; }

    public abstract void OptionsPane(Rect optionArea);
}
