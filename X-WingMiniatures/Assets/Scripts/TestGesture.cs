using UnityEngine;
using System.Collections;
using TouchScript;
using TouchScript.Gestures;

public class TestGesture : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	void OnEnable() {
		GetComponent<TransformGesture>().TransformStarted += handler;

	}

	void handler (object sender, System.EventArgs e)
	{
		Debug.Log ("e: " + e.ToString ());
		Debug.Log ("transformed");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
