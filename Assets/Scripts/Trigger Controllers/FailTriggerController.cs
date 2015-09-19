using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FailTriggerController : MonoBehaviour {

	public Text GameResult;

	// Use this for initialization
	void Start () {
	
	}
	
	void OnTriggerEnter(Collider other) {
		GameResult.text = "Game Over";
		StartCoroutine (WaitAndReset(2.0f));
	}

	IEnumerator WaitAndReset (float waitTime) {
		yield return new WaitForSeconds (waitTime);
		Application.LoadLevel ("first_level");
	}

}
