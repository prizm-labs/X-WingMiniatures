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
		explosion = transform.FindChild ("Explosion").gameObject;
		explosion.SetActive (false);
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
		yield return new WaitForSeconds (3.0f);
		Explode ();
	}

	IEnumerator Explode() {

		rb.velocity = Vector3.zero;
		coreObject.SetActive (false);


		Debug.Log ("exploded");
		explosion.SetActive (true);
		yield return new WaitForSeconds (1.0f);

		try {
			if (this.gameObject != null)
			Destroy (this.gameObject);
		}
		catch (MissingReferenceException ex) {
			//do nothing
		}
	}

	void OnTriggerEnter(Collider coll) {
		if (coll.transform == target) {
			StartCoroutine(Explode ());

		}
	}
}
