using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TouchScript;
using TouchScript.Gestures;

public class Player : MonoBehaviour {



	[System.NonSerialized]
	public PrizmRecord<PlayerSchema> record = new PrizmRecord<PlayerSchema>();

	public string playerID;
	private Rigidbody rb;

	[System.NonSerialized]
	public List<GameObject> shipsUnderCommand = new List<GameObject>();

	[System.NonSerialized]
	public string faction;

	[System.NonSerialized]
	public Color myColor;
	
	void Start(){
		rb = GetComponent<Rigidbody>();
		myColor = new Color (Random.value, Random.value, Random.value);
	}

	public void SummonShip(string shipName) {
		GameObject newShip = ShipManager.SelectShip (shipName, faction);
		newShip.transform.SetParent (transform);

		newShip.GetComponent<Ship> ().PlayerOwner = this;
		shipsUnderCommand.Add(newShip);
	}
	
	public void initializePlayer(string name, string lightOrDark, string id){
		gameObject.name = name;
		playerID = id;
		faction = lightOrDark;
	}
		
}
