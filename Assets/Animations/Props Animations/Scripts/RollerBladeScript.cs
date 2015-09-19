using UnityEngine;
using System.Collections;

public class RollerBladeScript : MonoBehaviour {

	// Use this for initialization

	Animator animator;
	CharacterController controller;

	public float speed = 6.0f;
	public float jumpSpeed = 8.0f;
	public float gravity = 20.0f;
	public float slowdownSpeed = 0.05f;
	public float brakeSpeed = 0.005f;

	public float forwardVelocity = 0f;
	Vector3 moveDirection = Vector3.zero;

	void Start () {
		animator = GetComponent<Animator>();
		controller =  GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () {

		if (controller.isGrounded) {
			// We are grounded, so recalculate
			// move direction directly from axes

			animator.SetFloat("Turn", Input.GetAxis("Horizontal"));
			animator.SetFloat("SkateForward", Input.GetAxis("Vertical"));
			animator.SetFloat("ForwardVelocity", forwardVelocity);
			
			if(Input.GetAxis("Vertical") < 0 && Input.GetButton("Vertical")){// 
				animator.SetBool("Brake", true);
			}else{
				animator.SetBool("Brake", false);
			}

			if(Input.GetButtonDown("Vertical") || forwardVelocity < speed){
				if(Input.GetAxis("Vertical") > 0){
					forwardVelocity += Input.GetAxis("Vertical");
				}else if(Input.GetAxis("Vertical") < 0){
					forwardVelocity += Input.GetAxis("Vertical") * brakeSpeed;
				}

				if(forwardVelocity> speed){
					forwardVelocity = speed;
				}
				if(forwardVelocity < 0){
					forwardVelocity = 0;
				}
			}

			if(!Input.GetButtonDown("Vertical")){
				forwardVelocity -= slowdownSpeed;
				if(forwardVelocity < 0){
					forwardVelocity = 0;
				}

			}

			moveDirection = new Vector3(0, 0, forwardVelocity);
			moveDirection = transform.TransformDirection(moveDirection);
			controller.transform.Rotate(0,Input.GetAxis("Horizontal"), 0);
			
			if (Input.GetButton ("Jump")) {
				moveDirection.y = jumpSpeed;
			}
		}
		// Apply gravity
		moveDirection.y -= gravity * Time.deltaTime;
		
		// Move the controller
		controller.Move(moveDirection * Time.deltaTime);
	}


}
