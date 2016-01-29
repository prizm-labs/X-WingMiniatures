using UnityEngine;
using System.Collections;
using Extensions;
using System.Collections.Generic;


public class PrizmRecordGroup<TMongoDocument>
where TMongoDocument : MongoSchema, new() {

//	mongoCollection.DidAddRecord += (string arg1, MongoSchema arg2) => ;
//	mongoCollection.DidChangeRecord += (string arg1, MongoSchema arg2, IDictionary arg3, string[] arg4) => ;
//	mongoCollection.DidRemoveRecord += (string obj) => ;

	private Dictionary<System.Guid,PrizmRecord<TMongoDocument>> unboundAssociates;

	public delegate void DidAddRecord(string arg1, TMongoDocument arg2);
	public DidAddRecord didAddRecord;

	public delegate void DidChangeRecord(string arg1, TMongoDocument arg2, IDictionary arg3, string[] arg4);
	public DidChangeRecord didChangeRecord;

	public delegate void DidRemoveRecord(string obj);
	public DidRemoveRecord didRemoveRecord;

	public Meteor.Collection<TMongoDocument> mongoCollection;
	public List<PrizmRecord<TMongoDocument>> associates;							//list of all PrizmRecords that belong to this recordGroup
	
	public string collectionKey;
	public string sessionID;


	public PrizmRecordGroup(string sessionID, string collectionKey){
		this.associates = new List<PrizmRecord<TMongoDocument>> ();
		this.unboundAssociates = new Dictionary<System.Guid,PrizmRecord<TMongoDocument>> ();
		this.collectionKey = collectionKey;
		this.sessionID = sessionID;
	}


	public PrizmRecord<TMongoDocument> LookUpPrizmRecordBy_ID(string id){

		//search through all associates and find the one with this id. 
		foreach (PrizmRecord<TMongoDocument> record in associates) {
			if(record.mongoDocument._id == id){
				return record;
			}
		}
		Debug.Log ("id not found in this collectionKey"+collectionKey+ " in associates");
		return null;
	}

	private void bindRecord (TMongoDocument mongoDoc) {
		Debug.Log ("before");
		PrizmRecord<TMongoDocument> unboundRecord = unboundAssociates[mongoDoc._GUID];
		Debug.Log ("before");
		unboundRecord.mongoDocument._id = mongoDoc._id;
		unboundRecord.mongoDocument.key = this.collectionKey;

		unboundAssociates.Remove (mongoDoc._GUID);
	}

	public IEnumerator AddRecord (PrizmRecord<TMongoDocument> record) {
		Debug.Log ("trying to add record");

		record.recordGroup = this;
		record.mongoDocument._GUID = System.Guid.NewGuid ();
		record.mongoDocument.key = this.collectionKey;

		associates.Add(record);	
		unboundAssociates.Add (record.mongoDocument._GUID, record);

		Debug.Log ("AddRecord: "+record.mongoDocument.toDictionary()["key"]);

		var methodCall = Meteor.Method<ChannelResponse>.Call ("addGameObject", this.sessionID, collectionKey, record.mongoDocument.toDictionary());
		yield return (Coroutine)methodCall;

		if (methodCall.Response.success) {

			// at this time, mongoCollection.DidAddRecord should have bound the _id to its record 
//			bindRecord(methodCall.Response.message);
			Debug.Log ("AFTER updateGameObject, response: " + methodCall.Response.message + ", key:"+record.mongoDocument.toDictionary()["key"]);



		} else {
			Debug.LogError ("uh oh! call to 'addGameObject' failed! Response: " + methodCall.Response.message);
		}

		yield return null;
	}

	public IEnumerator SyncAll() {
		Debug.Log ("Begin syncing database");

		for (int i = 0; i < associates.Count; i++) {			//go through list of associates and look for ones that need to be synced

			if (associates[i].needsUpdate == true) {

				var methodCall = Meteor.Method<ChannelResponse>.Call ("updateGameObject", associates[i].mongoDocument._id, associates[i].mongoDocument.toDictionary(), collectionKey);
				yield return (Coroutine)methodCall;

				if (methodCall.Response.success) {
					//Debug.LogError (associates[i].dbEntry._id + " should = " + methodCall.Response.message);
					associates[i].dbUpdated();		//tells the record that it was updated and it can rest now
				} else {
					//Debug.LogError ("Uh oh! database sync failed on record: " + associates[i].name + ", with _id: " + associates[i].mongoDocument._id);
				}

				Debug.Log ("AFTER updateGameObject");
				yield return null;
			}
		}
		Debug.LogError ("Finished with SyncAll()");
		yield return null;
	}

	//sync all objects with 'needsUpdate' flag to database
	//developer calls at own discretion
	public IEnumerator Sync(PrizmRecord<TMongoDocument> itemToSync) {
		Debug.Log ("PrizmRecordGroup Sync() " + this.collectionKey);

		if (itemToSync.needsUpdate) {
			//forms a dictionary to pass into meteor's 'updateGameObject' from the record's databaseEntry parameters
			//simplify this for the developer in the future (maybe use an enum?)

			var methodCall = Meteor.Method<ChannelResponse>.Call ("updateGameObject", itemToSync.mongoDocument._id, itemToSync.mongoDocument.toDictionary(), collectionKey);
			yield return (Coroutine)methodCall;

			if (methodCall.Response.success) {
				//Debug.LogError (associates[i].dbEntry._id + " should = " + methodCall.Response.message);
				itemToSync.dbUpdated();		//tells the record that it was updated and it can rest now
			} else {
				//Debug.LogError ("Uh oh! database sync failed on record: " + itemToSync.name + ", with _id: " + itemToSync.mongoDocument._id);
			}

			yield return null;

		} else {
			//Debug.LogError(itemToSync.name + "did not need to be updated, but Sync() was called on it");
		}
		Debug.Log ("Finished with Sync()");
		yield return null;
	}
	
	//removes record from GameObjects
	public IEnumerator RemoveRecord (PrizmRecord<TMongoDocument> record) {
		Debug.Log ("RemoveRecord");
		associates.Remove (record);

		if (!record.WasRemovedByServer) {
			Debug.Log ("Removing from database: _id: " + record.mongoDocument._id);		

			var methodCall = Meteor.Method<ChannelResponse>.Call ("removeGameObject", record.mongoDocument._id, this.collectionKey);		
			yield return (Coroutine)methodCall;
			if (methodCall.Response.success) {
				//Destroy(record);			//optional to remove it from the scene too
				Debug.Log ("Successfully removed");
			} else {
				//Debug.LogError ("Uh oh! call to 'removeGameObject' failed on record: " + record.name + ", with _id: " + record.mongoDocument._id);
			}
		}

		yield return null;
	}


	
	//collection of gameObjects
	public IEnumerator CreateMeteorCollection() {
		var methodCall = Meteor.Method<ChannelResponse>.Call ("createGameObjectCollection", this.sessionID, collectionKey);
		yield return (Coroutine)methodCall;


		var initMethodCall = Meteor.Method<ChannelResponse>.Call ("initGameObjectCollections", collectionKey);
		yield return (Coroutine)initMethodCall;


		var subscription = Meteor.Subscription.Subscribe ("gameObjectCollection", collectionKey);
		yield return (Coroutine)subscription;	//wait until subscription successful


		mongoCollection = Meteor.Collection <TMongoDocument>.Create (this.collectionKey);

		yield return mongoCollection;

		mongoCollection.DidAddRecord += (string arg1, TMongoDocument arg2) => { 
			//we only need to bind the record if the object was created by Tabletop and hasn't been bound.
			try {
				bindRecord (arg2); 
			}
			catch (KeyNotFoundException e) {
				Debug.Log("didn't find the record guid, assuming dont have to bind (object wasn't in the unbound things)");
			}

		};

		mongoCollection.DidRemoveRecord += (string obj) => {
			PrizmRecord<TMongoDocument> record = this.LookUpPrizmRecordBy_ID(obj);
			if (record != null)
				record.WasRemovedByServer = true;
			else 
				Debug.Log("record wasn't associated with us");
//			if (record != null) CoroutineHost.Instance.StartCoroutine (RemoveRecord(record));
		};

	}

}

