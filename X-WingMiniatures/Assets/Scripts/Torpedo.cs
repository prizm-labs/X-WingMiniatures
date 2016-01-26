using UnityEngine;
using System.Collections;

public class Torpedo : MonoBehaviour {

	[System.NonSerialized]
	public Transform target;
	Rigidbody rb;

	[System.NonSerialized]
	public float deployForce = 500.0f;
	[System.NonSerialized]
	public float rocketDelay = 1.0f;	//seconds from instantiation to deploy
	[System.NonSerialized]
	public float rocketThrust = 5000.0f;	//force multiplier

	//ship will first startcoroutine(deploy())
	//deploy will wait 1.0f seconds

	GameObject explosion;
	GameObject coreObject;

	void Awake() {
		rb = GetComponent<Rigidbody> ();
		explosion = transform.FindChild ("Explosion").gameObject;
		explosion.SetActive (false);

		coreObject = transform.FindChild ("Core").gameObject;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		transform.LookAt (target);
	}

	//pushes torpedos at right angle
	public IEnumerator Deploy(int leftOrRight) {
		transform.LookAt (target);

		rb.AddForce (transform.right * deployForce * leftOrRight);
		yield return new WaitForSeconds (rocketDelay);
		Rocket ();
		yield return new WaitForSeconds (rocketDelay / 2);
		GameManager.Instance.ToggleTime ();
	}

	void Rocket() {
		rb.velocity = Vector3.zero;
		transform.LookAt (target);
		rb.AddForce (transform.forward * rocketThrust);


	}

	IEnumerator Explode() {
		//yield return new WaitForSeconds (0.5f);
		rb.velocity = Vector3.zero;
		coreObject.SetActive (false);


		Debug.Log ("exploded");
		explosion.SetActive (true);
		yield return new WaitForSeconds (1.0f);
		Destroy (this.gameObject);
	}

	void OnTriggerEnter(Collider coll) {
		if (coll.transform == target) {
			StartCoroutine(Explode ());

		}
	}
}
