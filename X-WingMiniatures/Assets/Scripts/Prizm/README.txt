With server running, access MongoDB visualizer at http://localhost:6969/admin/
On first access, set up a username/password (i.e. "admin"/"pass")


Example 1: How to create a new database item, like a road. And also instantiate that thing in the game world. 
The example should be in code that is also carefully commented. 
I need to see the procedure for this using all related steps. 
I also need to see careful labels where each code snippet belongs.

A) Create a PrizmRecordGroup
@TabletopInitialization.cs

	roadRecordGroup = new PrizmRecordGroup<RoadSchema> (sessionID, "roads");
		yield return StartCoroutine (roadRecordGroup.CreateMeteorCollection ());
		
B) Add event handlers for MongoDB documents

@TabletopInitialization.cs

// NEW DOCUMENTS
	roadRecordGroup.mongoCollection.DidAddRecord += (string arg1, RoadSchema arg2) => {
			Debug.Log ("New road added!" + arg1 + " ID is: " + arg2._id + " Color is: " + arg2.myColor + " Key is:" + arg2.key + " isPlaced is:" + arg2.isPlaced);

			PrizmRecord<RoadSchema> record = roadRecordGroup.LookUpPrizmRecordBy_ID(arg2._id);

			if (record==null) return;
			giveRoadToPlayer(record.mongoDocument.myOwner, record);
		};

// UPDATED DOCUMENTS
		roadRecordGroup.mongoCollection.DidChangeRecord += (string arg1, RoadSchema arg2, IDictionary arg3, string[] arg4) => {
			Debug.Log ("Road changed! SessionID: "+arg1+" key is: "+arg2.key+" Owner is: "+arg2.myOwner+" color is: "+arg2.myColor+" location is: "+arg2.location);
			
			PrizmRecord<RoadSchema> record = roadRecordGroup.LookUpPrizmRecordBy_ID(arg2._id);
			if (record==null) return;
			
			GameObject obj = record.gameObject;
			// ...do something with GameObject
			Road road = obj.GetComponent<Road>();
			// ...do something with Road
		};

// DELETED DOCUMENTS
		roadRecordGroup.mongoCollection.DidRemoveRecord += (string arg1) => {
			Debug.Log ("Road removed! _id: "+arg1);
			
			PrizmRecord<RoadSchema> record = roadRecordGroup.LookUpPrizmRecordBy_ID(arg1);
			if (record!=null) Destroy(record.gameObject);
		};

C) Create PrizmRecords, set the data for the mongo document, and add them to the appropriate PrizmRecordGroup

@TabletopInitialization.cs
            
				PrizmRecord<RoadSchema> roadRecord = new PrizmRecord<RoadSchema> ();
				roadRecord.mongoDocument.myColor = ply.MyColor;
				roadRecord.mongoDocument.isPlaced = false;
				roadRecord.mongoDocument.myOwner = ply;

				yield return StartCoroutine (roadRecordGroup.AddRecord (roadRecord));
				while (roadRecord.gameObject==null){
					Debug.Log ("waiting for database confirmation");
					yield return null;
				}

D) Instantiate GameObjects with a script component that can be paired with the PrizmRecord/mongo document
Do this in the PrizmRecordGroup.DidAddRecord delegate

@TabletopInitialization.cs

	roadRecordGroup.mongoCollection.DidAddRecord += (string arg1, RoadSchema arg2) => {
			Debug.Log ("New road added!" + arg1 + " ID is: " + arg2._id + " Color is: " + arg2.myColor + " Key is:" + arg2.key + " isPlaced is:" + arg2.isPlaced);

			PrizmRecord<RoadSchema> record = roadRecordGroup.LookUpPrizmRecordBy_ID(arg2._id);

			if (record==null) return;
			giveRoadToPlayer(record.mongoDocument.myOwner, record);
		};
	
	//...


	void giveRoadToPlayer(Player ply, PrizmRecord<RoadSchema> roadRecord) {

		GameObject road_obj = Instantiate (Resources.Load<GameObject> ("Roads/Road_Prefab_WColliders_" + ply.MyColor));
		roadRecord.gameObject = road_obj;
		road_obj.GetComponent<Road> ().record = roadRecord;
		
		ply.MyRoadsInHand.Add (road_obj.GetComponent<Road> ()); 
		road_obj.transform.SetParent (ply.transform.FindChild("player_road_holder"));
		road_obj.transform.position = road_obj.transform.parent.position;
	}


Ensure that the script component to be paired with PrizmRecord has public field of type PrizmRecord<MongoSchema> for this assocation:

@Road.cs
	public class Road : MonoBehaviour {

		public PrizmRecord<RoadSchema> record;
	//...
	}

@PrizmRecord.cs

	public class PrizmRecord<TMongoDocument> where TMongoDocument : MongoSchema, new() {

		public GameObject gameObject;
		//...
	}




Example 2: How to update a property of a database object. 
For instance this could mean changing the location of a road from "home" to "player1." 
Another great example would be changing the owner of a resource card. 

A) Create a function on the script component that updates the mongoDocument fields of the paired PrizmRecord
Sync that PrizmRecord to database

@Road.cs
	public void PlaceMe() {
		Debug.Log ("PlaceMe()");
		record.mongoDocument.isPlaced = true;
		StartCoroutine (record.Sync ());
	}


B) When the database operation is comfirmed in the corresponding PrizmRecordGroup.DidChangeRecord, target appropriate the PrizmRecord/GameObject pair

@TabletopInitialization.cs
	roadRecordGroup.mongoCollection.DidChangeRecord += (string arg1, RoadSchema arg2, IDictionary arg3, string[] arg4) => {
			Debug.Log ("Road changed! SessionID: "+arg1+" key is: "+arg2.key+" Owner is: "+arg2.myOwner+" color is: "+arg2.myColor+" location is: "+arg2.location);
			
			PrizmRecord<RoadSchema> record = roadRecordGroup.LookUpPrizmRecordBy_ID(arg2._id);
			if (record==null) return;
			
			GameObject obj = record.gameObject;
			// ...do something with GameObject
			Road road = obj.GetComponent<Road>();
			// ...do something with Road
		};




Example 3: Properly removing a gameobject and its corresponding database record. 
Again, this needs to include all steps that the develops needs to use. 

A) Modify the paired script component OnDestroy() to destroy the associated PrizmRecord

@Road.cs

	void OnDestroy() {
		Debug.Log ("destroying road..");
		
		if (this.record != null) CoroutineHost.Instance.StartCoroutine (this.record.Destroy ());
	}
	
	
B) Implement changes to the associated GameObject in PrizmRecordGroup.DidRemoveRecord

@TabletopInitialization.cs
 	
 	roadRecordGroup.mongoCollection.DidRemoveRecord += (string arg1) => {
			Debug.Log ("Road removed! _id: "+arg1);
			
			PrizmRecord<RoadSchema> record = roadRecordGroup.LookUpPrizmRecordBy_ID(arg1);
			if (record!=null) Destroy(record.gameObject);
		}; 
