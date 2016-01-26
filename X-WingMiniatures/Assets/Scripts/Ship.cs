using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

public class Ship : MonoBehaviour {

	public PrizmRecord<ShipSchema> record;


	public Material redManeuverMaterialPrefab;
	public Material greenManeuverMaterialPrefab;
	public Material whiteManeuverMaterialPrefab;

	public GameObject protonTorpedoPrefab;
	public GameObject laserPrefab;

	[System.NonSerialized]
	public Pilot myPilot;

	[System.NonSerialized]
	public Rigidbody rb;

	private int stressTokens = 0;

	private Player playerOwner;
	public Player PlayerOwner{ get { return playerOwner; } set { playerOwner = value; } }

	private IEnumerator maneuverRoutine;

	private bool isMoving = false;

	private LineRenderer lineRenderer;
	private List<Vector3> points;
	[System.NonSerialized]
	public static int steps = 10;	//how many microsteps to move the ship between each curve point (the more, the smoother)

	[System.NonSerialized]
	public static float attackRangeDelta = 10.0f;	//what is the radius of each attack field

	[System.NonSerialized]
	public bool taggedAsPotentialTarget = false;

	List<GameObject> potentialAttackTargets = new List<GameObject>();
	GameObject selectedTarget;

	private int numTorpedos = 2;		//the number of torpedos in one attack (looks cool i guess?)
	private int numLasers = 3;

	[System.NonSerialized]
	public static float normalShipAltitude = -100.0f;

	public void GivePilot(string pilotName) {
		//load the ship from the JSON, determined by the string of id in the json config file
		//Debug.Log("in givepilot: " + pilotName);
		//Debug.Log ("has record? " + record.ToString ());
		//Debug.Log ("has pilot list?: " + record.mongoDocument.pilots.ToString ());
		foreach (JSONNode jo in JSON.Parse(record.mongoDocument.pilots).AsArray) {
			//Debug.Log ("looping thru: " + jo.ToString());
			//Debug.Log ("comparing:" + jo ["name"].Value.ToString () + "to:" + pilotName);
			if (jo ["name"].Value.ToString() == pilotName) {
				//Debug.Log ("found him!: " + pilotName);
				//Debug.Log ("object data: " + jo.ToString ());
				myPilot = new Pilot (jo);
				break;
			}
		}

		//Debug.Log ("found pilot, done giving ship a pilto");
		//Debug.Log ("pilot's stats: " + myPilot.name + ":"+ myPilot.ability + ":" + myPilot.skill.ToString());
	}

	void Awake() {
		normalShipAltitude = -GameManager.DistanceFromCamera + 100;
		rb = GetComponent<Rigidbody>();

		lineRenderer = GetComponent<LineRenderer> ();
		points = new List<Vector3> ();

		selectedTarget = GameObject.Find ("TestTarget");
		//Debug.Log (record.mongoDocument.name);
	}

	void Update() {
		if (Input.GetKeyDown (KeyCode.P)) {
			Debug.Log ("rolling dice");
			StartCoroutine(RollDice ("attack", 2));
		}
		if (Input.GetKeyDown (KeyCode.O)) {
			Debug.Log ("rolling dice");
			StartCoroutine(RollDice ("defend", 2));
		}
		if (Input.GetKeyDown (KeyCode.I)) {
			//Debug.Log ("Trying to draw bezier path for ship");
			maneuverRoutine = ExecuteManeuver("{\"speed\":\"3\",\"direction\":\"straight\",\"difficulty\":\"2\"}");

			StartCoroutine(maneuverRoutine);
			//and then send signal to game manager that this ship is done, advance to next one.
		}
		//
		if (Input.GetKeyDown (KeyCode.U)) {
			StartCoroutine (ScanAttackRange ());
		}
		if (Input.GetKeyDown (KeyCode.K)) {
			StartCoroutine(FireProtonTorpedos ());
		}
		if (Input.GetKeyDown (KeyCode.L)) {
			StartCoroutine(FireLazors ());
		}
		if (Input.GetKeyDown (KeyCode.J)) {
			if (record.mongoDocument.name == "MillenniumFalcon")
				StartCoroutine(DoABarrelRoll(1, 1.0f));
					else
			StartCoroutine (DoABarrelRoll (1));
		}
		if (Input.GetKeyDown (KeyCode.H)) {
			if (record.mongoDocument.name == "MillenniumFalcon")
				StartCoroutine(DoABarrelRoll(-1, 1.0f));
			else
			StartCoroutine (DoABarrelRoll (-1));
		}
		if (Input.GetKeyDown (KeyCode.Space)) {
			StartCoroutine (DodgeAttackBarrelRolls());
		}
	}

