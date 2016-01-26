using UnityEngine;
using System.Collections;


public enum attackDiceSides {blank=0, focus, hit, crit};
public enum defendDiceSides {blank=0, focus, evade};

public class Dice : MonoBehaviour {

	bool first = true;
	bool stabilized = false;




	Rigidbody rb;
	[System.NonSerialized]
	public Vector3 forceDirection = new Vector3(0, 50, 0);
	[System.NonSerialized]
	public float forceMultiplier = 250.0f;
	[System.NonSerialized]
	public float timeToLive = 4.0f;	//4 seconds

	void Awake () {
		rb = GetComponent<Rigidbody> ();

		foreach (Transform child in transform.GetChild(0)) {
			child.gameObject.AddComponent<DiceShell> ();
		}
	}

	//give the dice a callback when it lands with a return value of what it landed on
	//return as int and use enum to decode by the ship
	//grow the dice over time from when it was created
	//enable/disable the dice instead of destroying them


	void Update() {
		if (!stabilized && rb.velocity.magnitude < 0.01f) {
			DiceHasStabilized ();
		}
	}

	void DiceHasStabilized() {
		stabilized = true;
		foreach (Transform child in transform.GetChild(0)) {
			if (child.GetComponent<DiceShell> ().isThisTheChosenOne) {
				Debug.Log ("chosen one found!: " + child.GetComponent<Renderer> ().material.name.ToString());
			}
		}
		StartCoroutine (KillAfterTime (timeToLive));

	}
		

	public void Roll(Vector3 startingPosition) {
		Debug.Log ("rolling: ");
		transform.position = startingPosition + new Vector3 (0, 30, 0);
		Vector3 forceVector = new Vector3 (Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)) * forceMultiplier;
		rb.AddForce (forceVector + forceDirection);
		rb.AddTorque (forceVector);
		StartCoroutine (KillAfterTime (timeToLive * 5));
		//StartCoroutine (DoubleRoll ());
	}

	public IEnumerator DoubleRoll() {
		yield return new WaitForSeconds (1.5f);
		Debug.Log ("doubleRolling: ");
		Vector3 forceVector = new Vector3 (Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)) * forceMultiplier;
		rb.AddForce (forceVector);
	}

	private IEnumerator KillAfterTime(float ttl) {
		yield return new WaitForSeconds (ttl);
		Destroy (this.gameObject);
	}

	void OnCollision(Collider coll) {
		Debug.Log ("on colide" + coll.name);
	}




}
