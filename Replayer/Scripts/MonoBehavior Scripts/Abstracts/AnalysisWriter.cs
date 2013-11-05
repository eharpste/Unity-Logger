using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class AnalysisWriter : RAEComponent {
    public abstract bool Opened {get;}
    
    public abstract bool CaptureScreenShots { get; set; }

    //TODO - capture a screenshot in the current writing directory
    public abstract void CaptureScreenShotPNG(string name);

    public abstract void Open(string header);

    public abstract void Close(string footer);

    public abstract void Write(string line);

}
