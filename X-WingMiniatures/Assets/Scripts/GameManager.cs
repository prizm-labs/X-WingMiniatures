using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TouchScript;
using TouchScript.Gestures;
using UnityEngine.UI;
using SimpleJSON;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;

public enum GameState {
	None = 0,
	WaitingForPlayersEnter, 
	WaitingForPlayersChooseShip, 
	PlanningPhase, 	//HH plan their movements
	ActivationPhase, //loop through ships and execute in ascending pilot skill order
						//HH chooses ship action
	CombatPhase,		//loop through ships in descending turn order
						//each HH takes turn choosing 1 ship in attack range
						//bonuses applied (focus, etc)
						//roll dice
						//ships take damage, draw damage cards (if crit, give token)
	EndPhase			//remove focus/evade tokens
						//check if ships are still alive
};


public class GameManager : MonoBehaviour {
	
	public GameObject playerPrefab;
	public GameObject blankShipPrefab;	//used to check if a ship will collide with another in getrandomposition

	public GameObject lightShipPrefab;
	public GameObject darkShipPrefab;


	public Text msgPrefab;

	[System.NonSerialized]
	public GameState MyGameState = GameState.None;

	[System.NonSerialized]
	public static GameManager Instance;

	private Camera mainCamera;
	private float minPerlin = -10f, maxPerlin = 10f;
	private float shakeDuration = 0.02f;
	private int shakeIterations = 10;

	private GameObject msgCanvas;
	private GameObject starsMaster;
	private GameObject bigBangStars;
	private GameObject actionStars;
	private GameObject intro3DText;
	private GameObject waitingForText;

	[System.NonSerialized]
	public AudioSource mainAudioSource;
	public AudioClip starWarsIntroClip;
	public AudioClip starWarsVaderThemeClip;
	[System.NonSerialized]
	public AudioClip currentClip;

	[System.NonSerialized]
	public List<GameObject> playerList = new List<GameObject> ();
	[System.NonSerialized]
	public List<GameObject> sortedShipList = new List<GameObject> ();
	private GameObject playerManagerObject;

	[System.NonSerialized]
	public GameObject focusedShip;

	//string jsonURL = "10.0.1.130:8000/ships.json";
	string jsonURL = "localhost:6969/ships.json";

	[System.NonSerialized]
	public JSONClass masterJSON;

	[System.NonSerialized]
	public static float DistanceFromCamera = 200.0f;
	float BoundariesHeight = 500.0f;


	private int maxNumPlayers = 5;
	[System.NonSerialized]
	public int numPlayers = 2;
	[System.NonSerialized]
	public int numPlayersJoined = 0;
	private int numShips = 0;
	//private bool readyToMoveOntoNextPhase = false;

	private Slider playerNumSlider;
	private Text playerNumUI;

	public TabletopInitialization TT_Reference;
	public PrizmRecordGroup<ShipSchema> shipRecordGroup;
	Meteor.Collection<PlayerSchema> playerCollection;		//list of players

	bool timeStopped = false;
	List<GameObject> munitions = new List<GameObject> ();
	List<Vector3> velocities = new List<Vector3> ();

	public GameObject lightPlayer;
	public GameObject darkPlayer;
	

	void Awake () {
		if (Instance == null)
			Instance = this;
		else if (Instance != this)
			Debug.LogError ("other instance of game manager!");
		
		DontDestroyOnLoad (this.gameObject);

		InitializeGameManager();
		MyGameState++;
	}

	void Start() {
		//StartCoroutine (IntroduceWorld());

	}
		

