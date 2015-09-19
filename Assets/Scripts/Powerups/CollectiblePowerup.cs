using UnityEngine;
using System.Collections;

public class CollectiblePowerup : MonoBehaviour {
	public GameObject springFab;
	private Vector3 spawnLoc = new Vector3 (-150, 0, 0);

	void OnCollisionEnter(Collision col) 
	{
		if (col.gameObject.name == "PlayerRobot") {
			Destroy (gameObject);										//on collision, deletes the gameObject to which this script is attached
			Instantiate(springFab, spawnLoc, transform.rotation);		//instantiates a spring in the exploding box demonstrating that the powerup is collected
		}
	}

	void Update () 
	{
		transform.Rotate (new Vector3 (15, 30, 45) * Time.deltaTime);
	}

}
