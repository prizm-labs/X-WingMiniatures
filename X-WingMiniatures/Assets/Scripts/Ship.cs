using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

public class Ship : MonoBehaviour {

	public PrizmRecord<ShipSchema> record;

	public Material redManeuverMaterialPrefab;
	public Material greenManeuverMaterialPrefab;
	public Material whiteManeuverMaterialPrefab;

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
	public static float normalShipAltitude = 0.0f;

	public void GivePilot(string pilotName) {
		//load the ship from the JSON, determined by the string of id in the json config file
		Debug.Log("in givepilot: " + pilotName);
		Debug.Log ("has record? " + record.ToString ());
		Debug.Log ("has pilot list?: " + record.mongoDocument.pilots.ToString ());
		foreach (JSONNode jo in JSON.Parse(record.mongoDocument.pilots).AsArray) {
			Debug.Log ("looping thru: " + jo.ToString());
			Debug.Log ("comparing:" + jo ["name"].Value.ToString () + "to:" + pilotName);
			if (jo ["name"].Value.ToString() == pilotName) {
				Debug.Log ("found him!: " + pilotName);
				Debug.Log ("object data: " + jo.ToString ());
				myPilot = new Pilot (jo);
				break;
			}
		}

		Debug.Log ("found pilot, done giving ship a pilto");
		Debug.Log ("pilot's stats: " + myPilot.name + ":"+ myPilot.ability + ":" + myPilot.skill.ToString());
	}

	void Awake() {
		rb = GetComponent<Rigidbody>();

		lineRenderer = GetComponent<LineRenderer> ();
		points = new List<Vector3> ();
	}

	void Update() {
		if (Input.GetKeyDown (KeyCode.P)) {
			Debug.Log ("rolling dice");
			RollDice ("attack", 2);
		}
		if (Input.GetKeyDown (KeyCode.O)) {
			Debug.Log ("rolling dice");
			RollDice ("defend", 2);
		}
		if (Input.GetKeyDown (KeyCode.Space)) {
			Debug.Log ("Trying to draw bezier path for ship");
			maneuverRoutine = ExecuteManeuver("{\"speed\":2,\"direction\":\"kTurn\",\"difficulty\":0");

			StartCoroutine(maneuverRoutine);
			//and then send signal to game manager that this ship is done, advance to next one.
		}


		if (Input.GetKeyDown (KeyCode.F)) {
			lineRenderer.SetVertexCount (2);
			lineRenderer.SetPosition (0, new Vector3 (0, 0, 0));
			lineRenderer.SetPosition (1, new Vector3 (30, 30, 30));
			lineRenderer.SetPosition (1, new Vector3 (20, 10, 0));
		}
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

	private IEnumerator NormalizeBearingsDelay() {
		yield return new WaitForSeconds (1.0f);
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		Debug.Log ("before euler angles: " + transform.eulerAngles.ToString());
		transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);
		Debug.Log ("after euler angles: " + transform.eulerAngles.ToString());
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		GetComponent<BoxCollider> ().enabled = true;

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

		//get the points to draw and move along from maneuver cache
		skeletonPoints = ManeuverCache.GetManeuverPoints(transform, maneuverDetails);

		//draw the bezier curve
		drawingPoints = RenderBezier(skeletonPoints);


		//move ship along curve

		Debug.Log ("drawingpoints count: " + drawingPoints.Count.ToString ());
		for (int i = 0; i < drawingPoints.Count; i++) {
			Debug.Log ("NEW DESTINATION: " + drawingPoints [i].ToString ());

			float distance = (drawingPoints [i] - transform.position).magnitude;
			Debug.Log ("distance froom start to end: " + distance.ToString ());

			Debug.Log ("looking at: " + drawingPoints [i].ToString ());
			transform.LookAt (drawingPoints [i]);

			for (int j = 0; j < steps; j++) {
				transform.position = Vector3.MoveTowards (transform.position, drawingPoints [i], distance / steps);
				//Debug.Log(" going to: " + drawingPoints[i].ToString() + ", from : " + transform.position.ToString());
				yield return new WaitForSeconds (0.01f);
			}
		}

		Debug.LogError ("KTURN: " + maneuverDetails ["direction"].ToString ());

		if (maneuverDetails ["direction"].Value.ToString () == "kTurn") {
			Debug.Log ("executing kTurn, going to elegantly turn aroudn");

			//turn one degree for 180 degrees
			for (int i = 0; i < 180; i++) {
				transform.eulerAngles = transform.eulerAngles + Vector3.up;
				yield return new WaitForSeconds (0.01f);
			}
		}

		//erase curve
		Debug.Log ("done moving");
		EraseLine ();

		isMoving = false;
	}

	public void RollDice(string attackOrDefend, int numDice) {
		for (int i = 0; i < numDice; i++) {
			GameObject newDie = Instantiate(Resources.Load (attackOrDefend + "Die", typeof(GameObject))) as GameObject;
			newDie.GetComponent<Dice> ().Roll (transform.position);
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



}
