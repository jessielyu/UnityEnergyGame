using UnityEngine;
using System.Collections;

public class LightCollide : MonoBehaviour {

	//GameObject and Script accessors
	private GameObject playerRobot;
	private PlayerController pcAccessor;


	public float startTimer;

	private void timeCount() {
		startTimer -= Time.deltaTime;
		if (startTimer < 0) {
			startTimer = 0;
			GetComponent<Rigidbody>().useGravity = true;
		}
	}

	void Start() {
		playerRobot = GameObject.Find ("PlayerRobot");
		pcAccessor = playerRobot.GetComponent<PlayerController>();
	}


	void OnCollisionEnter(Collision col) {
		if (col.relativeVelocity.magnitude > 18) {
			GetComponent<AudioSource>().Play ();
			pcAccessor.awakeTriggered = true;
			pcAccessor.moveTriggered = true;
		}
	}

	void Update() {
		timeCount();
	}
}
