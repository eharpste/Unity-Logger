using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class HttpWriter : ILogWriter
{
	private enum ReadyState {
		Initializing,
		Ready,
		TimedOut
	}
	
    /// <summary>
    /// Shared secret is a string which is known by both the web service and RumbleBlocks
    /// and constitutes a very weak security measure for preventing the logging service
    /// from being spammed.  Since it is sent in the clear, it can be easily gleaned using
    /// a wire sniffer.  If we use HTTPS, we gain some protection here, but also have SSL
    /// overhead and certificate issues to deal with - always a problem on Apple devices.
    /// </summary>
    private const string SHARED_SECRET = "UgkLuhakJbBSAczdSisJHdn0PM02mK6W";	
	
	/// <summary>
	/// TBD: Need a configuration parameter here.
	/// </summary>
	private const string LOGGING_SERVICE_URL = "http://localhost";
	
	/// <summary>
	/// The number of seconds the system will continue to queue messages before
	/// giving up on the server.
	/// </summary>
	private float timeout = 10.0f;
	
	/// <summary>
	/// WWW instance used to check for communications readiness.
	/// </summary>
	private WWW hellowWorld;
	
	/// <summary>
	/// Indicates the current state of the HttpWriter
	/// </summary>
	private ReadyState state = ReadyState.Initializing;
	
	/// <summary>
	/// Holds messages waiting to be sent to the log server.
	/// </summary>
	private Queue<string> messageQueue = new Queue<string>();
	
	public HttpWriter (){
		// SOAP Service expects a FORM payload even if no arguments need to be supplied.
		WWWForm form = new WWWForm();
		form.AddField("", "");
		
		// Dispatch a call to HelloWorld - we check for response later.
		this.hellowWorld = new WWW("http://localhost:6253/LoggingService.asmx/HelloWorld", form);
		
		// Set timeout counter to a point in the future.
		this.timeout += Time.time;
	}
	
	/// <summary>
	/// Writes the given message to the web service. 
	/// </summary>
	/// <param name="message">The string to be written to the web service.</param>
	public void Write(string message) {
		
		// If we already timed out, just bailout.
		if (this.state == ReadyState.TimedOut)
			return;
		
		if (this.state == ReadyState.Initializing) {
			// Ordering is critical, must check if isDone before checking timeout
			// in case a large amount of time has elapsed between calls.
			if (this.hellowWorld.isDone) {
				while (this.messageQueue.Count > 0) {				
					this.SendMessage(this.messageQueue.Dequeue());
				}
				this.state = ReadyState.Ready;
			} else if (Time.time > this.timeout) {
				this.state = ReadyState.TimedOut;
			} else {
				this.messageQueue.Enqueue(message);				
			}
		}
		
		if (this.state == ReadyState.Ready) {
			this.SendMessage(message);
		}
	}		
	
	/// <summary>
	/// Handles the actual HTTP message transmission.
	/// </summary>
	/// <param name='message'>
	/// The message to be sent.
	/// </param>
	private void SendMessage(string message) {
		WWWForm form = new WWWForm();
		form.AddField("secret", HttpWriter.SHARED_SECRET);
		form.AddField("message", message);
		new WWW(LOGGING_SERVICE_URL + "/LogString", form);
	}
}

