using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TouchScript;
using TouchScript.Gestures;
using UnityEngine.UI;

public enum GameState {
	None,
	WaitingForPlayersEnter, 
	WaitingForPlayersChooseShip, 
	PlanningPhase, 
	ActivationPhase, 
	CombatPhase,
	EndPhase
};


public class GameManager : MonoBehaviour {
	public GameObject playerPrefab;

	//public PrizmRecordGroup recordGroup; 
	public BootstrapTT bootstrap; 

	public List<GameObject> resourcePrefabs;
	//public List<GameObject> listOfTerrainTilesInOrder;

	private GameObject cube_ThatRepresentsPlayer;

	private Player currentPlayerTurn;
	public Player CurrentPlayerTurn{ get { return currentPlayerTurn; } set { currentPlayerTurn = value; } }
	private bool beginnerGame;
	public bool BeginnerGame{ get { return beginnerGame; } set { beginnerGame = value; } }
	public List<Player> PlayersRegisteredBeforeTurnOrder;
	public List<Player> TheOneOfficialAllPlayerListInOrderOfTurns;
	//public GameObject dockTokenPrefab;
	private bool playerDockTokensGenerated;

	public GameState MyGameState;

	public static GameManager Instance{ get; private set; }
	private int playerNum;

	public Button start_btn;

	public List<string> ListOfDummyNames;
	public GameObject DockCardPrefab;
	public GameObject bouncingArrow_Prefab;
	public GameObject private_tut;

	private bool firstPlayerChosen;

	public delegate void TurnChangeAction(Player ply);
	public static event TurnChangeAction OnTurnChange;

	private TextMesh gameStateReporter;
	public TextMesh GameStateReporter{ get { return gameStateReporter; } set { gameStateReporter = value; } }

	private TextMesh public_TutorialText;
	public TextMesh PublicTutorialText{ get { return public_TutorialText; } set {public_TutorialText = value; } }

	//private List<Establishment> establishmentsPlacedOnBoard;
	public List<Establishment> EstablishmentsPlacedOnBoard;

	//private List<Road> roadsPlacedOnBoard;
	public List<Road> RoadsPlacedOnBoard;

	public List<Resource> ResourcesOnBoardWaitingToBeCollected;
	private int resourcesCollected;
	public int ResourcesCollected{ get { return resourcesCollected; } set { resourcesCollected = value; } }

	
	private bool allplayersplacedInitialSettlements;
	public bool AllPlayersplacedInitialRoadsSettlements{ get { return allplayersplacedInitialSettlements; } set { allplayersplacedInitialSettlements = value; } }

	public List<Color> ColorValuesInGame;
	public List<string> ColorNamesInGame;

	public List<Terrain_Script> AllHexTerrainsInGame;

	void Awake () {
		Instance = this;
		//recordGroup = GetComponent<PrizmRecordGroup>();
		bootstrap = GetComponent<BootstrapTT> ();
	}

	void Start(){
		MyGameState = GameState.SetupHexTilesNumberTokensAndHarbors;
		GameStateReporter = GameObject.FindGameObjectWithTag ("GameState_Reporter").GetComponent<TextMesh> ();
		PublicTutorialText = GameObject.FindGameObjectWithTag ("Public_Tutorial").transform.FindChild ("tut_text").GetComponent<TextMesh> ();
		StartCoroutine (SetupGame ());
	}

