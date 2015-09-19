using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Ventuz.OSC;
using DG.Tweening;
using SWS;

[RequireComponent(typeof (Animator))]
[RequireComponent(typeof (CapsuleCollider))]
[RequireComponent(typeof (Rigidbody))]
public class PlayerController : MonoBehaviour 
{
	//Struct to manage Player behavior, animation triggering and interaction data (i.e. the velocity of a punch)
	public struct GestureVars {
		public bool p1LeftBool;
		public bool p1RightBool;
		public bool p2LeftBool;
		public bool p2RightBool;
		public bool cycleBool;
		//TODO:(BEN) add second cycle bool
		
		public float p1LeftVel; //TODO: Vectors instead?
		public float p1RightVel;
		public float p2LeftVel;
		public float p2RightVel;

		public float p1ImmediateVel;
		public float p2ImmediateVel;
		public float p1ImmediateMagnitude;
		public float p2ImmediateMagnitude;
	}
	public GestureVars gestureVars = new GestureVars();

	private Vector3 playerPos;
	/// <summary>
	/// Public bools for triggering movement.
	/// </summary>
	public bool moveTriggered = false;														//true when movement is started
	public bool awakeTriggered = false;														//triggers the initial awake animation
	public bool returnEat = false;
	public bool returnPunch = false;
	public bool triggerSectionEnding = false;
	
	/// <summary>
	/// Bools for determining eating status, sent to the animator / creates energy
	/// </summary>
	public bool cycleLeftFlag = false;
	public bool cycleRightFlag = false;
	
	/// <summary>
	/// Private bools for handling triggers, or function calls
	/// </summary>
	private bool eatingAllowed = false;
	private bool punchingAllowed = false;
	private bool compressingAllowed = false;
	private bool rowingAllowed = false;
	//private bool wallReached = false; TODO: find a use for wallReached, or delete it
	private bool wallDestroyed = false;
	private bool climbActive = false;
	
	/// <summary>
	/// Doubles for handling various speeds based on kinect data.
	/// </summary>
	public double punchSpeed;
	public float wallDamage;
	public float animSpeed = 1.25f;
	public float moveSpeed;
	
	/// <summary>
	/// Private floats handling internal triggers, speeds, or other non-kinect data
	/// </summary>
	public float p1maxVel = 0.0f;
	public float p2maxVel = 0.0f;
	private float climbSpeed;
	private float punchTimer;
	private float oldPosX;
	private float oldPosZ;
	private float stopBuffer = 0.0f;

	/// <summary>
	/// Animator references
	/// </summary>
	private Animator anim;						  //Animator component reference
	private AnimatorStateInfo currentBaseState;   //Base layer animator reference
	private AnimatorStateInfo layer2CurrentState; //Second layer animator reference
	
	//GameObject and script accessors
	private PlayerEnergy peAccessor;
	private GameObject playerRobot;
	private SWS.PlayerSplineMove playerMover;
	private GestureManager gestureManager;
	
	public DestructibleWall obstacleWall;
	public GameObject FoodPowerUp;
	private GameObject clonedFood;
	private Vector3 totalEnergyScale = new Vector3 (0, 0, 0);
	
	/// <summary>
	/// References to the character's animation states, used for various actions in update
	/// </summary>
	//level One states
	static int sleepState = Animator.StringToHash("Base Layer.Sleep");
	static int stunState = Animator.StringToHash("Base Layer.Stunned");
	static int faintState = Animator.StringToHash("Base Layer.Faint");
	static int idleState = Animator.StringToHash("Base Layer.Idle");
	static int moveState = Animator.StringToHash("Base Layer.Move");
	static int leftPunchState = Animator.StringToHash("Base Layer.LeftPunch");
	static int rightPunchState = Animator.StringToHash("Base Layer.RightPunch");
	static int leftEatState = Animator.StringToHash ("Base Layer.LeftEat");
	static int rightEatState = Animator.StringToHash ("Base Layer.RightEat");
	static int climbEnterState = Animator.StringToHash("Base Layer.ClimbEnter");
	static int climbIdleState = Animator.StringToHash("Base Layer.ClimbIdle");
	static int leftClimbState = Animator.StringToHash("Base Layer.LeftClimb");
	static int rightClimbState = Animator.StringToHash("Base Layer.RightClimb");
	static int landState = Animator.StringToHash("Base Layer.Land");
	static int climbCompState = Animator.StringToHash ("Base Layer.ClimbComplete");
	static int danceState = Animator.StringToHash ("Base Layer.Dance");

