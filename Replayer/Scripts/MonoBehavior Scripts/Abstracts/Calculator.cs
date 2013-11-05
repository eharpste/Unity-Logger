using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Calculator : RAEComponent {
	protected string currentLine = string.Empty;
	protected LogReader feeder;

    public abstract bool Ready {
        get;
    }
	
	public abstract string HeaderLine {
		get;
	}
	
	public abstract string FooterLine {
		get;
	}

    public abstract string CurrentLine {
        get;
    }

    public abstract string ScreenShotName {
        get;
    }
	
	public abstract bool AllowEmptyString {
		get;
	}
	
	public abstract void ResetScores();
	
	public abstract void CalculateScores(StudentAction action);
}

