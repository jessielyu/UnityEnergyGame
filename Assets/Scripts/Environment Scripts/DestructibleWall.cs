using UnityEngine;
using System.Collections;

public class DestructibleWall : MonoBehaviour {
	// prefab(s) used to replace undamaged wall
	public GameObject destroyedWallPrefab;

	private PlayerController pcAccessor;
	public ParticleSystem wallParticleSystem;


	public double wallHealth = 100.0f;

	void Start() {
		pcAccessor = GameObject.Find ("PlayerRobot").GetComponent<PlayerController>();
		//wallHealth = 0f;
	}
	void FixedUpdate() {
		//print (wallHealth);
			//adds health to simulate wall durability, requires a big puch to damage the wall and replace with a different model
		//wallHealth += .2f;
		if (wallHealth > 100.0f) {
			wallHealth = 100.0f;
		}

		if (wallHealth <= 50f) {
			if (destroyedWallPrefab) {
				Instantiate (destroyedWallPrefab, transform.position, transform.rotation);
			}
			Destroy (this.gameObject);
		}
	}

	//determines the amount of damage to the wallHealth on collision based on velocity
	void OnCollisionEnter(Collision col) {
		//TODO:collision is happening multiple times per punch, need to limit to only one damage per animation play
		if (col.gameObject.name == "Left_Wrist_Joint_01" || col.gameObject.name == ("Right_Wrist_Joint_01")) {
			if (wallParticleSystem != null) {
				if(!wallParticleSystem.isPlaying) {
					wallParticleSystem.Play();
				}
			}
			wallHealth -= pcAccessor.wallDamage;
		}
	}
	void OnMouseDown() { //TODO: DELETE THIS FOR FINAL VERSION
		Destroy (this.gameObject);
		GameObject.Find ("PlayerRobot").GetComponent<PlayerEnergy> ().p1Energy = 100.0f;
	}
}
