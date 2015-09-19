using UnityEngine;
using System.Collections;

public class RoboAudioPlayer : MonoBehaviour {
	public AudioSource audio1;
	public AudioSource audio2;
	public AudioClip[] footstep = new AudioClip[8];
	public AudioClip nextClip;

	private GameObject playerRobot;
	private PlayerController pcAccessor;

	void Start () {
		playerRobot = GameObject.Find("PlayerRobot");
		pcAccessor = playerRobot.GetComponent < PlayerController >();
	}

	/// <summary>
	/// Plays footstep sounds if the player is currently moving, and not already playing
	/// </summary>
	public void playFootsteps() {
		if (pcAccessor.isMoving()) {
			if(!audio1.isPlaying) {
				nextClip = footstep[Random.Range (0, footstep.Length)]; //TODO: play based on collision with floor or use animation curves
				audio1.clip = nextClip;
				audio1.Play ();
			}
		}
	}

	/// <summary>
	/// Plays an audioclip from the parameter eClip. Intended for animation events.
	/// </summary>
	public void playAudioAtEvents(AudioClip eventClip) {
		audio2.clip = eventClip;
		if (!audio2.isPlaying) {
			audio2.Play();
		}
	}
	

	// Update is called once per frame
	void Update () {
	}
}
