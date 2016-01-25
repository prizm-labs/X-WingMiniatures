using UnityEngine;
using System.Collections;


public enum attackDiceSides {blank=0, focus, hit, crit};
public enum defendDiceSides {blank=0, focus, evade};

public class Dice : MonoBehaviour {

	bool first = true;

	private float _timeScale = 1;
	public float timeScale {
		get { return _timeScale; }
		set {
			Debug.Log ("setting timescale");
			if (!first) {
				rb.mass *= timeScale;
				rb.velocity /= timeScale;
				rb.angularVelocity /= timeScale;
			}

			first = false;

			_timeScale = Mathf.Abs (value);

			rb.mass /= timeScale;
			rb.velocity *= timeScale;
			rb.angularVelocity *= timeScale;
		}
	}



	Rigidbody rb;
	[System.NonSerialized]
	public Vector3 forceDirection = new Vector3(0, 1, 0);
	[System.NonSerialized]
	public float forceMultiplier = 0.5f;
	[System.NonSerialized]
	public float timeToLive = 4.0f;	//4 seconds

	void Awake () {
		rb = GetComponent<Rigidbody> ();
		timeScale = _timeScale;
	}

	//give the dice a callback when it lands with a return value of what it landed on
	//return as int and use enum to decode by the ship
	//grow the dice over time from when it was created
	//enable/disable the dice instead of destroying them


	void Update() {
		if (timeScale < 0.5f) {
			timeScale = 1f;
		} else {

			timeScale = 0.1f;
		}

	}
	public void Roll(Vector3 startingPosition) {
		transform.position = startingPosition + new Vector3 (0, 20, 0);
		Vector3 forceVector = new Vector3 (Random.value, Random.value, Random.value) * forceMultiplier;
		rb.AddForce (forceVector + forceDirection);
		StartCoroutine (KillAfterTime (timeToLive));
	}

	private IEnumerator KillAfterTime(float ttl) {
		yield return new WaitForSeconds (ttl);
		Destroy (this);
	}

	void OnCollision(Collider coll) {
		Debug.Log ("on colide" + coll.name);
	}


}