	private IEnumerator FireLazors() {
		for (int i = 0; i < numLasers; i++) {
			GameObject laze = Instantiate (laserPrefab) as GameObject;
			laze.GetComponent<Laser> ().target = selectedTarget.transform;
			if (i % 2 == 0 ) laze.transform.position = transform.position + transform.right * 2;
			else laze.transform.position = transform.position - transform.right * 2;
			laze.transform.rotation = transform.rotation;
			StartCoroutine (laze.GetComponent<Laser>().Deploy());

			yield return new WaitForSeconds (0.05f);
		}
		GameManager.Instance.ToggleTime ();
	}

	private IEnumerator FireProtonTorpedos() {

		for (int i = 0; i < numTorpedos; i++) {
			GameObject torp = Instantiate (protonTorpedoPrefab) as GameObject;
			torp.GetComponent<Torpedo> ().target = selectedTarget.transform;
			torp.transform.position = transform.position;
			torp.transform.rotation = transform.rotation;
			if (i % 2 == 0) StartCoroutine (torp.GetComponent<Torpedo>().Deploy(-1));
			else StartCoroutine (torp.GetComponent<Torpedo>().Deploy(1));
			yield return new WaitForSeconds (0.1f);
		}


	}

	public void AnnounceSelf() {
		Debug.Log ("my record is: " + record.mongoDocument);

	}
	private IEnumerator ScanAttackRange() {
		potentialAttackTargets.Clear ();

		if (playerOwner != null) {
			switch (playerOwner.faction) {

			case "light":
				lineRenderer.material = greenManeuverMaterialPrefab;
				break;
			case "dark":
				lineRenderer.material = redManeuverMaterialPrefab;
				break;
			default:
				lineRenderer.material = whiteManeuverMaterialPrefab;
				break;
			}
		}

		RaycastHit hit;
		RaycastHit[] hits;
		//shoots a raycast at every degree for 90 degrees
		for (int j = 1; j < 4; j++) {
			for (int i = (int) (transform.eulerAngles.y + 45); i > (int) (transform.eulerAngles.y - 45); i--) {
				
				//shoot the raycast
				/*	//this one doesn't pass through multiple targets
				if (Physics.Raycast (transform.position, Quaternion.AngleAxis (i, Vector3.up) * Vector3.forward, out hit, j * attackRangeDelta)) {
					Debug.DrawLine (transform.position, hit.point);
					Debug.Log ("HIT SOMETHING! YAY1");
					if (!hit.transform.gameObject.GetComponent<Ship> ().taggedAsPotentialTarget) {
						hit.transform.gameObject.GetComponent<Ship> ().taggedAsPotentialTarget = true;
						hit.transform.gameObject.GetComponent<Ship> ().ShowAsAvailableTarget ();

						potentialAttackTargets.Add (hit.transform.gameObject);
					}

				}
				*/

				//should go through multiple targets
				hits = Physics.RaycastAll (transform.position, Quaternion.AngleAxis (i, Vector3.up) * Vector3.forward, j * attackRangeDelta);

				for (int h = 0; h < hits.Length; h++ ) {

						hit = hits [h];
						Debug.DrawLine (transform.position, hit.point);
						//Debug.Log ("HIT SOMETHING! YAY1");
						if (!hit.transform.gameObject.GetComponent<Ship> ().taggedAsPotentialTarget) {
							hit.transform.gameObject.GetComponent<Ship> ().taggedAsPotentialTarget = true;
							hit.transform.gameObject.GetComponent<Ship> ().ShowAsAvailableTarget ();

							potentialAttackTargets.Add (hit.transform.gameObject);
						}

				}


				//draw a line segment
				lineRenderer.SetVertexCount(2);
				lineRenderer.SetPosition (0, transform.position);
				lineRenderer.SetPosition (1, transform.position + (Quaternion.AngleAxis (i, Vector3.up) * Vector3.forward) * j * attackRangeDelta);

				yield return new WaitForSeconds (0.01f);

			}
		}

		Debug.Log ("Done scanning all attack ranges");
		yield return new WaitForSeconds (0.1f);

	}