	void InitializeGameManager(){
		playerManagerObject = GameObject.Find ("PlayerManager");

		msgCanvas = GameObject.Find ("MsgCanvas");
		mainCamera = GameObject.Find ("MainCamera").GetComponent<Camera> ();
		mainAudioSource = GetComponent<AudioSource> ();

		waitingForText = GameObject.Find ("WaitingFor");

		starsMaster = GameObject.Find ("Stars");
		bigBangStars = starsMaster.transform.FindChild ("BigBang").gameObject;
		actionStars = GameObject.Find ("ActionStars");
		intro3DText = GameObject.Find ("Intro3DText");

		foreach (Transform child in starsMaster.transform) {
			child.gameObject.SetActive (false);
		}
		starsMaster.SetActive (false);
		actionStars.SetActive (false);


		CreateBoundariesDice ();
		CreateBoundariesShip ();

		lightPlayer = GameObject.Find ("TapToSpawnLight");
		darkPlayer = GameObject.Find ("TapToSpawnDark");

		lightPlayer.transform.position = GetRandomSpawnPosition ("light", true);
		darkPlayer.transform.position = GetRandomSpawnPosition ("dark", true);

		lightPlayer.transform.GetChild (0).gameObject.SetActive (false);
		darkPlayer.transform.GetChild (0).gameObject.SetActive (false);

		darkPlayer.SetActive (false);

		lightPlayer.SetActive (false);
	}

	IEnumerator IntroduceWorld() {



		yield return new WaitForSeconds (1.0f);

		starsMaster.SetActive (true);
		bigBangStars.SetActive (true);
		mainAudioSource.clip = starWarsIntroClip;
		mainAudioSource.Play ();

		yield return new WaitForSeconds (1.0f);

		intro3DText.GetComponent<Rigidbody> ().velocity = new Vector3 (0.0f, -5.0f, 5.0f);

		yield return new WaitForSeconds (2.0f);

		foreach(Transform child in starsMaster.transform) {
			child.gameObject.SetActive (true);
		}
		for (int i = 99; i > 0; i--) {
			mainAudioSource.volume = i * .01f;
			yield return new WaitForSeconds(0.1f);
		}
		bigBangStars.SetActive (false);
		mainAudioSource.Stop();
		intro3DText.SetActive (false);

		darkPlayer.SetActive (true);
		lightPlayer.SetActive (true);

	}

		
	void Update(){

		if (Input.GetKeyDown (KeyCode.Q)) {
			StartCoroutine(IntroduceWorld ());
		}
		if (Input.GetKeyDown (KeyCode.W)) {
			darkPlayer.SetActive (true);
			lightPlayer.SetActive (true);
		}
		if (Input.GetKeyDown (KeyCode.E)) {
			AdvanceGameState ();
		}
		if (Input.GetKeyDown (KeyCode.R)) {

		}
		if (Input.GetKeyDown (KeyCode.A)) {

		}
			
		if (Input.GetKeyDown (KeyCode.D)) {
			ToggleTime ();
		}
		if (Input.GetKeyDown (KeyCode.F)) {

		}
		if (Input.GetKeyDown (KeyCode.Z)) {
			PlayAudioChance ("Music", "DarthVader", 0.90f);
		}
		if (Input.GetKeyDown (KeyCode.Space)) {
			//PlayAudioChance ("SHIPS_EFFECTS/Move", chance: 0.90f);
			PlayAudioChanceAtPoint("TieFighter/Move", chance: 0.80f);
			//currentClip = starWarsIntroClip;
			//AudioSource.PlayClipAtPoint (currentClip, Vector3.zero);
		}
	}

	public void ToggleTime() {
		
		if (timeStopped) {
			for (int i = 0; i < munitions.Count; i++) {
				if (munitions[i] != null)
					munitions [i].GetComponent<Rigidbody> ().velocity = velocities [i];
			}

			timeStopped = false;
		} else {
			munitions.Clear ();
			velocities.Clear ();
			foreach (GameObject go in GameObject.FindGameObjectsWithTag ("Munition")) {
				munitions.Add (go);
			}
			for (int i = 0; i < munitions.Count; i++) {
				velocities.Insert (i, munitions [i].GetComponent<Rigidbody> ().velocity);
			}
			foreach (GameObject go in munitions) {
				go.GetComponent<Rigidbody> ().velocity = Vector3.zero;
			}
			timeStopped = true;
		}
	}

