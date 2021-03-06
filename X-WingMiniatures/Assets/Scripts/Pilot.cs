﻿using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

public class Pilot {
	public string name;
	public int weapon;
	public int agility;
	public int hull;
	public int shield;
	public string ability;
	public bool isUnique;
	public int cost;
	public int skill;

	List<string> upgrades = new List<string>();

	//finds the pilot's name from a json config file to initialize them
	public Pilot(JSONNode pilotJsonData) {
		var jsonObj = pilotJsonData;
		name = jsonObj ["name"];
		weapon = jsonObj ["weapon"].AsInt;
		agility = jsonObj ["agility"].AsInt;
		hull = jsonObj ["hull"].AsInt;
		shield = jsonObj ["shield"].AsInt;
		ability = jsonObj ["ability"];
		isUnique = jsonObj ["isUnique"].AsBool;
		cost = jsonObj ["cost"].AsInt;
		skill = jsonObj ["skill"].AsInt;
	}


	public Pilot(ShipSchema.PilotData pilotData) {
		name = pilotData.name;
		weapon = pilotData.weapon;
		agility = pilotData.agility;
		hull = pilotData.hull;
		shield = pilotData.shield;
		ability = pilotData.ability;
		isUnique = pilotData.isUnique;
		cost = pilotData.cost;
		skill = pilotData.skill;
	}

}