	public IEnumerator SetupGame(){
//		while (!MyGameState.Equals(GameState.SetupHexTilesNumberTokensAndHarbors)) {
//			yield return null;
//		}
		GameStateReporter.text = "GAME State = setting up hex tiles and harbors";
		yield return StartCoroutine (Setup_Manager.Instance.SetupHexTiles ());
		MyGameState = GameState.WaitingToRegisterAllPlayersViaMobile;
		while (!MyGameState.Equals(GameState.WaitingToRegisterAllPlayersViaMobile)) {
			yield return null;
		}
		GameStateReporter.text = "Waiting for all players to register via Mobile Device";
		PublicTutorialText.text = "Sign in to play using your\nmobile device.";
		while (!MyGameState.Equals(GameState.DockPlayerCardsAndEstablishTurnOrder)) {
			yield return null;
		}
		GameStateReporter.text = "Player should pull Circular tokens \ntowards their edge of the board";
		PublicTutorialText.text = "Pull your card towards\nyour edge of the board";
		if (!playerDockTokensGenerated) {
			int i = 0;
			foreach (Player ply in PlayersRegisteredBeforeTurnOrder) {
				GameObject dockCrd = Instantiate (DockCardPrefab, new Vector3 (Random.Range (-12, 12), 10, Random.Range (-4, 4)), Quaternion.identity) as GameObject;
				dockCrd.GetComponent<DockCard>().DockCardColorValue = ColorValuesInGame[i];
				dockCrd.GetComponent<DockCard>().DockCardColor = ColorNamesInGame[i];
				
				dockCrd.transform.SetParent (GameObject.FindGameObjectWithTag ("DockCard_Manager").transform);
				//dockCrd.transform.FindChild ("PlayerNameOnToken").GetComponent<TextMesh> ().text = ply.MyName;
				/**This is where you will customize the dock card for a player
				 **/

				dockCrd.GetComponent<DockCard> ().MyPlayer = ply;
				ply.MyDockCard = dockCrd.GetComponent<DockCard> ();
				ply.MyColor = dockCrd.GetComponent<DockCard> ().DockCardColor;
				ply.MyColorValue = dockCrd.GetComponent<DockCard> ().DockCardColorValue;
				dockCrd.transform.FindChild ("Canvas/p_header/t_player_name").GetComponent<Text>().text = ply.MyName;
				dockCrd.transform.FindChild ("Canvas/p_header/i_color_banner").GetComponent<Image>().color = ply.MyColorValue;
				i++;
					
			}
			playerDockTokensGenerated = true;
		}

		while (!MyGameState.Equals(GameState.PlaceInitialSettlementsAndRoads)) {
			yield return null;
		}
		GameStateReporter.text = "Please Wait. Game is creating DB records and\ndistributing all starting items to PLAYERS";
		PublicTutorialText.text = "Please Wait.\nSetting up behind the scenes.";
		yield return StartCoroutine (TabletopInitialization.Instance.GivePlayerStartingItems ());
		GameStateReporter.text = "All players should place one settlement on the board";
		PublicTutorialText.text = "Each player may place one \nsettlement at an intersection\non the board.";
		if (!firstPlayerChosen) {				
			Debug.Log ("Gave Players Starting Items");
			ChooseFirstPlayer ();
			//give every player two starting items.
		} 
		//Give players their first settlement to place
		foreach(Player ply in TheOneOfficialAllPlayerListInOrderOfTurns){
			ply.GivePlayerASettlementToPlace();
		}
//		yield return StartCoroutine (TabletopInitialization.Instance.GivePlayerStartingItems ());
		yield return StartCoroutine (TabletopInitialization.Instance.CreateAllResourceCardsAndDevelopmentCards());
		while(EstablishmentsPlacedOnBoard.Count < TheOneOfficialAllPlayerListInOrderOfTurns.Count){
			yield return null;
		}
		//give all players their second establishment to place.
		PublicTutorialText.text = "Each player may place their 2nd \nsettlement at an intersection\non the board.";
		foreach(Player ply in TheOneOfficialAllPlayerListInOrderOfTurns){
			ply.GivePlayerASettlementToPlace();
		}
		while(EstablishmentsPlacedOnBoard.Count < TheOneOfficialAllPlayerListInOrderOfTurns.Count*2){
			yield return null;
		}
		//give all players their first road to place.
		PublicTutorialText.text = "Each player may place their 1st \nroad at any intersection on the board\nconnected to one of their \nsettlements.";

		foreach(Player ply in TheOneOfficialAllPlayerListInOrderOfTurns){
			ply.GivePlayerARoadToPlace();
		}
		while(RoadsPlacedOnBoard.Count < TheOneOfficialAllPlayerListInOrderOfTurns.Count){
			yield return null;
		}
		//give all players their 2nd road to place.
		PublicTutorialText.text = "Each player may place their 2nd \nroad at any intersection on the board\nconnected to one of their \nsettlements or roads.";
		foreach(Player ply in TheOneOfficialAllPlayerListInOrderOfTurns){
			ply.GivePlayerARoadToPlace();
		}
		while(RoadsPlacedOnBoard.Count < TheOneOfficialAllPlayerListInOrderOfTurns.Count*2){
			yield return null;
		}
		PublicTutorialText.text = "Each player collects \nthe resources from their second\nsettlement. Drag the resource\n to your wallet/pouch.\nIt will become visible on\nyour mobile device.";

		//Give All Player their resources from the 2nd settlement they placed. 
		foreach (Player ply in TheOneOfficialAllPlayerListInOrderOfTurns) {
			ply.MySecondEstablishmentPlacedDuringSetup.SpawnInitialResources();
		}

		while (ResourcesOnBoardWaitingToBeCollected.Count > ResourcesCollected) {
			yield return null;
		}
		ResourcesOnBoardWaitingToBeCollected.Clear ();
		ResourcesCollected = 0;

		Debug.Log ("Whew  ^^^^^^^^^ we got through a lot of setup");
		AdvanceTurn(CurrentPlayerTurn);
		PublicTutorialText.text = "Now for the first players first\nofficial turn of the game.\n " +CurrentPlayerTurn.MyName + ": \nPlease Roll to produce resources.";

		yield break;



//		case GameState.GenerateStartingResourcesForPlayers:
//			break;
//		case GameState.PlayerTurn_RollPhase:
//			break;
//		case GameState.PlayerTurn_CollectBountyPhase:
//			break;
//		case GameState.PlayerTurn_TradePhase_Domestic:
//			break;
//		case GameState.PlayerTurn_TradePhase_Maritime:
//			break;
//		case GameState.PlayerTurn_BuildPhase:
//			break;
//		case GameState.PlayerTurn_BuyDevCards:
//			break;
//		case GameState.PlayerTurn_UseDevCard:
//			break;
//		case GameState.PlayerTurn_Robber_EveryOneReturnIfExceeding7:
//			break;
//		case GameState.PlayerTurn_Robber_RobOtherPlayer:
//			break;
//		default:
//			break;
//		}

	}