	//level Two states
	static int walkState = Animator.StringToHash("Base Layer.WalkFWD");
	static int idleStandJump = Animator.StringToHash ("Base Layer.IdleStandingJump");
	static int idle180State = Animator.StringToHash ("Base Layer.Idle180");
	static int jumpToSitState = Animator.StringToHash("Base Layer.JumpToSit");
	static int rowState = Animator.StringToHash("Base Layer.Row");
	/* static int rowState = Animator.StringToHash("Base Layer.Row");
	static int gyroState = Animator.StringToHash("Base Layer.Gyro");
	TODO: implement at a later date 2015-04-02
	*/

	//------------------------------------------------------------------------------------------------------------------------------------------------------------------
	// Functions                   
	//------------------------------------------------------------------------------------------------------------------------------------------------------------------
	void Start () 
	{
		playerRobot = GameObject.Find ("PlayerRobot");
		peAccessor = playerRobot.GetComponent<PlayerEnergy>();
		playerMover = playerRobot.GetComponent<SWS.PlayerSplineMove> ();
		gestureManager = GameObject.Find ("GestureManager").GetComponent<GestureManager>();
		oldPosX = playerRobot.transform.position.x;
		oldPosZ = playerRobot.transform.position.z;
		moveSpeed = 0.0f;
		
		//initialising reference variables
		anim = GetComponent<Animator> ();
		if (anim.layerCount == 2) {
			anim.SetLayerWeight (1, 1);
		}

		switch (Application.loadedLevel){
		case 0:
			playerMover.events[1].AddListener(delegate{pauseMovement(4f); GameObject.Find ("MainCamera").GetComponent<ThirdPersonCamera>().callDoorZoom();});
			break;
		case 1:
			playerMover.startPoint = 0;
			playerMover.SetPath(WaypointManager.Paths["Level02_Room01_Path01"]);
			playerMover.speed = 5.5f;
			playerMover.lockRotation = AxisConstraint.X;

			anim.SetBool("IsWalking", true);
			playerMover.StartMove();
			
			playerMover.events[3].AddListener(delegate() {pauseMovement(.8f); callJumping();});
			playerMover.events[4].AddListener(delegate() {pauseMovement(1.5f);}); 
			playerMover.events[5].AddListener(delegate() {callRowing (); stopMovement(.7f);});
			break;
		case 2:
			
			break;
		default:
			return;
		}
	}
	
	/// <summary>
	/// Updates the position to determine whether a moving animation should be stopped. Call in any event where the robot should be idle.
	/// </summary>
	private void updatePos() {
		oldPosX = playerRobot.transform.position.x;
		oldPosZ = playerRobot.transform.position.z;
	}
	
	private void updatePos(float buffer) {
		stopBuffer = buffer;
		oldPosX = playerRobot.transform.position.x;
		oldPosZ = playerRobot.transform.position.z;
	}
	
	/// <summary>
	/// Pauses the movement of the robot in the waypoint system, and stops the walking animation using updatePos.
	/// </summary>
	public void pauseMovement(float secs) {
		updatePos ();
		playerMover.Pause (secs);
	}

	/// <summary>
	/// Stops all coroutines in the PlayerSplineMoveScript. Call if all movement should be stopped idefinitely.
	/// </summary>
	public void stopMovement(float buffer) {
		updatePos (buffer);
		playerMover.Stop ();						//TODO: check if buffer is necessary, remove if unnecessary
	}

