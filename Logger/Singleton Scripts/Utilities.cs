using UnityEngine;
using System.Collections;

/// <summary>
/// Unity only allows the creation of Coroutines (and therefore web requests 
/// using its WWW utility) inside MonoBehaviours, so HttpWriter.cs and other code 
/// use these behaviors as a proxy to that functionality.
/// </summary>
public class Scheduler : MonoBehaviour
{
	public static Coroutine startCoroutine(IEnumerator method)
	{
		var instance = Scheduler.FindObjectOfType(typeof(Scheduler)) as Scheduler;
		if (instance == null)
		{
			GameObject o = new GameObject("Scheduler");
			DontDestroyOnLoad(o);
			instance = o.AddComponent<Scheduler>();
			DontDestroyOnLoad(instance);
		}
		return instance.StartCoroutine(method);
	}
}

/// <summary>
/// A class for calling web services from non-MonoBehaviour classes.
/// </summary>
public class WebService
{	
	public delegate void WebResponseHandler(string reponse);
	public delegate void WebErrorHandler(string error);
	
	static IEnumerator worker( WWW www, WebResponseHandler responseHandler, WebErrorHandler errorHandler)
	{
		yield return www;
		
		if( www.error != null ) {
//			Debug.Log ("Request to " + www.url + " failed");
			Debug.Log( "WWW error: " + www.error );
			if (errorHandler != null)
				errorHandler(www.error);
		} else {
//			Debug.Log ("Request to " + www.url + " succeeded");
			if (responseHandler != null)
				responseHandler(www.text);
		}
	}
	
	public static Coroutine request( string url, byte[] data, Hashtable headers, WebResponseHandler callback) 
	{
		return request(url, data, headers, callback, null);		
	}
	
	public static Coroutine request( string url, byte[] data, Hashtable headers, WebResponseHandler callback, WebErrorHandler errorHandler)
	{
		return Scheduler.startCoroutine(worker( new WWW(url, data, headers), callback, errorHandler));
	}
}