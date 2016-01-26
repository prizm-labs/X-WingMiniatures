using UnityEngine;
using System.Collections;

public class DiceShell : MonoBehaviour {

	[System.NonSerialized]
	public bool isThisTheChosenOne = false;


	void OnTriggerEnter (Collider col) {
		if (col.tag == "DiceFloor") {
			isThisTheChosenOne = true;
		}
	}

	void OnTriggerExit (Collider col) {
		if (col.tag == "DiceFloor") {
			isThisTheChosenOne = false;
		}
	}
}