	public void ChooseFirstPlayer(){
		if (!firstPlayerChosen) {
			int rand = Random.Range (0, (TheOneOfficialAllPlayerListInOrderOfTurns.Count-1));
			Debug.Log ("the first player that I chose was: " + rand);
			//OnTurnChange (TheOneOfficialAllPlayerListInOrderOfTurns [rand]);
			CurrentPlayerTurn = TheOneOfficialAllPlayerListInOrderOfTurns [rand];
			if(TheOneOfficialAllPlayerListInOrderOfTurns.Count >= 5){
				Debug.Log ("Playing with 5 OR 6 players");
			}
			firstPlayerChosen = true;
		}
	}



	public void AdvanceTurn(Player _currentPlayer){
		Debug.Log ("Advancing the turn");
		int playerTurnNum = TheOneOfficialAllPlayerListInOrderOfTurns.IndexOf (_currentPlayer);
		Debug.Log ("Player turn BEFORE TURN CHANGE" + _currentPlayer.MyTurnNumID);
		playerTurnNum++;
		if (playerTurnNum > TheOneOfficialAllPlayerListInOrderOfTurns.Count-1) {
			Debug.Log ("plyaerTurn Num being set to zero ");
			playerTurnNum = 0;
		} 
		CurrentPlayerTurn = TheOneOfficialAllPlayerListInOrderOfTurns [playerTurnNum];
		OnTurnChange(TheOneOfficialAllPlayerListInOrderOfTurns[playerTurnNum]);
		MyGameState = GameState.PlayerTurn_RollPhase;
	}

	//the devCard deck in the corner of the board should be highlighted throughout all of the player turn
	//this player action can disrupt the normal turn phase order
	public void UseDevelopmentCard(GameState bookMarkGameState){

	}

	public void SetupTradeEnvironment(){

	}

	//this might involve putting down an invisible layer that the dice can bounce around inside 
	public void SetupDiceRollingEnvironment(){

	}

	public void CheckIfResourcesLeftUncollectedOnBoard(){

	}

	public void TradePhase_CheckIfANyResourceCardsLeftUnclaimed(){

	}