	public Vector3 GetRandomSpawnPosition(string faction, bool middle = false) {
		//light spawns on left, dark spawns on right
		Vector3 tryPos;
		RaycastHit hit;
		bool goingToCollide = false;

		if (middle) {
			switch (faction) {
			case "light":
				tryPos = mainCamera.ViewportToWorldPoint (new Vector3 (0.05f, 0.5f, DistanceFromCamera));
				return tryPos;

			case "dark":
				tryPos = mainCamera.ViewportToWorldPoint (new Vector3 (0.95f, 0.5f, DistanceFromCamera));
				return tryPos;
			}
		}




		int attempts = 10;		//will try 10 times before giving up

		switch (faction) {
		case "light":
			do {
				attempts--;
				tryPos = mainCamera.ViewportToWorldPoint (new Vector3 (0.05f, Random.value, DistanceFromCamera));
				if (Physics.SphereCast (tryPos - new Vector3 (0, 50, 0), 5.0f, Vector3.up, out hit)) {
					if (hit.transform.tag == "Ship") {
						goingToCollide = true;
					} else {
						goingToCollide = false;
					}
				} else {
					goingToCollide = false;
				}
			} while (goingToCollide && attempts >= 0);
			if (attempts <= 0) return mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, DistanceFromCamera));
			return tryPos;

