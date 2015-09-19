using UnityEngine;
using System.Collections;

public class EndClimbTrigger : MonoBehaviour {
	private GameObject playerRobot;
	private PlayerController pcAccessor;

	void Start () {
		playerRobot = GameObject.Find ("PlayerRobot");
		pcAccessor = playerRobot.GetComponent<PlayerController> ();
	}

	void OnCollisionEnter(Collision col) 
	{
		if (col.gameObject.name == "PlayerRobot") {
			pcAccessor.triggerSectionEnding = true;
		}
	}

	void OnCollisionExit(Collision col) {
		if (col.gameObject.name == "PlayerRobot") {
			pcAccessor.triggerSectionEnding = false;
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
