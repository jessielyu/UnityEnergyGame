using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using SWS;

public class ThirdPersonCameraMod : MonoBehaviour
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

	public struct SmoothOptions {
		//use if smoothing during an intro sequence
		public bool isIntro;
		public bool smoothCondition;
		public bool compVal;
		public float tempSmooth;
		public float scaleVal;
		public float timeDownScaler;
	}
	SmoothOptions smoothOpts = new SmoothOptions();

	//------------------------------------------------------------------------------------------------------------------------------------------------------------------
	// Functions                   
	//------------------------------------------------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Call this function in start to assign intro-sequence smoothing variables with speed scaling
	/// </summary>
	public void setIntroSmoothingVariables(bool iI, float sV, float tDS) {
		smoothOpts.isIntro = iI;
		smoothOpts.scaleVal = sV;
		smoothOpts.timeDownScaler = tDS;
	}

	/// <summary>
	/// Call this function to manage smoothing based on two conditions without speed scaling
	/// </summary>
	public void setSmoothingVariables(bool sC, bool cV, float tS) {
		smoothOpts.smoothCondition = sC;
		smoothOpts.compVal = cV;
		smoothOpts.tempSmooth = tS;
	}

	public void resetSmooth() {
		smoothOpts.isIntro = false;
		smoothOpts.smoothCondition = true;
		smoothOpts.compVal = false;
		smoothOpts.scaleVal = 0.0f;
		smoothOpts.tempSmooth = 0.0f;
		smoothOpts.timeDownScaler = 0.0f;
	}

	public void manageSmoothSpeed(bool intro, bool condition, bool comparisonValue, float scaleValue, float downScale, float tempSmoothSpeed) {
		smooth = standardSmoothValue;

		if (condition == comparisonValue && !intro) {
			smooth = tempSmoothSpeed;
		}

		if (condition == comparisonValue && intro) {
			if (scaleOverTime < scaleValue) {
				scaleOverTime += Time.deltaTime / downScale;
			}
			smooth = scaleOverTime;
		}
	}

	private void updateOptionsDuringIntro() {
		switch (Application.loadedLevel) {
		case 0:
			if (pcAccessor.awakeTriggered == true) {
				smoothOpts.isIntro = false;
			} else {
				setIntroSmoothingVariables(true, 0.775f, 13.5f);
			}
				break;
		case 1:
			break;
		}
	}

	//=============================
	//Event specific functions
	//=============================
	public void callDoorZoom() {
		brokenDoorZoom = true;
	}
	
	private void doorZoom() {
		if (playerMover.paused == true && brokenDoorZoom == true) {
			transform.LookAt (GameObject.Find ("BrokenDoor").transform.position, Vector3.up);
			transform.position = Vector3.Lerp (transform.position, GameObject.Find ("BrokenDoor").transform.position, Time.deltaTime * smooth);
			setSmoothingVariables (brokenDoorZoom, true, 0.4f);
		}
		if (playerMover.paused == false && brokenDoorZoom == true) {
			brokenDoorZoom = false;
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

		switch (Application.loadedLevel) {

		case 0:
			break;
		case 1:
			setIntroSmoothingVariables(true, 0.95f, 13.5f);
			break;
		case 2:
			break;
		}
	}

	//TODO: Fix camera
	//setSmoothingVariables sets all variables only once
	//Need way to check dynamically changing variables and pass those to smoothOpts
	//Want to disable Intro variable after the intro zoom is completed

	void LateUpdate ()
	{
		updateOptionsDuringIntro ();
		manageSmoothSpeed (smoothOpts.isIntro, smoothOpts.smoothCondition, smoothOpts.compVal, smoothOpts.scaleVal, smoothOpts.timeDownScaler, smoothOpts.tempSmooth);
		doorZoom ();

		transform.position = Vector3.Lerp (transform.position, standardPos.position, Time.deltaTime * smooth);	
		transform.forward = Vector3.Lerp (transform.forward, standardPos.forward, Time.deltaTime * smooth);

	}
}