		case "dark":
			do {
				attempts--;
				tryPos = mainCamera.ViewportToWorldPoint (new Vector3 (0.95f, Random.value, DistanceFromCamera));
				if (Physics.SphereCast (tryPos - new Vector3 (0, 50, 0), 5.0f, Vector3.up, out hit)) {
					if (hit.transform.tag == "Ship") {
						goingToCollide = true;
					} else {
						goingToCollide = false;
					}
				} else {
					goingToCollide = false;
				}
			} while (goingToCollide && attempts >= 0);
			if (attempts <= 0) return mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, DistanceFromCamera));
			return tryPos;

		default:
			Debug.LogError ("invalid faction: " + faction + ", spawning in middle of game");
			return mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, DistanceFromCamera));
		}
	}

	public void AdvanceGameState() {

		numPlayersJoined = 0;


		switch (MyGameState) {
		case GameState.WaitingForPlayersEnter:
			//StartCoroutine (IntroduceWorld ());
			//AdvanceGameState();
			MyGameState++;


			//send message to HH to choose ships (might wait 2 seconds)
			break;
		case GameState.WaitingForPlayersChooseShip:
			//CreateSortedListShips ();
			//Debug.Log ("players choosing ship");
			//in this state we will send message to HH to show planning phase
			break;
		case GameState.PlanningPhase:
			//StartCoroutine (ExecuteAllShipsManeuvers ());
			Debug.Log ("players are planning");
			break;
		case GameState.ActivationPhase:
			//when activation is done, need to clean up as in: 
			//all ships need to execute their action (evade, focus, etc)
			//this is where tokens get placed
			Debug.Log ("players activating, waiting for all players to lock in an action on handheld");
			break;
		case GameState.CombatPhase:
			Debug.Log ("entering combat phase");
			//StartCoroutine (ExecuteAllShipsCombat ());
			//advance to next phase when all ships have been hit (check if phaseresponsibilitiescompleted)
			break;
		case GameState.EndPhase:
			Debug.Log ("cleanup phase");
			//check if all ships of one faction are dead
			//CheckWinCondition();
			break;
		default:
			Debug.LogError("game state unstable!" + MyGameState.ToString());
			break;
		}

		//advance game state at end (take care of work of what game state we are currently in at advance gamestate())
		MyGameState++;
		//Debug.Log ("Game State Advance: " + MyGameState.ToString () + " out of: " + System.Enum.GetValues (typeof(GameState)).Length.ToString ());

		if ((int)MyGameState > System.Enum.GetValues (typeof(GameState)).Length - 1) {
			MyGameState = GameState.PlanningPhase;
		}

		foreach (GameObject ship in sortedShipList) {
			ship.GetComponent<Ship> ().phaseDutiesCompleted = false;
		}

		//remember to give a UI text indication that sthe stage is advancing
		//make this ui notice look nicer?

		//createMsgLog ("Now entering game state: " + MyGameState.ToString ());

		Debug.Log ("we are now in the " + MyGameState.ToString () + " phase");

	}

	private void CheckWinCondition() {
		string winningFaction = sortedShipList[0].GetComponent<Ship>().record.mongoDocument.faction;
		foreach (GameObject go in sortedShipList) {
			if (winningFaction != go.GetComponent<Ship> ().record.mongoDocument.faction) {
				return;
			}
		}

		createMsgLog ("GAMEOVER! THE " + winningFaction + " has taken victory this time");
	}

	//removes the ship that died, checks win condition
	public void ShipHasDied(GameObject shipThatDied) {
		sortedShipList.Remove (shipThatDied);
		CheckWinCondition ();
	}

	private IEnumerator ExecuteAllShipsCombat() {
		foreach (GameObject craft in sortedShipList) {
			StartCoroutine(craft.GetComponent<Ship>().EnterCombat());
			PlayAudioChance ("CombatPhase/" + craft.GetComponent<Ship> ().record.mongoDocument.faction);
			while (!craft.GetComponent<Ship> ().phaseDutiesCompleted) {
				yield return null;
			}
		}

		AdvanceGameState ();
	}

	private IEnumerator ExecuteAllShipsManeuvers() {
		foreach (GameObject craft in sortedShipList) {
			focusedShip = craft;
			StartCoroutine(craft.GetComponent<Ship>().maneuverRoutine);
			while (craft.GetComponent<Ship> ().isMoving) {
				yield return null;
			}
			yield return new WaitForSeconds (1.0f);
			craft.GetComponent<Ship> ().phaseDutiesCompleted = true;
		}

		AdvanceGameState ();
	}

	public void PlayAudioChance (string folder, string specificTrack = null, float chance = 0.20f, float volume = 1.0f) {

		string path = "Audio/" + folder;

		if (Random.value < chance) {
			if (specificTrack != null) {		//load that specific track
				Debug.Log("loading from: " + path + "/" + specificTrack);
				currentClip = Resources.Load(path + "/" + specificTrack, typeof(AudioClip)) as AudioClip;
				mainAudioSource.clip = currentClip;
				mainAudioSource.volume = volume;
				mainAudioSource.Play ();
			} else {		//find random track from folder
				int randomClip = Random.Range (0, CountNumClips (folder));
				Debug.Log ("random clip playing is: " + randomClip.ToString ());
				currentClip = Resources.Load(path + "/" + GetFileName(path, randomClip), typeof(AudioClip)) as AudioClip;
				mainAudioSource.clip = currentClip;
				mainAudioSource.volume = volume;
				mainAudioSource.Play ();
			}
		}
	}

	public void PlayAudioChanceAtPoint (string folder, string specificTrack = null, float chance = 0.20f, float volume = 1.0f, Vector3 position = new Vector3()) {
		
		if (position == Vector3.zero)
			position = mainCamera.transform.position;
		string path = "Audio/" + folder;

		if (Random.value < chance) {
			if (specificTrack != null) {
				currentClip = Resources.Load(path + "/" + specificTrack, typeof(AudioClip)) as AudioClip;
				AudioSource.PlayClipAtPoint (currentClip, position);
			} else {		//find random track from folder
				int randomClip = Random.Range (0, CountNumClips (folder));
				currentClip = Resources.Load(path + "/" + GetFileName(path, randomClip), typeof(AudioClip)) as AudioClip;
				AudioSource.PlayClipAtPoint (currentClip, position);
			}
		}
	}

	private string GetFileName(string folder, int index) {
		DirectoryInfo levelDirectoryPath = new DirectoryInfo (Application.dataPath + "/Resources/" + folder);
		FileInfo[] fileInfo = levelDirectoryPath.GetFiles ("*.*", SearchOption.AllDirectories);
		return fileInfo [index * 2].Name.Substring(0,fileInfo [index * 2].Name.Length - 4);
	}

	//returns number of resources in a particular folder in Resources/
	public int CountNumClips(string folderName) {
		DirectoryInfo levelDirectoryPath = new DirectoryInfo (Application.dataPath + "/Resources/Audio/" + folderName);
		FileInfo[] fileInfo = levelDirectoryPath.GetFiles ("*.*", SearchOption.AllDirectories);
		int num = 0;
		foreach (FileInfo file in fileInfo) {
			if (file.Extension != ".meta") {
				num++;
			}
		}
		return num;
	}






	public IEnumerator ShakeCamera(float innerDuration = 0.02f, int iterations = 10) {
		Vector3 newPos;
		Vector3 cameraSetPosition = mainCamera.transform.position;
		newPos = mainCamera.transform.position;
		for (int i = 0; i < iterations; i++) {

			newPos.x = Mathf.PerlinNoise (newPos.x * minPerlin, newPos.x * maxPerlin) - 0.5f;
			newPos.y = Mathf.PerlinNoise (newPos.y * minPerlin, newPos.y * maxPerlin) - 0.5f;
			GetComponent<Camera>().transform.position = newPos;

			yield return new WaitForSeconds (innerDuration);
		}

		GetComponent<Camera>().transform.position = cameraSetPosition;

		yield return null;
	}


	public void createMsgLog(string message, float timer = 2f){
		if(msgCanvas.transform.FindChild("mesg")){
			msgCanvas.transform.FindChild("mesg").GetComponent<selfDestructMessage>().killMyself(message, timer);
		}
		else{
			Text newText = Instantiate (msgPrefab) as Text;
			newText.transform.position.Set (0, 0, 0);
			newText.transform.SetParent(msgCanvas.transform);
			newText.gameObject.name = "mesg";
			newText.GetComponent<selfDestructMessage> ().killMyself (message, timer);
		}
	}

	//creates walls so balls can't escape world
	public void CreateBoundariesDice() {
		List<GameObject> boundaries = new List<GameObject> ();

		Vector3 lowerLeft = mainCamera.ViewportToWorldPoint (new Vector3 (0.05f, 0.05f, DistanceFromCamera));
		Vector3 lowerRight = mainCamera.ViewportToWorldPoint (new Vector3 (0.95f, 0.05f, DistanceFromCamera));
		Vector3 upperLeft = mainCamera.ViewportToWorldPoint (new Vector3 (0.05f, 0.95f, DistanceFromCamera));
		Vector3 upperRight = mainCamera.ViewportToWorldPoint (new Vector3 (0.95f, 0.95f, DistanceFromCamera));

		float width = lowerRight.x - lowerLeft.x;
		float height = upperRight.z - lowerRight.z; 


		Vector3 bottom = (lowerLeft + lowerRight ) / 2;
		Vector3 top = (upperLeft + upperRight ) / 2;
		Vector3 left = (upperLeft + lowerLeft ) / 2;
		Vector3 right = (lowerRight + upperRight ) / 2;


		GameObject bottomBound = GameObject.CreatePrimitive(PrimitiveType.Cube);
		bottomBound.transform.position = bottom;
		bottomBound.transform.localScale = new Vector3 (width, BoundariesHeight, 1f);

		GameObject topBound = GameObject.CreatePrimitive(PrimitiveType.Cube);
		topBound.transform.position = top;
		topBound.transform.localScale = new Vector3 (width, BoundariesHeight, 1f);

		GameObject leftBound = GameObject.CreatePrimitive(PrimitiveType.Cube);
		leftBound.transform.position = left;
		leftBound.transform.localScale = new Vector3 (1f, BoundariesHeight, height);

		GameObject rightBound = GameObject.CreatePrimitive(PrimitiveType.Cube);
		rightBound.transform.position = right;
		rightBound.transform.localScale = new Vector3 (1f, BoundariesHeight, height);

		boundaries.Add (bottomBound);
		boundaries.Add (topBound);
		boundaries.Add (leftBound);
		boundaries.Add (rightBound);

		foreach (GameObject bond in boundaries) {
			bond.AddComponent<Rigidbody> ();
			bond.GetComponent<Rigidbody> ().useGravity = false;
			bond.GetComponent<Rigidbody> ().isKinematic = true;
		}

		bottomBound.layer = 9;	//Dice layer
		topBound.layer = 9;	//Dice layer
		leftBound.layer = 9;	//Dice layer
		rightBound.layer = 9;	//Dice layer

		//make boundaries invisible
		Destroy(bottomBound.GetComponent<MeshRenderer>());
		Destroy(bottomBound.GetComponent<MeshCollider>());

		Destroy(topBound.GetComponent<MeshRenderer>());
		Destroy(topBound.GetComponent<MeshCollider>());

		Destroy(leftBound.GetComponent<MeshRenderer>());
		Destroy(leftBound.GetComponent<MeshCollider>());

		Destroy(rightBound.GetComponent<MeshRenderer>());
		Destroy(rightBound.GetComponent<MeshCollider>());
	}

	//creates walls so ships die when they hit it
	public void CreateBoundariesShip() {

		Vector3 lowerLeft = mainCamera.ViewportToWorldPoint (new Vector3 (-0.1f, -0.10f, DistanceFromCamera));
		Vector3 lowerRight = mainCamera.ViewportToWorldPoint (new Vector3 (1.1f, -0.10f, DistanceFromCamera));
		Vector3 upperLeft = mainCamera.ViewportToWorldPoint (new Vector3 (-0.1f, 1.1f, DistanceFromCamera));
		Vector3 upperRight = mainCamera.ViewportToWorldPoint (new Vector3 (1.1f, 1.1f, DistanceFromCamera));

		float width = lowerRight.x - lowerLeft.x;
		float height = upperRight.z - lowerRight.z; 


		Vector3 bottom = (lowerLeft + lowerRight ) / 2;
		Vector3 top = (upperLeft + upperRight ) / 2;
		Vector3 left = (upperLeft + lowerLeft ) / 2;
		Vector3 right = (lowerRight + upperRight ) / 2;


		GameObject bottomBound = GameObject.CreatePrimitive(PrimitiveType.Cube);
		bottomBound.transform.position = bottom;
		bottomBound.transform.localScale = new Vector3 (width, BoundariesHeight, 0.1f);

		GameObject topBound = GameObject.CreatePrimitive(PrimitiveType.Cube);
		topBound.transform.position = top;
		topBound.transform.localScale = new Vector3 (width, BoundariesHeight, 0.1f);

		GameObject leftBound = GameObject.CreatePrimitive(PrimitiveType.Cube);
		leftBound.transform.position = left;
		leftBound.transform.localScale = new Vector3 (0.1f, BoundariesHeight, height);

		GameObject rightBound = GameObject.CreatePrimitive(PrimitiveType.Cube);
		rightBound.transform.position = right;
		rightBound.transform.localScale = new Vector3 (0.1f, BoundariesHeight, height);

		bottomBound.GetComponent<BoxCollider> ().isTrigger = true;
		bottomBound.tag = "GateToHell";
		bottomBound.layer = 10;	//Ship Layer
		bottomBound.name = "Hell";

		topBound.GetComponent<BoxCollider> ().isTrigger = true;
		topBound.tag = "GateToHell";
		topBound.layer = 10;	//Ship Layer
		topBound.name = "Hell";

		leftBound.GetComponent<BoxCollider> ().isTrigger = true;
		leftBound.tag = "GateToHell";
		leftBound.layer = 10;	//Ship Layer
		leftBound.name = "Hell";

		rightBound.GetComponent<BoxCollider> ().isTrigger = true;
		rightBound.tag = "GateToHell";
		rightBound.layer = 10;	//Ship Layer
		rightBound.name = "Hell";


		//make boundaries invisible
		Destroy(bottomBound.GetComponent<MeshRenderer>());
		Destroy(bottomBound.GetComponent<MeshCollider>());

		Destroy(topBound.GetComponent<MeshRenderer>());
		Destroy(topBound.GetComponent<MeshCollider>());

		Destroy(leftBound.GetComponent<MeshRenderer>());
		Destroy(leftBound.GetComponent<MeshCollider>());

		Destroy(rightBound.GetComponent<MeshRenderer>());
		Destroy(rightBound.GetComponent<MeshCollider>());
	}
		



}