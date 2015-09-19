using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using SWS;

public class ThirdPersonCamera : MonoBehaviour
{
	//=============================
	//Camera Variables
	//=============================
	public float smooth;							// a public variable to adjust smoothing of camera motion
	private float standardSmoothValue = 0.0f;
	private float scaleOverTime = 0.0f;
	private Transform standardPos;					// the usual position for the camera, specified by a transform in the game


	//=============================
	//Accessors
	//=============================
	private GameObject playerRobot;
	private SWS.PlayerSplineMove playerMover;
	private PlayerController pcAccessor;

	//=============================
	//Private management bools
	//=============================
	private bool eventCalled = false;
	private bool brokenDoorZoom = false;


	public void callDoorZoom() {
		brokenDoorZoom = true;
	}

	private void doorZoom() {
		if (playerMover.paused == true && brokenDoorZoom == true) {
			transform.LookAt (GameObject.Find ("BrokenDoor").transform.position, Vector3.up);
			transform.position = Vector3.Lerp (transform.position, GameObject.Find ("BrokenDoor").transform.position, Time.deltaTime * smooth);
		}
		if (playerMover.paused == false && brokenDoorZoom == true) {
			brokenDoorZoom = false;
			playerMover.events[1].RemoveAllListeners();
		}
	}

	public void manageSmoothSpeed() {
		smooth = standardSmoothValue;				//runs constantly if no other conditions are used
		

		if (pcAccessor.awakeTriggered == false) {
			if (scaleOverTime < 0.775) {
				scaleOverTime += Time.deltaTime / 13.5f;
				eventCalled = true;
			}
			smooth = scaleOverTime;
		} else {
			eventCalled = false;
		}

		if (brokenDoorZoom == true) {
			smooth = 0.4f;
			eventCalled = true;
		} else {
			eventCalled = false;
		}
	}


	void Start()
	{

		// initialising references
		if (GameObject.Find ("MainCamera")) {
			Destroy(GameObject.Find ("DebugCamera"));
		}
		standardPos = GameObject.Find ("CamPos").transform;
		playerRobot = GameObject.Find ("PlayerRobot");
		playerMover = playerRobot.GetComponent<PlayerSplineMove> ();
		pcAccessor = playerRobot.GetComponent<PlayerController> ();

		standardSmoothValue = 2.25f;
	}

	void OnLevelWasLoaded(int level) {
	
	}

	void LateUpdate ()
	{

		manageSmoothSpeed ();
		doorZoom ();
		if (standardPos != GameObject.Find ("CamPos").transform) {
			standardPos = GameObject.Find ("CamPos").transform; //TODO:(BEN)if looking at something else do not try to move back to CamPos
		}

		// return the camera to standard position and direction
		transform.position = Vector3.Lerp(transform.position, standardPos.position, Time.deltaTime * smooth);	
		if (eventCalled == false) {
			transform.forward = Vector3.Lerp (transform.forward, standardPos.forward, Time.deltaTime * smooth);
		}

	}
}
