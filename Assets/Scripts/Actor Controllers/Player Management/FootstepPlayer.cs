using UnityEngine;
using System.Collections;

public class FootstepPlayer : MonoBehaviour {
	private Vector3 groundDist;
	public AudioClip footstep;
	// Use this for initialization
	void Start () {
		groundDist = new Vector3 (GetComponent<Collider>().bounds.center.x * 1.0f, GetComponent<Collider>().bounds.min.y - 0.1f, GetComponent<Collider>().bounds.center.z * 1.0f); 
	}
	
	bool IsGrounded(){
		return Physics.CheckCapsule (GetComponent<Collider>().bounds.center, groundDist, 0.18f);
	}

	void Update(){
		if (IsGrounded()) {
			if(!GetComponent<AudioSource>().isPlaying) {
			GetComponent<AudioSource>().PlayOneShot(footstep, 1.0f);
			}
		}
	}
}
