using UnityEngine;
using System.Collections;

public class FloatingBoat : MonoBehaviour {

	private Animator anim;
	private bool isRowing;
	private Vector3 pos;
	
	private PlayerController pcAccessor;

	// Use this for initialization
	void Start () {
		pos = transform.position;
		pcAccessor = GameObject.Find ("PlayerRobot").GetComponent<PlayerController>();
		anim = GameObject.Find ("PlayerRobot").GetComponent<Animator>();
		//pos.z = transform.position.z
	}

	// Update is called once per frame
	void Update () {
		//boat move on the water
		Vector3 p = transform.position;
		isRowing = anim.GetBool("IsRowing");

		//only called while rowing
		if (isRowing) {
			//adjust the temporary transform position variable while rowing
			pos.z += (pcAccessor.gestureVars.p1ImmediateVel) * 5.0f;
		}
		//update the boat transform
		transform.position = Vector3.Lerp(p, pos, Time.deltaTime * 3); //smooth damp?

		//boat floating on the water
		pos.y = 1.2f + Mathf.Sin (Time.time * 3.5f) * 0.1f;

	}
}
