using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour {


	[System.NonSerialized]
	public Transform target;
	Rigidbody rb;


	[System.NonSerialized]
	public float lazorThrust = 5000.0f;	//force multiplier

	//ship will first startcoroutine(deploy())
	//deploy will wait 1.0f seconds

	GameObject explosion;
	GameObject coreObject;

	void Awake() {
		rb = GetComponent<Rigidbody> ();
	}

	// Update is called once per frame
	void FixedUpdate () {

		transform.LookAt (target);
	}

	//pushes torpedos at right angle
	public IEnumerator Deploy() {
		rb.velocity = Vector3.zero;
		transform.LookAt (target);
		rb.AddForce (transform.forward * lazorThrust);
		yield return null;
	}

	void Explode() {

		Destroy (this.gameObject);
	}

	void OnTriggerEnter(Collider coll) {
		if (coll.transform == target) {
			Explode ();

		}
	}
}
