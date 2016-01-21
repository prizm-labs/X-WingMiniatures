using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class selfDestructMessage : MonoBehaviour {
	public void killMyself(string message, float countDown){
		GetComponent<Text> ().text = message;
		StartCoroutine (destroyMessage (countDown));
	}
	private IEnumerator destroyMessage(float time){

		yield return new WaitForSeconds (time);
		if (time != 0) {
			Destroy (gameObject);
		}
	}
}
