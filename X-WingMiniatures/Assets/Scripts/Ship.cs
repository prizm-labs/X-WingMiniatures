using UnityEngine;
using System.Collections;
using SimpleJSON;

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