	/// <summary>
	/// When passed a 'true' value, the robot will move backwards on the path.
	/// </summary>
	public void turn(bool backwards) {
		
		if (backwards == playerMover.reverse) {
			return;
		}
		
		int currentPoint = playerMover.currentPoint;
		playerMover.reverse = !playerMover.reverse;
		playerMover.StartMove ();
		playerMover.GoToWaypoint (currentPoint);
	}
	
	/// <summary>
	/// Call at the punching waypoint to determine when the player(s) should eat again.
	/// </summary>
	public void callReturnToEating() {
		returnPunch = false;
		if (playerMover.paused == false) {
			pauseMovement (1000.0f);
		}
		returnEat = true;
		//wallReached = true;
	}

	/// <summary>
	/// Call at the eating waypoint to determine when the player(s) should punch again.
	/// </summary>
	public void callReturnToPunching() {
		returnEat = false;
		turn (false);
		if (playerMover.paused == false && returnPunch == false) {
			transform.LookAt(GameObject.Find ("FoodGenerator").transform.position, Vector3.up);
			pauseMovement (10.0f);
		}
		returnPunch = true;
	}

	public void callCompressingAllowed(){
		compressingAllowed = true;
	}
	
	public void callClimbing() {
		climbActive = true;
	}

	private void callJumping() {
		//anim.SetTrigger("Jump");
		anim.SetTrigger ("EnterBoat");
		if (currentBaseState.fullPathHash == idleStandJump) {
			anim.SetBool ("IsWalking", false);
		}
	}

	private void callRowing() {
		rowingAllowed = true;
	}
	
	/// <summary>
	/// Determines when to return the player to the eating section. Reverses the direction of the player on the path.
	/// </summary>
	public void returnToEating() {
		if (playerMover.paused == true) {
			punchingAllowed = true;
		} else {
			punchingAllowed = false;
		}
		if (playerMover.paused == false && obstacleWall != null && peAccessor.totalEnergy == 0.0f) {
			turn (true);
		}
	}
	
	/// <summary>
	/// Determines when to return the player to punching. Can be called after callReturnToEating() to return to the breakable wall after some time.
	/// </summary>
	public void returnToPunching() {
		if (playerMover.paused == true) {
			eatingAllowed = true; //TODO: shut down punching if not at particular location
		} else {
			eatingAllowed = false;
		}
	}
	
	/// <summary>
	/// Determines whether the wall is destroyed. If it is, a new path is set and the game progresses.
	/// </summary>
	private void checkWallDestroyed() {
		//if the wall obstacle has been destroyed, change the path and add events
		if (obstacleWall == null) {
			returnEat = false;
			returnPunch = false;
			punchingAllowed = false;
			eatingAllowed = false;
			gestureVars.p1ImmediateVel = 0.0f;
			gestureVars.p2ImmediateVel = 0.0f;
			gestureVars.p1ImmediateMagnitude = 0.0f;
			gestureVars.p2ImmediateMagnitude = 0.0f;
			Destroy (clonedFood);
			//changes the path
			playerMover.SetPath(WaypointManager.Paths["Room02_Path01"]);
			playerMover.startPoint = 1;
			playerMover.moveToPath = true;
			
			//Handles the events at new waypoints once the path has been changed
			playerMover.clearAllEvents();

			playerMover.events[2].AddListener(delegate{pauseMovement(15.0f); callCompressingAllowed();});
			playerMover.events[3].AddListener(delegate{stopMovement(1.0f); callClimbing();});
			wallDestroyed = true;
		}
	}
	
	/// <summary>
	/// Abstract method for comparing elements of a string array with a string.
	/// </summary>
	private bool checkMessage(string[] strArr, int arrayEle, string s) {
		return (strArr [arrayEle].Equals (s));
	}
	
	
	/// <summary>
	/// Determines whether the player is moving by comparing a previous position with the current position, and adjusts animation speed accordingly.
	/// </summary>
	public bool isMoving() {
		
		if (oldPosX > playerRobot.transform.position.x + 0.1f + stopBuffer || oldPosZ > playerRobot.transform.position.z + 0.1f + stopBuffer) {
			moveSpeed = 0.5f;
			return true;
		} else if (oldPosX < playerRobot.transform.position.x - 0.1f - stopBuffer || oldPosZ < playerRobot.transform.position.z - 0.1f - stopBuffer) {
			moveSpeed = 0.5f;
			return true;
		} else {
			moveSpeed = 0.0f;
			return false;
		}
		
	}
	
