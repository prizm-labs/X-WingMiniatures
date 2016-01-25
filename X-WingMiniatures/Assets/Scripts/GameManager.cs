using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TouchScript;
using TouchScript.Gestures;
using UnityEngine.UI;
using SimpleJSON;
using UnityEngine.SceneManagement;

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
	public List<GameObject> playerList = new List<GameObject> ();
	private GameObject playerManagerObject;

	string jsonURL = "10.0.1.130:8000/ships.json";

	[System.NonSerialized]
	public JSONClass masterJSON;

	[System.NonSerialized]
	public static float DistanceFromCamera = 200.0f;
	float BoundariesHeight = 50.0f;


	private int maxNumPlayers = 5;
	private int numPlayers = 2;
	private int numPlayersJoined = 0;

	private Slider playerNumSlider;
	private Text playerNumUI;

	public TabletopInitialization TT_Reference;
	public PrizmRecordGroup<ShipSchema> shipRecordGroup;
	Meteor.Collection<PlayerSchema> playerCollection;		//list of players


	void Awake () {
		if (Instance == null)
			Instance = this;
		else if (Instance != this)
			Debug.LogError ("other instance of game manager!");
		
		DontDestroyOnLoad (this.gameObject);

		StartCoroutine(LoadJsonFromServer ());

		//uncomment this when done working on main scene

		//playerNumSlider = GameObject.Find ("Slider").GetComponent<Slider> ();
		//playerNumUI = GameObject.Find ("Number").GetComponent<Text> ();
		//playerNumSlider.value = 0.25f;


		//comment this out when done working on main scene
		InitializeGameManager();
		MyGameState++;
	}

	/*
	void Start() {
		TT_Reference = GetComponent<TabletopInitialization> ();
		shipRecordGroup = TT_Reference.shipRecordGroup;
		StartCoroutine (TT_Reference.ConfigureShipDatabase());
		playerCollection = TT_Reference.playerCollection;
	}
	*/

	public void SetPlayerNum() {
		numPlayers =(int) (playerNumSlider.value * (float)maxNumPlayers + 1.0f);
		Debug.Log ("num players: " + numPlayers.ToString ());
		UpdatePlayerNumUI();
	}

	public void UpdatePlayerNumUI() {
		playerNumUI.text = numPlayers.ToString();
	}

	public void LoadMainScene() {
		UpdatePlayerNumUI ();
		MyGameState++;
		SceneManager.LoadScene ("Main");
	}

	IEnumerator LoadJsonFromServer(){
		masterJSON = new JSONClass ();

		var json = new WWW (jsonURL);
		yield return json;

		masterJSON = JSON.Parse (json.text).AsObject;
		Debug.Log("json loaded: " + masterJSON["ships"].ToString());
	}

	void OnLevelWasLoaded() {
		Debug.Log ("preserved number of players as: " + numPlayers.ToString ());
		InitializeGameManager ();
	}

	void InitializeGameManager(){

		playerManagerObject = GameObject.Find ("PlayerManager");

		Debug.Log ("initializing GameManager");
		msgCanvas = GameObject.Find ("MsgCanvas");
		mainCamera = GameObject.Find ("MainCamera").GetComponent<Camera> ();
		mainAudioSource = GetComponent<AudioSource> ();

		waitingForText = GameObject.Find ("WaitingFor");

		starsMaster = GameObject.Find ("Stars");
		Debug.Log ("stars master: " + starsMaster.ToString ());
		bigBangStars = starsMaster.transform.FindChild ("BigBang").gameObject;
		actionStars = GameObject.Find ("ActionStars");
		intro3DText = GameObject.Find ("Intro3DText");

		foreach (Transform child in starsMaster.transform) {
			child.gameObject.SetActive (false);
		}
		starsMaster.SetActive (false);
		actionStars.SetActive (false);

		waitingForText.GetComponent<Text> ().text = "Waiting for " + (numPlayers - numPlayersJoined).ToString () + " more players to join\nJoin IP Address:'" + TabletopInitialization.GetIP () + ":6969'";
	}

	IEnumerator IntroduceWorld() {
		
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
			yield return new WaitForSeconds(0.5f);
		}
		bigBangStars.SetActive (false);
		mainAudioSource.Stop();
		intro3DText.SetActive (false);
	}

	void SetupRecordHandlers() {
		playerCollection.DidAddRecord += (string arg1, PlayerSchema arg2) => {
			var doc = arg2;
			if (doc.session_id == TabletopInitialization.Instance.sessionID) {
				Debug.Log("creating player record handled from gamemanager");
				GetComponent<GameManager>().CreateNewPlayer(doc);
			}
		};

		shipRecordGroup.didAddRecord += HandleDidAddShipRecord;
	}

	void HandleDidAddShipRecord (string arg1, ShipSchema arg2)
	{
		Debug.Log ("added ship: " + arg1);

		//make a record on our side from the data received
		PrizmRecord<ShipSchema> tempRecord = new PrizmRecord<ShipSchema> ();
		tempRecord.mongoDocument = arg2;

		//find the owner from the list
		GameObject owner = playerList.Find (p => p.GetComponent<Player> ().playerID.Equals (arg2.owner));
		Debug.Log ("faction before giving: " + owner.GetComponent<Player> ().faction);
		//give the ship to the player
		giveShipToPlayer (owner.GetComponent<Player>(), tempRecord);


	}

	//functions that show how to 'give' players objects
	//instantiates ship and creates the game object from the shiprecord
	void giveShipToPlayer(Player ply, PrizmRecord<ShipSchema> shipRecord) {
		//Debug.Log ("in giveshiptoplayer(), shiprecord: " + shipRecord.ToString ());
		//Debug.Log ("now, the schema: " + shipRecord.mongoDocument.ToString());
		//Debug.Log ("now, the list: " + shipRecord.mongoDocument.pilots.ToString ());

		GameObject ship_obj = Instantiate (Resources.Load<GameObject> ("ShipPrefabs/" + shipRecord.mongoDocument.name));

		shipRecord.gameObject = ship_obj;



		//give database items
		ship_obj.GetComponent<Ship> ().record = shipRecord;	//maybe this will work?

		//give pilot
		ship_obj.GetComponent<Ship>().GivePilot(shipRecord.mongoDocument.selectedPilot);
		ship_obj.GetComponent<Ship> ().PlayerOwner = ply;

		ply.shipsUnderCommand.Add (ship_obj); 
		ship_obj.transform.SetParent (ply.transform);
		Debug.Log ("faction: " + ply.faction);
		ship_obj.transform.position = GetRandomSpawnPosition(ply.faction);		
	}

	void Update(){

		if (Input.GetKeyDown (KeyCode.Q)) {
			InitializeGameManager ();
		}
		if (Input.GetKeyDown (KeyCode.W)) {
			StartCoroutine(IntroduceWorld ());
		}
		if (Input.GetKeyDown (KeyCode.E)) {
			AdvanceGameState ();
		}
		if (Input.GetKeyDown (KeyCode.R)) {
			PlayerSchema newPlay = new PlayerSchema ();
			newPlay.faction = "dark";
			newPlay.name = "Donald Duck";
			CreateNewPlayer (newPlay);
		}
		if (Input.GetKeyDown (KeyCode.A)) {
			PlayerSchema newPlay = new PlayerSchema ();
			newPlay.faction = "light";
			newPlay.name = "Scotch Tape";
			CreateNewPlayer (newPlay);
		}


		if (Input.GetKeyDown (KeyCode.S)) {

			if (Random.value > 0.5f) {
				ShipSchema tempShipData = new ShipSchema ();
				tempShipData.name = "TieFighter";
				tempShipData.faction = "dark";
				tempShipData.isStressed = false;
				tempShipData.actions = new List<string> () { "focus", "targetLock" };
				tempShipData.maneuvers = new List<string> () { "jesusTurn", "alley-yoop" };
				tempShipData.hull = 2;
				tempShipData.shield = 2;
				tempShipData.owner = "Donald Duck";
				tempShipData.selectedManeuver = "jesusTurn";
				tempShipData.selectedAction = "focus";
				tempShipData.cost = 1000;
				tempShipData.agility = 10;
				tempShipData.selectedPilot = "Chewbacca";
				//Debug.LogError ("LOOK HERE" + masterJSON.ToString());
				Debug.LogError ("finding pilots" + masterJSON ["ships"] [0] ["pilots"].ToString ());
				tempShipData.pilots = masterJSON ["ships"] [0] ["pilots"].ToString ();

				Debug.Log ("tempdata's pilots : " + tempShipData.pilots);
				
						
				PrizmRecord<ShipSchema> tempShipRecord = new PrizmRecord<ShipSchema> ();
				tempShipRecord.mongoDocument = tempShipData;

				Debug.Log ("player in list: " + playerList [0].ToString ());
			
				giveShipToPlayer (playerList [0].GetComponent<Player> (), tempShipRecord);
			} else {
				ShipSchema tempShipData = new ShipSchema ();
				tempShipData.name = "MillenniumFalcon";
				tempShipData.faction = "light";
				tempShipData.isStressed = false;
				tempShipData.actions = new List<string> () { "focus", "targetLock" };
				tempShipData.maneuvers = new List<string> () { "jesusTurn", "alley-yoop" };
				tempShipData.hull = 2;
				tempShipData.shield = 2;
				tempShipData.owner = "Scotch Tape";
				tempShipData.selectedManeuver = "jesusTurn";
				tempShipData.selectedAction = "focus";
				tempShipData.cost = 1000;
				tempShipData.agility = 10;
				tempShipData.selectedPilot = "Chewbacca";
				//Debug.LogError ("LOOK HERE" + masterJSON.ToString());
				Debug.LogError ("finding pilots" + masterJSON ["ships"] [0] ["pilots"].ToString ());
				tempShipData.pilots = masterJSON ["ships"] [0] ["pilots"].ToString ();

				Debug.Log ("tempdata's pilots : " + tempShipData.pilots);


				PrizmRecord<ShipSchema> tempShipRecord = new PrizmRecord<ShipSchema> ();
				tempShipRecord.mongoDocument = tempShipData;

				Debug.Log ("player in list: " + playerList [0].ToString ());

				giveShipToPlayer (playerList[1].GetComponent<Player> (), tempShipRecord);
				//giveShipToPlayer (playerList [(int)Random.Range (0, playerList.Count)].GetComponent<Player> (), tempShipRecord);
			}
		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			if (Time.timeScale < 0.5f) {
				Time.timeScale = 1.0f;
			} else {
				
				Time.timeScale = 0.1f;
			}
		}

	}

	public Vector3 GetRandomSpawnPosition(string faction) {
		//light spawns on left, dark spawns on right
		switch (faction) {
		case "light":
			return mainCamera.ViewportToWorldPoint(new Vector3(0.05f, Random.value, DistanceFromCamera));

		case "dark":
			return mainCamera.ViewportToWorldPoint(new Vector3(0.95f, Random.value, DistanceFromCamera));

		default:
			Debug.LogError ("invalid faction: " + faction + ", spawning in middle of game");
			return mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, DistanceFromCamera));
		}
	}

	public void AdvanceGameState() {
		//remember to give a UI text indication that sthe stage is advancing


		MyGameState++;
		Debug.Log ("Game State Advance: " + MyGameState.ToString () + " out of: " + System.Enum.GetValues (typeof(GameState)).Length.ToString ());

		if ((int)MyGameState > System.Enum.GetValues (typeof(GameState)).Length - 1) {
			MyGameState = GameState.PlanningPhase;
		}

		switch (MyGameState) {
		case GameState.WaitingForPlayersEnter:
			Debug.Log ("waiting for players to enter");
			break;
		case GameState.WaitingForPlayersChooseShip:
			Debug.Log ("players choosing ship");
			StartCoroutine(IntroduceWorld ());
			break;
		case GameState.PlanningPhase:
			Debug.Log ("players are planning");
			break;
		case GameState.ActivationPhase:
			Debug.Log ("players activating");
			break;
		case GameState.CombatPhase:
			Debug.Log ("entering combat phase");
			break;
		case GameState.EndPhase:
			Debug.Log ("cleanup phase");
			//check if all ships of one faction are dead
			break;
		default:
			Debug.LogError("game state unstable!" + MyGameState.ToString());
			break;
		}
	}

	//called on when a player record is added
	public void CreateNewPlayer(PlayerSchema playerToCreate) {
		createMsgLog (playerToCreate.name + " has joined the game!");
		
		Debug.Log("creating new player in: " + playerToCreate.faction);
		//instantiate player prefab
		GameObject newPlayer = Instantiate(playerPrefab) as GameObject;
		newPlayer.GetComponent<Player> ().record.mongoDocument = playerToCreate;
		newPlayer.GetComponent<Player> ().initializePlayer (playerToCreate.name, playerToCreate.faction, playerToCreate._id);

		newPlayer.transform.SetParent (playerManagerObject.transform);
		playerList.Add (newPlayer);



		numPlayersJoined++;
		if (numPlayersJoined >= numPlayers) {
			//disable record handler


			AdvanceGameState ();
			createMsgLog (" ");
			waitingForText.SetActive (false);
		} else {
			waitingForText.GetComponent<Text> ().text = "Waiting for " + (numPlayers - numPlayersJoined).ToString () + " more players to join\nJoin IP Address:'" + TabletopInitialization.GetIP () + ":6969'";
		}
	}
		

	//when someone quits the game
	public void HandleDidLosePlayer(string id) {
		foreach(GameObject obj in Object.FindObjectsOfType(typeof(GameObject))){
			if(obj.tag == "Player"){
				//probably destroy the player object
			}
		}

		Debug.Log ("player lost connection, object is: " + id);
	}





	public IEnumerator ShakeCamera(float innerDuration = 0.02f, int iterations = 10) {
		Vector3 newPos;
		Vector3 cameraSetPosition = mainCamera.transform.position;
		newPos = mainCamera.transform.position;
		Debug.Log (newPos.ToString ());

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
	public void CreateBoundaries() {

		Vector3 lowerLeft = mainCamera.ViewportToWorldPoint (new Vector3 (0, 0, DistanceFromCamera));
		Vector3 lowerRight = mainCamera.ViewportToWorldPoint (new Vector3 (1, 0, DistanceFromCamera));
		Vector3 upperLeft = mainCamera.ViewportToWorldPoint (new Vector3 (0, 1, DistanceFromCamera));
		Vector3 upperRight = mainCamera.ViewportToWorldPoint (new Vector3 (1, 1, DistanceFromCamera));

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


	void OnApplicationQuit(){
		StopAllCoroutines ();
		reset ();
	}

	public void reset(){
		StartCoroutine (resetGame ());
	}

	IEnumerator resetGame() {
		var methodCall = Meteor.Method<ChannelResponse>.Call ("endTabletopSession", GameObject.Find ("GameManager").GetComponent<TabletopInitialization>().sessionID);
		yield return (Coroutine)methodCall;
	}




}