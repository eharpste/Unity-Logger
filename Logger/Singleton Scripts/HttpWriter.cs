using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Concrete implementation of the ILogWriter interface which writes
/// log entries to a [remote] web service via HTTP POST protocol.
/// </summary>
/// Change History:
/// 2013/07/17: Upgrade to Unity 4 broke logging, replaced body of code with
///             Beanstalk logger which is based on working code model from
///             the CS2N API.
/// 2013/03/04: Fixed bug in HelloWorld URL
/// 2012/11/08: Added/updated comments.
/// </remarks>
public class HttpWriter : ILogWriter
{
	private enum ReadyState {
		/// <summary>
		/// Starting state
		/// </summary>
		Initializing,
		/// <summary>
		/// Called logging service, waiting for reply
		/// </summary>
		WakingServiceUp,
		/// <summary>
		/// Logging service replied to initial wake up call.
		/// </summary>
		CommunicationVerified,
		/// <summary>
		/// HttpWriter is ready for logging.
		/// </summary>
		Ready,
		/// <summary>
		/// Logging service failed to reply to wake up call, log writing disabled.
		/// </summary>
		TimeoutGivingUp
	}
	
    /// <summary>
    /// The shared secret is a string which is known by both the remote web service and 
    /// the local application intended to prevent casual spamming of the web service
    /// via drive-by attacks.  Since it is sent in the clear, it can be easily gleaned 
    /// using a wire sniffer.  If we use HTTPS, we gain some protection here, but also 
    /// have SSL overhead and certificate issues to deal with.
    /// </summary>
    private const string SHARED_SECRET = "UgkLuhakJbBSAczdSisJHdn0PM02mK6W";	
	
	/// <summary>
	/// TBD: Need a way to supply this via configuration file.
	/// </summary>
	private const string LOGGING_SERVICE_URL = "http://rumbleblocks.etc.cmu.edu/Logger/LoggingService.asmx";
	
	/// <summary>
	/// The number of seconds the system will queue messages before
	/// assuming the server will be able to handle them.
	/// </summary>
	private float timeout = 10.0f;
	
	/// <summary>
	/// WWW instance used to check for communications readiness.
	/// </summary>
	private WWW helloWorld;
	
	/// <summary>
	/// Indicates the current state of the HttpWriter
	/// </summary>
	private ReadyState state = ReadyState.Initializing;
	
	/// <summary>
	/// Holds messages waiting to be sent to the log server.
	/// </summary>
	private Queue<string> messageQueue = new Queue<string>();
	
	public HttpWriter (){
	}
	
	/// <summary>
	/// Writes the given message to the web service. 
	/// </summary>
	/// <param name="message">The string to be written to the web service.</param>
	public void Write(string message) {		
		
		if (this.state == ReadyState.Initializing) {
			WakeUpService();
			this.state = ReadyState.WakingServiceUp;
		}
		
		if (this.state == ReadyState.WakingServiceUp) {
			if (Time.time > this.timeout) {
				// Drat!  Didn't hear back from service, so give up and don't queue
				// messages further, clear queue, don't log, don't try to recover.
				this.state = ReadyState.TimeoutGivingUp;
				this.messageQueue.Clear();
			}
			else { 
				// keep state as WakingServiceUp until our timeout reached or we get ack 
				// back from service moving us to state of CommunicationVerified:
				// queue messages during this period
				this.messageQueue.Enqueue(message);
			}
		}
		else if (this.state == ReadyState.CommunicationVerified) {
			// Service is there.  Dequeue all that we have and send those off, 
			// then mark state as Ready so that queue no longer needs to be used.
			while (this.messageQueue.Count > 0) {				
				this.SendMessage(this.messageQueue.Dequeue());
			}
			this.state = ReadyState.Ready;
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
		WWWForm myForm = new WWWForm();
		myForm.AddField("secret", HttpWriter.SHARED_SECRET);
		myForm.AddField("message", message);
		byte[] byteData = myForm.data;
		Hashtable headers = new Hashtable();
		headers = myForm.headers;
		WebService.request(LOGGING_SERVICE_URL + "/LogString", byteData, headers, WebRequestOK, WebRequestErr);
	}
	
	private void WebRequestOK(string response) {
		if (this.state == ReadyState.WakingServiceUp)
			this.state = ReadyState.CommunicationVerified;
	}
		
	private void WebRequestErr(string error) {
		if (this.state == ReadyState.WakingServiceUp) {
			this.state = ReadyState.TimeoutGivingUp;
			this.messageQueue.Clear();
		}
		// NOTE:  not worrying about a lost logged message: !!!TBD!!! should we be? Only concerned about first HelloWorld success or not.
	}
		
	private void WakeUpService() {
		// SOAP Service expects a FORM payload even if no arguments need to be supplied.
		WWWForm myForm = new WWWForm();
		myForm.AddField("", "");
		
		byte[] byteData = myForm.data;
		Hashtable headers = new Hashtable();
		headers = myForm.headers;
		WebService.request(LOGGING_SERVICE_URL + "/HelloWorld", byteData, headers, WebRequestOK, WebRequestErr);
		
		// Set timeout counter to a point in the future.
		this.timeout += Time.time;
	}
}