	//sets the speed of any animation using the StringToHash ints above, and speed parameters
	private void setAnimSpeed(int state, float speed){
		if (currentBaseState.fullPathHash == state) {
			anim.speed = speed;
		}
	}

	void callVictoryDance() {
		anim.SetBool ("VictoryDance", true);
	}
	
	/// <summary>
	/// Determines whether a punch bool is true, and modifies the animation parameters, wall damage, and energy
	/// </summary>
	void isPunching() {
		if (peAccessor.p1Energy == 0) {
			gestureVars.p1LeftBool = false;
			gestureVars.p1RightBool = false;
		}
		if (peAccessor.p2Energy == 0) {
			gestureVars.p2LeftBool = false;
			gestureVars.p2RightBool = false;
		}
		
		anim.SetBool ("LeftPunch", gestureVars.p1LeftBool || gestureVars.p2LeftBool);
		anim.SetBool ("RightPunch", gestureVars.p1RightBool || gestureVars.p2RightBool);
		anim.SetBool("CyclePunch", gestureVars.cycleBool);

		//player One check punches
		if (gestureVars.p1LeftBool) {
			if (gestureManager.p1CheckOnceLeft == false) {
				peAccessor.p1Energy -= (gestureVars.p1ImmediateMagnitude * 30.0f);															//set the energy of the player, subtracts a constant times the velocity divided by a number (lower = more energy lost)
				gestureManager.p1CheckOnceLeft = true;
			}
			wallDamage = (gestureVars.p1LeftVel) * 7.50f;     																				//set the wall damage equal to the velocity of both punches divided by a number (adjust for strength, lower = more damage)
			gestureVars.p1LeftVel = 0.0f;																									//reset the velocity values (allows only one punch velocity to be taken when the punch booleans are set).
			gestureVars.p1ImmediateMagnitude = 0.0f;
		}
		if (gestureVars.p1RightBool) {
			if (gestureManager.p1CheckOnceRight == false) {
				peAccessor.p1Energy -= (gestureVars.p1ImmediateMagnitude * 30.0f);																//set the energy of the player, subtracts a constant times the velocity divided by a number (lower = more energy lost)
				gestureManager.p1CheckOnceRight = true;
			}
			wallDamage = (gestureVars.p1RightVel) * 7.50f;     																				//set the wall damage equal to the velocity of both punches divided by a number (adjust for strength, lower = more damage)																								//reset the velocity values (allows only one punch velocity to be taken when the punch booleans are set).
			gestureVars.p1RightVel = 0.0f;
			gestureVars.p1ImmediateMagnitude = 0.0f;
		}

		//player Two check punches
		if(gestureVars.p2LeftBool) {
			if (gestureManager.p2CheckOnceLeft == false) {
				peAccessor.p2Energy -= (gestureVars.p2ImmediateMagnitude * 30.0f);																//adjust wall damage | exponential? (pVelLeft + pVelRight + 1)^3
				gestureManager.p2CheckOnceLeft = true;
			}
			wallDamage = (gestureVars.p2LeftVel * 7.50f);
			gestureVars.p2LeftVel = 0.0f; 
			gestureVars.p2ImmediateMagnitude = 0.0f;
		}

		if (gestureVars.p2RightBool) {
			if (gestureManager.p2CheckOnceRight == false) {
				peAccessor.p2Energy -= (gestureVars.p2ImmediateMagnitude * 30.0f);																	
				gestureManager.p2CheckOnceRight = true;
			}
			wallDamage = (gestureVars.p2RightVel * 7.50f);
			gestureVars.p2RightVel = 0.0f;
			gestureVars.p2ImmediateMagnitude = 0.0f;
		}

		//Reduce size of food powerup
		if (gestureVars.p1LeftBool || gestureVars.p1RightBool || gestureVars.p2LeftBool || gestureVars.p2RightBool) {
			if (clonedFood != null) {
				if (clonedFood.transform.localScale.x > 0.02f && clonedFood.transform.localScale.y > 0.02f && clonedFood.transform.localScale.z > 0.02f) {
					clonedFood.transform.localScale = totalEnergyScale;
				} else {
					Destroy (clonedFood);
				}
			}
		}
	}
	
