using UnityEngine;
using System.Collections;

public class Ship : MonoBehaviour {

	public PrizmRecord<ShipSchema> record;

	[System.NonSerialized]
	public Pilot myPilot;

	[System.NonSerialized]
	public Rigidbody rb;

	private Player playerOwner;
	public Player PlayerOwner{ get { return playerOwner; } set { playerOwner = value; } }

	/*
	private string myPlayerOwnerColorString;
	public string MyPlayerOwnerColorString{ get { return myPlayerOwnerColorString; } set { myPlayerOwnerColorString = value; } }
	private string myLocation;
	public string MyLocation{ get { return myLocation; } set { myLocation = value; } }
	public bool OnBoardToBeCollected;
	*/

	public Ship(string shipName, string pilotName) {
		//load the ship from the JSON, determined by the string of id in the json config file

		myPilot = gameObject.AddComponent<Pilot> ();

		myPilot.Initialize (pilotName);
	}

	void Awake() {
		rb = GetComponent<Rigidbody>();

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
	}

	public void RollDice(string attackOrDefend, int numDice) {
		for (int i = 0; i < numDice; i++) {
			GameObject newDie = Instantiate(Resources.Load (attackOrDefend + "Die", typeof(GameObject))) as GameObject;
			newDie.GetComponent<Dice> ().Roll (transform.position);
		}
	}

}
