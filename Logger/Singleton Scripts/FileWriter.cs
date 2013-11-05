using UnityEngine;
using System;
using System.IO;

/// <summary>
/// Concrete implementation of the ILogWriter interface which writes
/// log entries to a file on the local filesystem.  The log directory
/// is hard coded to be /Logs relative to the application's persistent
/// data pate.  This should be more compatible with Android devices.
/// </summary>
/// Change History:
/// 2013/02/07: Log file uses persistent data path for Android compatibility.
/// 2012/11/08: Added comments.
/// </remarks>
public class FileWriter : ILogWriter
{
	/// <summary>
	/// The text stream writer used to log streams to file. 
	/// </summary>
	private TextWriter writer;
	
	public FileWriter ()
	{
		string logPath = Application.persistentDataPath + "/Logs";
		// Check if output directory exists
		if (!Directory.Exists("Logs")) {
			Directory.CreateDirectory("Logs");
		}
		
		string logFile = String.Format("{0}/{1:yyyy_MM_dd_HH_mm_ss}.log", logPath, System.DateTime.Now);
		
		try {
			this.writer = new StreamWriter(logFile);
		} catch (Exception ex) {
			Debug.Log("The following error occurred while attempting to open the log file: " + logFile);
			Debug.Log(ex.Message);
			this.writer = null;
		}				
	}
			
	/// <summary>
	/// Flushes the log buffer and closes the stream. 
	/// </summary>
	~FileWriter() {
		if (this.writer != null) {
			this.writer.WriteLine("</Log>");
			this.writer.Flush();
			this.writer.Close();
		}
	}
	
	/// <summary>
	/// Writes the given message to the log file. 
	/// </summary>
	/// <param name="message">The string to be written to the log file.</param>
	public void Write(string message) {
		if (this.writer != null) {
			Debug.Log(">>> FileWriter.Write('" + message + "'");
			this.writer.WriteLine(message);
			this.writer.Flush();
		}
	}
}