	public void isEating() {
		anim.SetBool ("LeftEat", gestureVars.p1LeftBool || gestureVars.p2LeftBool);
		anim.SetBool ("RightEat", gestureVars.p1RightBool || gestureVars.p2RightBool);
		
		if (gestureVars.p1LeftBool || gestureVars.p1RightBool) {
			peAccessor.p1Energy += 5.0f;
		}
		if (gestureVars.p2LeftBool || gestureVars.p2RightBool) {
			peAccessor.p2Energy += 5.0f;
		}
		if (gestureVars.p1LeftBool || gestureVars.p1RightBool || gestureVars.p2LeftBool || gestureVars.p2RightBool) {
			if (clonedFood != null) {
				if (clonedFood.transform.localScale.x < 10.0f && clonedFood.transform.localScale.y < 10.0f && clonedFood.transform.localScale.z < 10.0f){
					clonedFood.transform.localScale = totalEnergyScale;
				}
			}
		}

		if (currentBaseState.fullPathHash == leftEatState || currentBaseState.fullPathHash == rightEatState) {
			if (clonedFood == null) {
				clonedFood = (GameObject)Instantiate(FoodPowerUp, new Vector3(-150,0,0), transform.rotation);
			}
		}
	}

	//changes player energy based on hip position, compared to knee position
	public void isCompressingSpring() {
		peAccessor.p1Energy += gestureVars.p1ImmediateVel * 85.0f;
		peAccessor.p2Energy += gestureVars.p2ImmediateVel * 85.0f;
	}
	