	public void TakeDamage(int damageTaken) {
		int damageLeft = damageTaken;
		if (record.mongoDocument.shield > 0 && damageLeft > 0) {
			record.mongoDocument.shield--;
			damageLeft--;
		} else if (record.mongoDocument.hull > 0 && damageLeft > 0) {
			record.mongoDocument.hull--;
			damageLeft--;
		}

		Debug.Log ("damageLeft: " + damageLeft.ToString ());
		Debug.Log ("done calculating damage, shield/hull: " + record.mongoDocument.shield + "/" + record.mongoDocument.hull);
		Debug.Log ("took " + damageTaken.ToString () + " points of damage.");

		if (record.mongoDocument.hull <= 0) {
			//ship is kill
		}

		//if proton torpedos hit, show explosion damage
	}

	//makes the material red (or probably just adds a particle effect)
	public void ShowAsAvailableTarget() {
		Debug.Log (this.name + "has been tagged");

	}

	//generates an explosion particle effect
	public IEnumerator ShowExplosionDamage() {

		return null;
	}
		



	void OnCollisionEnter (Collision coll) {

		if (isMoving) {
			//assumign collided witha  ship
			GetComponent<BoxCollider> ().enabled = false;
			coll.gameObject.GetComponent<Rigidbody> ().isKinematic = true;
			rb.AddExplosionForce (500.0f, coll.transform.position, 100.0f);
			rb.AddTorque (new Vector3 (Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * 4000.0f);
			//stop coroutine for movement (will glitch it out)
			StopCoroutine (maneuverRoutine);
			isMoving = false;

			//set momentum to zero
			StartCoroutine (NormalizeBearingsDelay ());

			coll.gameObject.GetComponent<Rigidbody> ().isKinematic = false;
		}
	}

	void OnTriggerEnter (Collider coll) {
		if (coll.tag == "GateToHell") {
			Debug.Log ("hit the gate to hell, sending to hell");

			Destroy (this.gameObject);

		}
	}

	private IEnumerator NormalizeBearingsDelay(float time = 1.0f) {
		yield return new WaitForSeconds (time);
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		//.Log ("before euler angles: " + transform.eulerAngles.ToString());
		transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);
		//Debug.Log ("after euler angles: " + transform.eulerAngles.ToString());
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		GetComponent<BoxCollider> ().enabled = true;

		transform.position = new Vector3 (transform.position.x, normalShipAltitude, transform.position.z);
	}

	private void StabilizeImmediate() {

		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		//.Log ("before euler angles: " + transform.eulerAngles.ToString());
		transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);
		//Debug.Log ("after euler angles: " + transform.eulerAngles.ToString());
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;

