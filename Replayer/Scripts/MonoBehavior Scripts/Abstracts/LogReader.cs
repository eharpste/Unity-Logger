using UnityEngine;

public abstract class LogReader  : RAEComponent {
    public abstract StudentAction Current {get;}
    public abstract StudentAction Previous{get;}
    public abstract StudentAction Next { get; }
    public abstract StudentAction LastStartState { get; }

    public abstract bool IsNewAttempt { get; }
    public abstract bool Loaded { get; }
    public abstract bool HasNext { get; }

    public abstract void Load();

    public abstract StudentAction GetNextStudentAction();
}