	public void isClimbing() {
		//keep a float for distance climbed
		//send the total distance
		anim.SetBool ("LeftClimb", gestureVars.p1LeftBool || gestureVars.p2LeftBool);
		anim.SetBool ("RightClimb", gestureVars.p1RightBool || gestureVars.p2RightBool);
		anim.SetBool ("CycleClimb", gestureVars.cycleBool);

		if (climbSpeed > 0) {
			peAccessor.p1Energy -= (0.025f + ((climbSpeed + 1) / 50.0f));															//drain player energy at a slightly faster rate (losing energy fighting gravity and moving)
			peAccessor.p2Energy -= (0.025f + ((climbSpeed + 1) / 50.0f));															//the players will need to get high initial energy to make it to the top, and climb fast enough without wasting energy
			anim.speed = climbSpeed;
		}

		if (gestureVars.p1LeftBool || gestureVars.p2LeftBool) {
			climbSpeed = Mathf.Pow((gestureVars.p1LeftVel + gestureVars.p2LeftVel + 1), 2) / 1.5f;
			if (climbSpeed < 0) {
				climbSpeed = climbSpeed * -1.0f;
			}
		}
		if (gestureVars.p1RightBool || gestureVars.p2RightBool) {
			climbSpeed = Mathf.Pow((gestureVars.p1RightVel + gestureVars.p2RightVel + 1), 2) / 1.5f;
			if (climbSpeed < 0) {
				climbSpeed = climbSpeed * -1.0f;
			}
		}

		if (peAccessor.totalEnergy > 0 && playerMover.paused == false) {
			anim.SetBool ("ClimbReady", true);
			GetComponent<Rigidbody> ().useGravity = false;
		} else {
			anim.SetBool ("ClimbReady", false);
		}
		if (currentBaseState.fullPathHash == climbEnterState) {
			//transform.Translate(0, 0.02f, 0);
		}
		
		if (currentBaseState.fullPathHash == climbIdleState) {
			peAccessor.p1Energy -= 0.025f;																						//drain player energy at a constant rate while idleing on the wall (loss of energy fighting gravity)
			peAccessor.p2Energy -= 0.025f;
			GetComponent<Rigidbody> ().useGravity = false;
		}

		if (currentBaseState.fullPathHash == leftClimbState || currentBaseState.fullPathHash == rightClimbState) {
				GetComponent<Rigidbody> ().useGravity = false;
				transform.Translate(0, climbSpeed / 10.0f, 0);
		}
		if (triggerSectionEnding == true) {
			anim.SetTrigger ("ClimbComplete");
		}
		if (currentBaseState.fullPathHash == climbCompState) {
			//TODO:(BEN) This is not the way to do this, probably need animation curves or bool with smoother during update
			triggerSectionEnding = false;
			climbSpeed = 0.0f;
			anim.SetBool("ClimbReady", false);   																				//TODO: still needs to trigger twice to disable
			climbActive = false;

			playerMover.SetPath(WaypointManager.Paths["Room02_Path02"]);
			playerMover.lockRotation = AxisConstraint.X;
			playerMover.startPoint = 1;
			playerMover.moveToPath = true;
			playerMover.ChangeSpeed (5);
			playerMover.clearAllEvents();
			playerMover.events[4].AddListener (delegate {pauseMovement(7.0f); callVictoryDance();});
		}
		if (peAccessor.totalEnergy == 0 && anim.GetBool ("IsFalling") == false) {
			anim.SetBool ("IsFalling", true);
			anim.SetBool ("ClimbReady", false);
		}
		if (anim.GetBool ("IsFalling") == true) {
			GetComponent<Rigidbody> ().useGravity = true;
			gestureVars.p1ImmediateVel = 0.0f;
			gestureVars.p2ImmediateVel = 0.0f;
			gestureVars.p1ImmediateMagnitude = 0.0f;
			gestureVars.p2ImmediateMagnitude = 0.0f;
			climbSpeed = 0.0f;
			if (Physics.Raycast (transform.position, Vector3.down, playerRobot.GetComponent<CapsuleCollider>().bounds.extents.y + 0.1f)) {
				anim.SetBool ("IsFalling", false);
				pauseMovement (10.0f);
				compressingAllowed = true;
			} 
		}
	}

	private void isRowing() {
		playerPos = playerRobot.transform.position;
		Vector3 p =  playerRobot.transform.position;
		GameObject.Find ("Boat").transform.parent = playerRobot.transform;
		//boat move on the water
		anim.SetBool ("IsRowing", gestureVars.p1LeftBool || gestureVars.p2LeftBool);

		playerPos.y = 1.2f + Mathf.Sin (Time.time * 3.5f) * 0.1f;

		if (currentBaseState.fullPathHash == rowState) {
			playerPos.z += ((gestureVars.p1ImmediateMagnitude + gestureVars.p2ImmediateMagnitude) * 5.0f);
		}
		playerRobot.transform.position = Vector3.Slerp (p, playerPos, .334f);

		if (triggerSectionEnding) {
			GameObject.Find ("Boat").transform.parent = null;
			anim.SetTrigger("RowingFinished");
			rowingAllowed = false;
			playerMover.SetPath(WaypointManager.Paths["Level02_Room02_Path02"]);
			playerMover.startPoint = 1;
			playerMover.clearAllEvents();
			pauseMovement (3.12f);
			playerMover.events[1].AddListener(delegate {pauseMovement(0.65f); anim.SetTrigger("ExitBoat");});
			playerMover.events[5].AddListener (delegate {stopMovement(0.5f); callVictoryDance(); });
		}
	}


	/*void OnLevelWasLoaded(int level) { TODO:(BEN) is this fucntion potentially useful?

	}*/