	public void CreateNewPlayerTest0(){
		int i = GameObject.FindGameObjectWithTag ("Player_Manager").transform.childCount;
		Debug.Log ("Child Count on Players = " + i);
		if (i < 6) {
			cube_ThatRepresentsPlayer = Instantiate (playerPrefab) as GameObject;
			cube_ThatRepresentsPlayer.name = ListOfDummyNames[i];
			cube_ThatRepresentsPlayer.GetComponent<Player>().MyName = ListOfDummyNames[i];
			cube_ThatRepresentsPlayer.transform.SetParent (GameObject.FindGameObjectWithTag ("Player_Manager").transform);
			cube_ThatRepresentsPlayer.GetComponent<MeshRenderer> ().material.color = Color.black;
			cube_ThatRepresentsPlayer.transform.position = cube_ThatRepresentsPlayer.transform.parent.transform.position;
			cube_ThatRepresentsPlayer.transform.position = new Vector3 (cube_ThatRepresentsPlayer.transform.position.x + i, cube_ThatRepresentsPlayer.transform.position.y, cube_ThatRepresentsPlayer.transform.position.z);
			PlayersRegisteredBeforeTurnOrder.Add (cube_ThatRepresentsPlayer.GetComponent<Player>());
		}
		if(GameObject.FindGameObjectWithTag ("Player_Manager").transform.childCount >= 2){
			start_btn.interactable = true;
		}
	}
	
	public void CreateNewPlayer(string playerName, string player_id) {
		int i = GameObject.FindGameObjectWithTag ("Player_Manager").transform.childCount;

		if (i < 6) {
			//Debug.Log ("Tap the cube.");
			cube_ThatRepresentsPlayer = Instantiate (playerPrefab) as GameObject;
			cube_ThatRepresentsPlayer.name = playerName;
			cube_ThatRepresentsPlayer.GetComponent<Player>().MyName = playerName;
//			cube_ThatRepresentsPlayer.GetComponent<touchMe> ().playerID = player_id;
//			cube_ThatRepresentsPlayer.GetComponent<touchMe> ().dbEntry.location = "home";
//			cube_ThatRepresentsPlayer.GetComponent<touchMe> ().AddToRecordGroup ();
			cube_ThatRepresentsPlayer.transform.SetParent (GameObject.FindGameObjectWithTag ("Player_Manager").transform);
			cube_ThatRepresentsPlayer.GetComponent<MeshRenderer> ().material.color = Color.red;
			cube_ThatRepresentsPlayer.transform.position = cube_ThatRepresentsPlayer.transform.parent.transform.position;
			cube_ThatRepresentsPlayer.transform.position = new Vector3 (cube_ThatRepresentsPlayer.transform.position.x + i, cube_ThatRepresentsPlayer.transform.position.y, cube_ThatRepresentsPlayer.transform.position.z);
			PlayersRegisteredBeforeTurnOrder.Add (cube_ThatRepresentsPlayer.GetComponent<Player>());
		}
		if(GameObject.FindGameObjectWithTag ("Player_Manager").transform.childCount >= 2){
			start_btn.interactable = true;
		}
	}





	
//	public void HandleDidChangeRecord (string arg1, DatabaseEntry arg2, IDictionary arg3, string[] arg4){
//		Debug.Log ("record changed: " + arg2.location);
//		if (arg2.location == "home") {
//			cube_ThatRepresentsPlayer.SetActive(true);
//		} 
//	}

	//when someone quits the game
	public void HandleDidLosePlayer(string id) {
		foreach(GameObject obj in Object.FindObjectsOfType(typeof(GameObject))){
			if(obj.tag == "Player"){
//				if(obj.GetComponent<touchMe>().playerID == id){
//					Destroy(obj);
//				}
			}
		}

		Debug.Log ("player lost connection, object is: " + id);

	}

	void OnApplicationQuit(){
		reset ();
	}
	
	public void reset(){
		StartCoroutine (resetGame ());
	}

	IEnumerator resetGame() {
		var methodCall = Meteor.Method<ChannelResponse>.Call ("endTabletopSession", GameObject.Find ("GameManager_TT").GetComponent<TabletopInitialization>().sessionID);
		yield return (Coroutine)methodCall;
		//reset the scene, make visual indicator
	}
}