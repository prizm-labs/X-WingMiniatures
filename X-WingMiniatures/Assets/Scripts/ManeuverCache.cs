﻿using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

public class ManeuverCache : MonoBehaviour {

	[System.NonSerialized]
	public static float baseSegmentMultiple = 5.0f;
	public static int totalMovementSegments = 5;


	private static List<Vector3> skeletonPoints = new List<Vector3>();

	public static List<Vector3> GetManeuverPoints(Transform shipT, JSONNode maneuverDetails) {

		//initialize needed variables
		string direction = maneuverDetails ["direction"];
		Debug.Log ("taking direction: " + direction);
		int speed = maneuverDetails ["speed"].AsInt;
		Debug.Log ("taking speed: " + speed.ToString ());
		float segmentDistance = speed * baseSegmentMultiple;

		//clear points
		skeletonPoints.Clear ();


		//generate spine
		for (int i = 0; i < totalMovementSegments; i++) {
			skeletonPoints.Add (shipT.position + shipT.forward * segmentDistance * i);
		}


		switch (direction) {
		case "bankLeft":
			for (int i = totalMovementSegments - 1; i > totalMovementSegments / 2; i--) {
				skeletonPoints [i] += -shipT.right * segmentDistance;
			}
			break;
		case "bankRight":
			for (int i = totalMovementSegments - 1; i > totalMovementSegments / 2; i--) {
				skeletonPoints [i] += shipT.right * segmentDistance;
			}
			break;
		case "turnLeft":
			for (int i = totalMovementSegments - 1; i > totalMovementSegments / 5; i--) {
				skeletonPoints [i] += -shipT.right * segmentDistance;
			}
			break;
		case "turnRight":
			for (int i = totalMovementSegments - 1; i > totalMovementSegments / 2; i--) {
				skeletonPoints [i] += shipT.right * segmentDistance;
			}
			break;
		default:				//covers "straight" and "kturn"
			Debug.Log ("stiraght or k-turn, don't do anythign else");
			break;
		}

		return skeletonPoints;
	}






}