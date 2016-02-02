using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TouchScript;
using TouchScript.Gestures;

public class Player : MonoBehaviour {



	private int timesPressed = 0;
	public string playerID;
	private Rigidbody rb;
	GameObject myShipObj;
	Ship myShip;
	List<string> movements = new List<string>();

	public int movementPhase = 0;

	
	void Awake(){
		//GetComponent<TapGesture>().Tapped += HandleTap;
		myShipObj = transform.GetChild(0).gameObject;
		myShip = myShipObj.GetComponent<Ship> ();
		movements.Add("{\"speed\":\"3\",\"direction\":\"straight\",\"difficulty\":\"0\"}");
		movements.Add("{\"speed\":\"3\",\"direction\":\"bankLeft\",\"difficulty\":\"0\"}");
		movements.Add("{\"speed\":\"4\",\"direction\":\"turnRight\",\"difficulty\":\"1\"}");

		movements.Add("{\"speed\":\"3\",\"direction\":\"straight\",\"difficulty\":\"0\"}");
		movements.Add("{\"speed\":\"3\",\"direction\":\"bankLeft\",\"difficulty\":\"0\"}");
		movements.Add("{\"speed\":\"4\",\"direction\":\"turnRight\",\"difficulty\":\"1\"}");

		movements.Add("{\"speed\":\"3\",\"direction\":\"straight\",\"difficulty\":\"0\"}");
		movements.Add("{\"speed\":\"3\",\"direction\":\"bankLeft\",\"difficulty\":\"0\"}");
		movements.Add("{\"speed\":\"4\",\"direction\":\"turnRight\",\"difficulty\":\"1\"}");

	}

	public void SetUpTapHandlers() {
		//GetComponent<TapGesture>().Tapped += HandleTap;
	}

	void OnEnable() {
		GetComponent<TapGesture>().Tapped += HandleTap;
		Debug.Log ("tap handler set");
	}

	void OnDisable() {
		GetComponent<TapGesture>().Tapped -= HandleTap;
	}

	void HandleTap (object sender, System.EventArgs e)
	{
		Debug.Log ("player was tapped" + gameObject.name);


		if (GameManager.Instance.MyGameState == GameState.WaitingForPlayersEnter) {
			Destroy (GetComponent<ParticleSystem> ());
			ShowShip ();
			GameManager.Instance.numPlayersJoined++;
			if (GameManager.Instance.numPlayersJoined >= GameManager.Instance.numPlayers) {
				GameManager.Instance.AdvanceGameState ();
			}
		} else if (GameManager.Instance.MyGameState == GameState.PlanningPhase) {
			Debug.Log ("trying to move ship");
			myShip.maneuverRoutine = myShip.ExecuteManeuver(movements[movementPhase]);
			movementPhase++;

			if (movementPhase >= 3)
				myShip.isAttacking = true;

			if (movementPhase <= 3)
				StartCoroutine (myShip.maneuverRoutine);
			else
				StartCoroutine (myShip.ScanAttackRange ());



			GameManager.Instance.numPlayersJoined++;
			if (GameManager.Instance.numPlayersJoined >= GameManager.Instance.numPlayers) {
				//GameManager.Instance.AdvanceGameState ();
				Debug.Log("going to advance game state");
			}
		}

	}



	public void ShowShip() {
		transform.GetChild (0).gameObject.SetActive (true);
	}
		
}
