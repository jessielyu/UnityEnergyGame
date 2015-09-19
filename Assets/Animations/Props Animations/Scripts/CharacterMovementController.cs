using UnityEngine;
using System.Collections;

public class CharacterMovementController : MonoBehaviour {

	// Use this for initialization
	public float speed;
	public float walkspeed;
	public float runSpeed;
	public float jumpSpeed;
	public float gravity;
	public bool pistol;
	
	Vector3 moveDirection = Vector3.zero;
	
	public Vector3 movementSpeed = Vector3.zero;
	
	CharacterController controller;
	
	Animator animator;
	
	
	void Start(){
		animator = GetComponent<Animator>();
		controller = GetComponent<CharacterController>();
	}
	
	void Update() {
		
		transform.Rotate(0,Input.GetAxis("Mouse X"),0);
		
		//Do all the pistol stuff.
		Pistol();
		
		
		if (controller.isGrounded) {
			// We are grounded, so recalculate
			// move direction directly from axes
			moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			movementSpeed = moveDirection;
			
			if(Input.GetButton("Sprint")){
				speed = Mathf.Lerp(speed, runSpeed, Time.deltaTime * 2);
			}else{
				speed = Mathf.Lerp(speed, walkspeed, Time.deltaTime * 2);
			}
			movementSpeed *= speed;
			animator.SetFloat("MovementX", movementSpeed.x);
			animator.SetFloat("MovementZ", movementSpeed.z);
			
			moveDirection = transform.TransformDirection(moveDirection);
			moveDirection *= speed;
			
			//Do all the jump stuff.
			
			
		}
		
		Jump();
		// Apply gravity
		
		moveDirection.y -= gravity * Time.deltaTime;
		
		
		// Move the controller
		controller.Move(moveDirection * Time.deltaTime);
	}
	
	void Jump(){
		//if the character is on the ground and walking forward only, allow him to jump
		if (controller.isGrounded) {
		
			if(Input.GetAxis("Horizontal") == 0 && animator.GetFloat("MovementZ") > 1.3f){
				if (Input.GetButton ("Jump")) {
					animator.SetBool("Jump", true);
				}
				
				//only jump off the right foot.
				if(animator.GetFloat("JumpCurve") == 1){
					moveDirection.y = jumpSpeed;
				}
			}else{
				animator.SetBool("Jump", false);
			}
		}
		// if the character has jumped set jump bool to false
		if(animator.GetFloat("Curve") > .3f){
			animator.SetBool("Jump", false);
		}
	}
	
	
	void Pistol(){
		//a bool to turn pistol on and off.
		if(Input.GetButtonDown("Pistol")){
			pistol = true;
		}else if (Input.GetButtonDown("Unarmed")){
			pistol = false;
		}
		
		//set the pistol layer weight.
		if(pistol){
			if(animator.GetLayerWeight(1) < 0.999999f){
				animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1),1,Time.deltaTime * 3));
			}
		}else{
			if(animator.GetLayerWeight(1) > 0.000001f){
				animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1),0,Time.deltaTime * 3));
			}
		}
	}
}
