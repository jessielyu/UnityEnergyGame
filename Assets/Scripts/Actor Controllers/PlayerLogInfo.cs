using UnityEngine;
using System.Collections;

public class PlayerLogInfo : MonoBehaviour {
	public string P1;
	public string P2;

	private float P1Energy;
	private float P2Energy;

	private bool isEating;
	private bool isPunching;
	private bool isCrouching;
	private bool isClimbing;
	private bool isTPosing;
	private bool isRowing;
	private bool isWalking;

	private float eatingTime = 0.0f;
	private float punchingTime = 0.0f;
	private float crouchingTime = 0.0f;
	private float climbingTime = 0.0f;
	private float tPosingTime = 0.0f;
	private float rowingTime = 0.0f;

	private int eatTimes = 0;
	private int punchTimes = 0;
	private int crouchTimes = 0;
	// TODO: climb times, tPose times, row times

	private PlayerController pcAccessor;
	private Animator anim;
	private PlayerEnergy playerEnergyReference;

	// Use this for initialization
	void Start () {
		pcAccessor = GameObject.Find ("PlayerRobot").GetComponent<PlayerController>();
		anim = GameObject.Find ("PlayerRobot").GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {

		playerEnergyReference = GameObject.Find("PlayerRobot").GetComponent<PlayerEnergy>();
		P1Energy = playerEnergyReference.p1Energy;
		P2Energy = playerEnergyReference.p2Energy;

		switch (Application.loadedLevel) {
		case 0:
			isEating = anim.GetBool ("LeftEat") || anim.GetBool ("RightEat");
			isPunching = anim.GetBool("LeftPunch") || anim.GetBool("RightPunch");
			isCrouching = anim.GetBool(""); //TODO: Add crouching bool
			isClimbing = anim.GetBool("ClimbEnter") || anim.GetBool("ClimbIdle") || 
						 anim.GetBool("LeftClimb") || anim.GetBool("LeftClimb") || anim.GetBool("Climbcomplete");
			isWalking = anim.GetBool("Move");
		case 1:
			isTPosing = anim.GetBool(""); //TODO: Get TPose boolean
			isRowing = anim.GetBool ("IsRowing");
			isWalking = anim.GetBool("IsWalking");
		}

		if (isEating) {
			eatingTime += Time.deltaTime;
			print("Eating Time: " + eatingTime);
			//TODO: How to get information of different players?
			print("Name: " + P1 + "  " + "Energy: " + P1Energy);
			print("Name: " + P2 + "  " + "Energy: " + P2Energy);
			//TODO: Accumulate all the eating time with eat return and count the total number of eat times
		}

		if (isPunching) {
			punchingTime += Time.deltaTime;
			print("Punching Time: " + punchingTime);
			print("Name: " + P1 + "  " + "Energy: " + P1Energy);
			print("Name: " + P2 + "  " + "Energy: " + P2Energy);
			//TODO
		}

		if (isCrouching) {
			crouchingTime += Time.deltaTime;
			print("Crouching Time: " + crouchingTime);
			print("Name: " + P1 + "  " + "Energy: " + P1Energy);
			print("Name: " + P2 + "  " + "Energy: " + P2Energy);
			//TODO
		}

		if (isClimbing) {
			climbingTime += Time.deltaTime;
			print("Climbing Time: " + climbingTime);
			print("Name: " + P1 + "  " + "Energy: " + P1Energy);
			print("Name: " + P2 + "  " + "Energy: " + P2Energy);
			//TODO
		}

		if (isTPosing) {
			tPosingTime += Time.deltaTime;
			print("TPosing Time: " + tPosingTime);
			print("Name: " + P1 + "  " + "Energy: " + P1Energy);
			print("Name: " + P2 + "  " + "Energy: " + P2Energy);
			//TODO
		}

		if (isRowing) {
			rowingTime += Time.deltaTime;
			print("Rowing Time: " + rowingTime);
			print("Name: " + P1 + "  " + "Energy: " + P1Energy);
			print("Name: " + P2 + "  " + "Energy: " + P2Energy);
			//TODO
		}
	}
}
