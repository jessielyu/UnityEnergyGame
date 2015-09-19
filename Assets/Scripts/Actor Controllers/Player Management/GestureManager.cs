using UnityEngine;
using System.Collections;
using Ventuz.OSC;

public class GestureManager : MonoBehaviour {
	public UdpReader readGestures;				//receiving OSC messages
	public OscMessage toRec;


	//message 
	private int messageCount = 0;        			// counter that resets certain variables when a cycle of messages completes
	private string[] currentMess = new string[30];  //string array that holds a window of messages 

	//private bools for handling cycle animation checks
	public bool p1CheckOnceLeft = false;
	public bool p1CheckOnceRight = false;
	public bool p2CheckOnceLeft = false;
	public bool p2CheckOnceRight = false;
	
	//GameObject and script accessors
	private GameObject playerRobot;
	//private PlayerEnergy peAccessor; TODO: Find a use for energy, or delete this
	private PlayerController pcAccessor;

	// Use this for initialization
	void Start () {
		playerRobot = GameObject.Find ("PlayerRobot");
		pcAccessor = playerRobot.GetComponent<PlayerController> ();
		//peAccessor = playerRobot.GetComponent<PlayerEnergy>();

		readGestures = new UdpReader (12346);
	}

	void OnLevelWasLoaded() {
		playerRobot = GameObject.Find ("PlayerRobot");
		pcAccessor = playerRobot.GetComponent<PlayerController> ();
		//peAccessor = playerRobot.GetComponent<PlayerEnergy>();
	}

	/// <summary>
	/// Abstract method for comparing elements of a string array with a string.
	/// </summary>
	private bool checkMessage(string[] strArr, int arrayEle, string s) {
		return (strArr [arrayEle].Equals (s));
	}
	
	public void recMess(string leftPath, string rightPath) {
		pcAccessor.gestureVars.p1LeftBool = false;
		pcAccessor.gestureVars.p1RightBool = false;
		pcAccessor.gestureVars.p2LeftBool = false;
		pcAccessor.gestureVars.p2RightBool = false;
		
		if (toRec == null) {
			pcAccessor.gestureVars.cycleBool = false;
		}
		
		if (toRec != null) {
			string[] message = (toRec.ToString ()).Split ('\n');																	//Received messages delimited by value <- this line receives the path
			string[] mess1 = message [1].Split ('S');																				//Deliminted by OSC message type 	   <- this line receives a value
			if (messageCount > 28) {																								//resets the message loops and the cycling flags to limit animation loops
				messageCount = 0;
				pcAccessor.p1maxVel = 0.0f;
				pcAccessor.p2maxVel = 0.0f;
				pcAccessor.cycleLeftFlag = false;
				pcAccessor.cycleRightFlag = false;
			} else {
				messageCount++;
			}
			
			currentMess [messageCount] = message [0];
			if (currentMess [messageCount].Equals (leftPath + "1") || currentMess [messageCount].Equals (leftPath + "2")) {
				pcAccessor.cycleLeftFlag = true;
			}
			if (currentMess [messageCount].Equals (rightPath + "1") || currentMess [messageCount].Equals (rightPath + "2")) {
				pcAccessor.cycleRightFlag = true;
			}
			if (pcAccessor.cycleRightFlag) {
				if (pcAccessor.cycleLeftFlag) {
					if (currentMess [messageCount].Equals (rightPath + "1") || currentMess [messageCount].Equals (rightPath + "2")) {
						pcAccessor.gestureVars.cycleBool = true;
					}
				}
			}
			
			if (message [0].Equals (leftPath + "1")) { // checkMessage(message, 0, "/punch/left/1");
				pcAccessor.gestureVars.p1LeftBool = true;
				pcAccessor.gestureVars.p1LeftVel = pcAccessor.p1maxVel;
				pcAccessor.gestureVars.p1ImmediateVel = float.Parse (mess1[0]);
				pcAccessor.gestureVars.p1ImmediateMagnitude = Mathf.Abs (float.Parse (mess1[0]));																
				p1CheckOnceRight = false;																							//Set the opposite check once bool to false (i.e. if left arm punch, set right arm check to false). Allows accurate energy subtraction.
				if (float.Parse(mess1[0]) > pcAccessor.p1maxVel) {																				//Get the largest Velocity out of a single gesture
					pcAccessor.p1maxVel = float.Parse (mess1[0]);
				}
				//print ("Player 1 Left Punch:"+mess1 [0]); 
			}
			if (message [0].Equals (rightPath + "1")) {
				pcAccessor.gestureVars.p1RightBool = true;
				pcAccessor.gestureVars.p1RightVel = pcAccessor.p1maxVel;
				pcAccessor.gestureVars.p1ImmediateVel = float.Parse (mess1[0]);
				pcAccessor.gestureVars.p1ImmediateMagnitude = Mathf.Abs (float.Parse (mess1[0]));
				p1CheckOnceLeft = false;
				if (float.Parse(mess1[0]) > pcAccessor.p1maxVel) {																				//Get the largest Velocity out of a single gesture
					pcAccessor.p1maxVel = float.Parse (mess1[0]);
				}
				//print ("Player 1 Right Punch:"+mess1 [0]);
			}
			
			//Player 2 checking whether a punch is received
			if (message [0].Equals (leftPath + "2")) {
				pcAccessor.gestureVars.p2LeftBool = true;
				pcAccessor.gestureVars.p2LeftVel = pcAccessor.p2maxVel;
				pcAccessor.gestureVars.p2ImmediateVel = float.Parse (mess1[0]);
				pcAccessor.gestureVars.p2ImmediateMagnitude = Mathf.Abs (float.Parse (mess1[0]));
				p2CheckOnceRight = false;
				if (float.Parse(mess1[0]) > pcAccessor.p2maxVel) {																				//Get the largest Velocity out of a single gesture
					pcAccessor.p2maxVel = float.Parse (mess1[0]);
				}
				//print ("Player 2 Left Punch:"+mess1 [0]); 
			}
			if (message [0].Equals (rightPath + "2")) {
				pcAccessor.gestureVars.p2RightBool = true;
				pcAccessor.gestureVars.p2RightVel = pcAccessor.p2maxVel;
				pcAccessor.gestureVars.p2ImmediateVel = float.Parse (mess1[0]);
				pcAccessor.gestureVars.p2ImmediateMagnitude = Mathf.Abs (float.Parse (mess1[0]));
				p2CheckOnceLeft = false;
				if (float.Parse(mess1[0]) > pcAccessor.p2maxVel) {																				//Get the largest Velocity out of a single gesture
					pcAccessor.p2maxVel = float.Parse (mess1[0]);
				}
				//print ("Player 2 Right Punch:"+mess1 [0]);
			}
			
			if (message[0].Equals ("/neutral/1")) {																					//If the player is not performing a gesture: do something
				p1CheckOnceLeft = false;
				p1CheckOnceRight = false;
			}
			if (message[0].Equals ("/neutral/2")) {
				p2CheckOnceLeft = false;
				p2CheckOnceRight = false;
			}
			if (message[0].Equals("/neutral/1") && message[0].Equals ("/neutral/2")) {
				pcAccessor.gestureVars.cycleBool = false;
			}
		}
		
	}


	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate() {
		toRec = readGestures.Receive (); //TODO:(BEN) determine best place to read gestures -> update? fixed update? etc.
	}
}
