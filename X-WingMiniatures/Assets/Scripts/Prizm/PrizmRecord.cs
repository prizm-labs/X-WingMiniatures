using UnityEngine;
using System.Collections;
using Extensions;
using System.Collections.Generic;


//parent class of all game objects that need syncing
public class PrizmRecord<TMongoDocument> where TMongoDocument : MongoSchema, new() {

	public GameObject gameObject;
	public TMongoDocument mongoDocument;
	public bool needsUpdate = false;
	public PrizmRecordGroup<TMongoDocument> recordGroup;
	public bool WasRemovedByServer = false;

	public PrizmRecord(){
		mongoDocument = new TMongoDocument ();
	}

//	virtual public void Awake() {
//		mongoDocument = new TMongoDocument ();
//	}
//
//	virtual public void Start() {
//		//AddToRecordGroup ();		//needs to happen after PrizmRecordGroup finishes Start(); (don't know how to do that yet)
//	}
//	
//	virtual public void OnDestroy() {
//		//StartCoroutine (GameObject.Find ("/GameManager_TT").GetComponent<PrizmRecordGroup> ().RemoveRecord (this));
//		//StartCoroutine( recordGroup.RemoveRecord(this));
//	}
	public IEnumerator Destroy() {
		yield return CoroutineHost.Instance.StartCoroutine (this.recordGroup.RemoveRecord (this));
	}
	
	public void dbUpdated() {
		needsUpdate = false;
	}

	public IEnumerator Sync() {
		Debug.Log ("PrizmRecord Sync()"+this.mongoDocument._id);
		this.needsUpdate = true;
		yield return CoroutineHost.Instance.StartCoroutine (this.recordGroup.Sync (this));
	}

//	public void setName(string n) {
//		//Debug.LogError ("setting UID to : " + n);
////		dbEntry.UID = n;
//		//Debug.LogError ("did it succeed? " + dbEntry.UID);
//	}

	//add self to PrizmRecordGroup
//	public void AddToRecordGroup() 
//	{
//		Debug.Log ("adding to record group");
//		//StartCoroutine(GameObject.Find ("/GameManager_TT").GetComponent<PrizmRecordGroup>().AddRecord (this));	
//		//StartCoroutine( recordGroup.AddRecord(this));
//	}
}