		transform.position = new Vector3 (transform.position.x, normalShipAltitude, transform.position.z);
	}

	public IEnumerator ExecuteManeuver(string json) {
		isMoving = true;

		List<Vector3> drawingPoints = new List<Vector3> ();
		List<Vector3> skeletonPoints = new List<Vector3> ();

		//take the path data from player's selection
		JSONNode maneuverDetails = JSON.Parse(json);

		switch (maneuverDetails ["difficulty"].AsInt) {
		case 0:
			//show green material, remove stress token
			GiveGreenDifficulty();
			break;
		case 1:
			//show white material, no outside effect
			GiveWhiteDifficulty();
			break;
		case 2:
			//show red material, give stress token
			GiveRedDifficulty();
			break;
		default:
			Debug.LogError ("invalid difficulty: " + maneuverDetails ["difficulty"].AsInt.ToString ());
			break;
		}

		/*
		if (stressTokens > 0) {
			record.mongoDocument.isStressed = true;
		} else {
			record.mongoDocument.isStressed = false;
		}
		*/
		//uncomment this for integration testing
		//StartCoroutine (record.Sync ());

		//get the points to draw and move along from maneuver cache
		skeletonPoints = ManeuverCache.GetManeuverPoints(transform, maneuverDetails);

		//draw the bezier curve
		drawingPoints = RenderBezier(skeletonPoints);


		//move ship along curve

		for (int i = 0; i < drawingPoints.Count; i++) {

			float distance = (drawingPoints [i] - transform.position).magnitude;
			transform.LookAt (drawingPoints [i]);

			for (int j = 0; j < steps; j++) {
				transform.position = Vector3.MoveTowards (transform.position, drawingPoints [i], distance / steps);
				//Debug.Log(" going to: " + drawingPoints[i].ToString() + ", from : " + transform.position.ToString());
				yield return new WaitForSeconds (0.01f);
			}
		}


		if (maneuverDetails ["direction"].Value.ToString () == "kTurn") {

			//turn one degree for 180 degrees
			for (int i = 0; i < 180; i++) {
				transform.eulerAngles = transform.eulerAngles + Vector3.up;
				yield return new WaitForSeconds (0.01f);
			}
		}

		//erase curve
		//Debug.Log ("done moving");
		EraseLine ();

		isMoving = false;
		StartCoroutine (NormalizeBearingsDelay ());
	}

	public IEnumerator RollDice(string attackOrDefend, int numDice) {
		for (int i = 0; i < numDice; i++) {
			GameObject newDie = Instantiate(Resources.Load (attackOrDefend + "Die", typeof(GameObject))) as GameObject;
			//GameObject newDie = Instantiate(Resources.Load ("attack_d-8_thickened", typeof(GameObject))) as GameObject;
			newDie.GetComponent<Dice> ().Roll (transform.position + transform.forward * numDice * 10);
			yield return new WaitForSeconds (0.1f);
		}
	}

	private void GiveWhiteDifficulty() {
		lineRenderer.material = whiteManeuverMaterialPrefab;
	}
	private void GiveRedDifficulty() {
		stressTokens++;
		lineRenderer.material = redManeuverMaterialPrefab;
	}
	private void GiveGreenDifficulty() {
		if (stressTokens > 0)
			stressTokens--;
		lineRenderer.material = greenManeuverMaterialPrefab;
	}



	//takes control points and makes into curvy points
	private List<Vector3> RenderBezier(List<Vector3> points)
	{
		BezierPath bezierPath = new BezierPath();

		bezierPath.SetControlPoints(points);
		List<Vector3> drawingPoints = bezierPath.GetDrawingPoints0();
		RenderLine(drawingPoints);
		return drawingPoints;
	}

	//actually draws the line on the screen
	private void RenderLine(List<Vector3> drawingPoints)
	{
		lineRenderer.SetVertexCount(drawingPoints.Count);
		for (int i = 0; i < drawingPoints.Count; i++)
		{
			lineRenderer.SetPosition(i, drawingPoints[i]);
		}
	}

	private void EraseLine() {
		lineRenderer.SetVertexCount (0);
	}

	private IEnumerator DodgeAttackBarrelRolls() {
		float duration = 0.50f;
		if (record.mongoDocument.name == "MillenniumFalcon")
			duration = 1.0f;
		GetComponent<BoxCollider> ().enabled = false;
		StartCoroutine (DoABarrelRoll (1, duration));
		yield return new WaitForSeconds (duration + 0.05f);
		StartCoroutine (DoABarrelRoll (-1, duration));
		yield return new WaitForSeconds (duration + 0.05f);
		GetComponent<BoxCollider> ().enabled = true;

	}

	private IEnumerator DoABarrelRoll(int direction, float duration = 0.5f) {	//0.5f good for tie fighter, bad for M_falcon
		/*
		for (int i = 0; i < 360; i++) {
			transform.Rotate (Vector3.forward * direction);
			transform.position = transform.position + (Vector3.forward * direction) / 10;
			yield return new WaitForSeconds (0.0000001f);
		}
		*/
		rb.AddForce (Vector3.forward * 1000 * direction);
		rb.AddTorque (transform.forward * 10000 * direction);
		yield return StartCoroutine(NormalizeBearingsDelay (duration));
		//yield return null;
	}



}
