using UnityEngine;
using System.Collections;

public class BasicDoor : MonoBehaviour {
	//Welcome to the included basic door script! Herein are all the bells and whistles of the doors' behaviours.
	//The script is set up in such a way that you can call upon the Open/Close function from a custom script,
	//meaning that you can change how you interact with the doors, if you know your way around basic scripting.
	//As a sample, this script includes behaviour for opening and closing, when a character controller is intersecting the trigger of the door.
	//Note that this might not be the most optimal script, but it does the trick!
	//Happy developing! /Marcus S.
	
	public GameObject doorChild; //The door body child of the door prefab.
	public GameObject audioChild; //The prefab's audio source GameObject, from which the sounds are played.
	
	public AudioClip openSound; //The door opening sound effect (3D sound.)
	public AudioClip closeSound; //The door closing sound effect (3D sound.)
	
	private bool inTrigger = false; //Bool to check if CharacterController is in the trigger.
	private bool doorOpen = false; //Bool used to check the state of the door, if it's open or not.
	
	//Door opening and closing function. Can be called upon from other scripts.
	public void doorOpenClose() {
		//Check so that we're not playing an animation already.
		if (doorChild.GetComponent<Animation>().isPlaying == false) {
			//Check the state of the door, to determine whether to close or open.
			if (doorOpen == false) {
				//Opening door, play Open animation and sound effect.
				doorChild.GetComponent<Animation>().Play("Open");
				audioChild.GetComponent<AudioSource>().clip = openSound;
				audioChild.GetComponent<AudioSource>().Play();
				doorOpen = true;
			}
			else {
				//Closing door, play Close animation and sound effect.
				doorChild.GetComponent<Animation>().Play("Close");
				audioChild.GetComponent<AudioSource>().clip = closeSound;
				audioChild.GetComponent<AudioSource>().Play();
				doorOpen = false;
			}
		}
	}
	
	
	
	//The rest is for the interaction with the door. This can be removed or altered if you'd like to control the doors in a different way.
	//Set the inTrigger to true when CharacterController is intersecting, which in turn means routine in Update will check for button press (interaction.)
	void OnTriggerEnter(Collider collider) {
		if (collider.GetComponent<CharacterController>())
			inTrigger = true;
	}
	//Set the inTrigger to false when CharacterController is out of trigger, which in turn means routine in Update will NOT check for button press.
	void OnTriggerExit(Collider collider) {
		if (collider.GetComponent<CharacterController>())
			inTrigger = false;
	}
	
	void Update() {
		//Check the inTrigger bool, to see if CharacterController is in the trigger and thus can interact with the door.
		if (inTrigger == true) {
			//If inTrigger is true, check for button press to interact with door.
			//For this sample behaviour, we're checking for Fire2, which defaults to the right mouse button.
			if (Input.GetButtonDown("Fire2")) {
				doorOpenClose();
			}
		}
	}
}
