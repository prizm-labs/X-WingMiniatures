using UnityEngine;
using System.Collections;

public class ShipManager : MonoBehaviour {

	//has enum of ships to choose from
	public enum AllianceShipyard {xWing=0}
	public enum ImperialShipyard {tieFighter=0, vaderTieFighter}

	// Use this for initialization
	void Start () {
	
	}

	//has to instantiate a ship prefab (with mesh renderer and ship scripts, etc) and gives it to the player so they can make it their own
	static public GameObject SelectShip(string shipName, string faction) {
		
		//load the ship like this and assign it as the player gameobject's child
		GameObject brandNewShipInstance = Instantiate (Resources.Load ("ShipPrefabs/" + shipName, typeof(GameObject))) as GameObject;
		brandNewShipInstance.transform.position = GameManager.Instance.GetRandomSpawnPosition (faction);
		return brandNewShipInstance;
	}
}
