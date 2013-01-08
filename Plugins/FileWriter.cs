using UnityEngine;
using System;
using System.IO;

public class FileWriter : ILogWriter
{
	/// <summary>
	/// The text stream writer used to log streams to file. 
	/// </summary>
	private TextWriter writer;
	
	public FileWriter ()
	{
		// Check if output directory exists
		if (!Directory.Exists("Logs")) {
			Directory.CreateDirectory("Logs");
		}
		
		string logFile = String.Format("Logs/{0:yyyy_MM_dd_HH_mm_ss}.log", System.DateTime.Now);
		
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

