using System;

/// <summary>
/// Base class implementation for log writers.  
/// </summary>
public interface ILogWriter
{
	/// <summary>
	/// Writes the given message to the log stream. 
	/// </summary>
	/// <param name="message">The string to be written to the log stream.</param>
	void Write(string message);
}

