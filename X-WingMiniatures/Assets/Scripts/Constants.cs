using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;


public enum clientStatuses {paired=0, groupsSynced, uiReady, playerReady, waiting, running, paused, ended}
public enum sessionStatuses {created=0, allPaired, allGroupsSynced, allUiReady, allPlayersReady, running, paused, ended}


public abstract class MongoSchema: Meteor.MongoDocument {
	public string key;
	public System.Guid _GUID;

	public abstract Dictionary<string, object> toDictionary();
}

public class PlayerSchema: MongoSchema {
	public string name;
	public List<string> ships = new List<string>();
	public string faction;	//"light" or "dark"
	public string session_id;

	public override Dictionary<string, object> toDictionary() {
		Dictionary<string, object> dictionary = new Dictionary<string, object> () {
			{"key",key},
			{"_GUID", _GUID},
			{"name", name},
			{"ships", ships.ToString()},	//test this as a list
			{"faction", faction}
		};	
		return dictionary;
	}
}



public class ShipSchema: MongoSchema {
	public string name = "";
	public string owner = "";
	public int weapon = 0;
	public int agility = 0;
	public int currentShield = 0;
	public int currentHull = 0;
	public string faction = "";
	public bool isStressed = false;
	public string selectedPilot = "";
	public int cost = 0;


	public List<string> actions = new List<string>();
	public string selectedAction  = "";

	public string selectedManeuver = "";
	public List<Maneuver> maneuvers = new List<Maneuver>(); 	//json array of json objects
	public List<string> selectedUpgrades = new List<string>();
	public List<PilotData> pilots = new List<PilotData>();	//json array of json objects

	public override Dictionary<string, object> toDictionary() {

		Dictionary<string, object> dictionary = new Dictionary<string, object> ();

		dictionary.Add("key",key);
		dictionary.Add("_GUID", _GUID);
		dictionary.Add("name", name);
		dictionary.Add("owner", owner);
		dictionary.Add("weapon", weapon.ToString());
		dictionary.Add("agility", agility.ToString());
		dictionary.Add("currentShield", currentShield.ToString());
		dictionary.Add("currentHull", currentHull.ToString());
		dictionary.Add("faction", faction);
		dictionary.Add("isStressed", isStressed);
		dictionary.Add("selectedPilot", selectedPilot);
		dictionary.Add("cost", cost.ToString());

		dictionary.Add("actions", actions.ToString());

		dictionary.Add("selectedAction", selectedAction);
		dictionary.Add("selectedManeuver", selectedManeuver);
		dictionary.Add("maneuvers", maneuvers.ToString());
		dictionary.Add ("pilots", pilots.ToString ());

		/*
		{

			{"key",key},
			{"_GUID", _GUID},
			{"name", name},
			{"owner", owner},
			{"weapon", weapon.ToString()},
			{"agility", agility.ToString()},
			{"shield", shield.ToString()},
			{"hull", hull.ToString()},
			{"faction", faction},
			{"isStressed", isStressed.ToString()},
			{"selectedPilot", selectedPilot},
			{"cost", cost.ToString()},

			{"upgrades", upgrades.ToString()},
			{"actions", actions.ToString()},

			{"selectedAction", selectedAction},
			{"selectedManeuver", selectedManeuver},
			{"maneuvers", maneuvers.ToString()},
			{"pilots", pilots.ToString()}

		} ;	

*/
		return dictionary;
	}



	[Serializable]
	public class Maneuver
	{
		public int speed;
		public string direction;
		public int difficulty;

	}

	[Serializable]
	public class PilotData
	{
		public string name;
		public string ability;
		public bool isUnique;
		public int cost;
		public int skill;
		public int weapon;
		public int agility;
		public int hull;
		public int shield;
		public List<string> upgrades = new List<string>();
	}




}

public class UpgradeSchema: MongoSchema {
	public string name;
	public string type;
	public int cost;
	public string ability;
	public bool abilityIsUnique = false;
	
	public override Dictionary<string, object> toDictionary() {
		Dictionary<string, object> dictionary = new Dictionary<string, object> () {
			{"key",key},
			{"_GUID", _GUID},
			{"name", name},
			{"type", type},
			{"cost", cost.ToString()},
			{"ability", ability},
			{"abilityIsUnique", abilityIsUnique.ToString()}
		} ;	
		return dictionary;
	}
}

[Serializable]
public class Upgrade
{
	public string name;
	public string type;
	public int cost;
	public string ability;
	public bool isUnique;
}

public class ObstacleSchema: MongoSchema{
	public int size;
	public int[] position = new int[2];
	
	public override Dictionary<string, object> toDictionary() {
		Dictionary<string, object> dictionary = new Dictionary<string, object> () {
			{"key",key},
			{"_GUID", _GUID},
			{"size", size.ToString()},
			{"position", position.ToString()},
		} ;	
		return dictionary;
	}
}

//class used when first opening session
public class OpenSessionResponse : Meteor.MongoDocument {
	public bool success;
	public string sessionID;
	public string clientID;
	public string sessionName;
}

//channel response for any record changes
public class ChannelTemplate : Meteor.MongoDocument {
	public string session_id = "";
	public string sender_id = "";
	public string receiver_id = "";
	public string payload = "";
}

//Channel Repsonse class returned by reportToTabletopClient
public class ChannelResponse : Meteor.MongoDocument {
	public bool success;
	public string message = "";
}

//Parameters that a client record has in the client channel
public class ClientTemplate : Meteor.MongoDocument {
	public string sessionID = "";
	public string deviceType = "";
	public string deviceID = "";
	public string state = "";
	public string currentPlayer = "";
}

//parameters that a session record has in the session channel
public class SessionTemplate : Meteor.MongoDocument {
	public string appID = "";
	public string tabletopDeviceID = "";
	public List<string> groups = new List<string>();
	public List<string> players = new List<string>();
	public string currentPlayer = "";
	public string name = "";
}

//used when creating playerchannel 
public class PlayerTemplate : Meteor.MongoDocument {
	public string playerID = "";
	public string name = "";

}