//public class MasterGameObjectCollection {
//	
//	public Meteor.Collection<MongoSchema> mongoCollection;
//	public Dictionary<string,object> recordGroups;
//
//	public MasterGameObjectCollection() {
//		recordGroups = new Dictionary<string,object> ();
//	}
//
//	//collection of gameObjects
//	public IEnumerator CreateMeteorCollection() {
//		mongoCollection = Meteor.Collection <MongoSchema>.Create ("gameObjects");
//		yield return mongoCollection;
//	}
//
//	public void AddRecordGroupHandlers<TMongoDocument>() where TMongoDocument : MongoSchema, new() {
//	
//		/* Handler for debugging the addition of gameObjects: */
//		
//		mongoCollection.DidAddRecord += (arg1, arg2) => {	
//			TMongoDocument doc = (TMongoDocument)arg2;//Coerce<TMongoDocument>();
//
//			Debug.Log ("masterCollection DidAddRecord:"+ doc._id+ " key:"+doc.key); 
//			//Debug.LogError(string.Format("gameObject document added:\n{0}", document.Serialize()));
//			if (recordGroups.ContainsKey(doc.key)) {
//				var recordGroup = (PrizmRecordGroup<TMongoDocument>)recordGroups[doc.key];
//				Debug.Log ("recordGroup collectionKey:" +recordGroup.collectionKey);
//				recordGroup.didAddRecord(arg1,doc);
//			}
//		};
//		
//		mongoCollection.DidChangeRecord += (arg1, arg2, arg3, arg4) => {	
//			TMongoDocument doc = (TMongoDocument)arg2;//Coerce<TMongoDocument>();
//
//			Debug.Log ("masterCollection DidChangeRecord:"+ doc._id+ " key:"+doc.key); 
//			//Debug.LogError(string.Format("gameObject document added:\n{0}", document.Serialize()));
//			if (recordGroups.ContainsKey(arg2.key)) {
//				
//				var recordGroup = (PrizmRecordGroup<TMongoDocument>)recordGroups[doc.key];
//				Debug.Log ("recordGroup collectionKey:" +recordGroup.collectionKey);
//				recordGroup.didChangeRecord(arg1,doc,arg3,arg4);
//			}
//		};
//		
//		//		mongoCollection.DidAddRecord += (string arg1, MongoSchema arg2) => ;
//		//		mongoCollection.DidChangeRecord += (string arg1, MongoSchema arg2, IDictionary arg3, string[] arg4) => ;
//		//		mongoCollection.DidRemoveRecord += (string obj) => ;
//	}
//
//
//	
//	public PrizmRecordGroup<TMongoDocument> registerRecordGroup<TMongoDocument>(string sessionID, string collectionKey)  where TMongoDocument : MongoSchema, new() {
//		Debug.Log ("registerRecordGroup");
//		PrizmRecordGroup<TMongoDocument> recordGroup = new PrizmRecordGroup<TMongoDocument> (sessionID, collectionKey);
//		Debug.Log ("registerRecordGroup collectionKey:" +recordGroup.collectionKey);
//		recordGroups.Add (recordGroup.collectionKey, recordGroup);
//
//		return recordGroup;
//	}
//}