	//Update is called once per frame, use this for non-physics updates.
	void Update() {
			currentBaseState = anim.GetCurrentAnimatorStateInfo (0);																	//set currentState variable to the current state of the Base Layer of animation

		
		if (anim.layerCount == 2 && anim != null) {
			layer2CurrentState = anim.GetCurrentAnimatorStateInfo (1); 															// set layer2CurrentState variable to the current state of the second layer of animation
		}

		//Set default animation speed
		anim.speed = (float)(animSpeed);																						//set the standard speed of our animator
	
		totalEnergyScale.Set (peAccessor.totalEnergy / 10.0f, peAccessor.totalEnergy / 10.0f, peAccessor.totalEnergy / 10.0f);

		isMoving ();

		//Begin movement if movement is triggered and the player was not already moving
		if (moveTriggered == true && currentBaseState.fullPathHash == idleState) {
			playerMover.StartMove();
			moveTriggered = false;
		}
		
		//If the player is idle, enforce gravity
		if (currentBaseState.fullPathHash == idleState) {
			GetComponent<Rigidbody>().useGravity = true;
		}

		//If the playerMover is paused, enforce stopped movement
		if (playerMover.paused == true) {
			moveSpeed = 0.0f;
		}

		if (Application.loadedLevel == 0) {

			//=====================================
			//Set and get animator speeds
			anim.speed = (float)(animSpeed);																						//set the standard speed of our animator
			anim.SetFloat ("Speed", moveSpeed);
			setAnimSpeed (sleepState, 0.5f);
			setAnimSpeed (stunState, 0.75f);
			setAnimSpeed (faintState, 1.0f);
			setAnimSpeed (leftEatState, 1.3f);
			setAnimSpeed (rightEatState, 1.3f);
			setAnimSpeed (landState, 0.75f);
			setAnimSpeed (climbCompState, 1.25f);
			setAnimSpeed (danceState, 0.8f);
			//=====================================

			//checks whether awake has been triggered, and sets the animation parameter
			if (awakeTriggered == false) {
				anim.SetBool ("IsSleeping", true);
			} else {
				anim.SetBool ("IsSleeping", false);
			}


			//=====================================
			//Manage movment between two waypoints
			if (returnEat == true) {
				if (peAccessor.totalEnergy == 0) {
					playerMover.Resume ();
				}
				returnToEating ();
			}
		
			if (returnPunch == true) {
				returnToPunching ();
			}
			//=====================================

			//=====================================
			//Manage when particular gesture are allowed. Sets bools at locations, calls recMess with the path of the permitted gesture.
			if (punchingAllowed) {
				gestureManager.recMess ("/punch/left/", "/punch/right/");
				isPunching ();
			} else {
				anim.SetBool ("LeftPunch", false);
				anim.SetBool ("RightPunch", false);
				anim.SetBool ("CyclePunch", false);
			}
		
			if (eatingAllowed) {
				gestureManager.recMess ("/eat/left/", "/eat/right/");
				isEating ();
			} else {
				anim.SetBool ("LeftEat", false);
				anim.SetBool ("RightEat", false);
			}
			if (compressingAllowed) {
				gestureManager.recMess ("/crouch/yes/", "");
				isCompressingSpring ();
				if (playerMover.paused == false) {
					compressingAllowed = false;
				}
			}

			if (climbActive) {
				gestureManager.recMess ("/climb/left/", "/climb/right/");
				isClimbing ();
			} else {
				anim.SetBool ("LeftClimb", false);
				anim.SetBool ("RightClimb", false);
				anim.SetBool ("CycleClimb", false);
			}
			//=====================================

			if (wallDestroyed == false) {
				checkWallDestroyed ();
			}
		}

		if (Application.loadedLevel == 1) {

			if (rowingAllowed) {
				gestureManager.recMess("/row/yes/", "");
				isRowing ();
			} else {
				anim.SetBool ("IsRowing", false);
			}
		}
	}
	
	//FixedUpdate is called once per frame, use this for physics updates, runs on Time.fixedDeltaTime.
	void FixedUpdate () 
	{

	}
}