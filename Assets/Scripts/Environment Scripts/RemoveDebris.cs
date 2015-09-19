using UnityEngine;
using System.Collections;

public class RemoveDebris : MonoBehaviour {
	private float clearDebrisTimer = 7.0f;   //timer used to determine when to perform a function

	//function to remove the collider components of the children of the game object to which this script is attached
	private void RemoveCollidersRecursively() {
		var allColliders = GetComponentsInChildren<Collider> ();

		foreach (var childCollider in allColliders)Destroy (childCollider);
	}

	//function to destroy all the children of the game object to which this script is attached
	private void DestroyAllChildren() {
		foreach (Transform child in transform) {
			GameObject.Destroy (child.gameObject);
		}
	}

	
	// Update is called once per frame
	void FixedUpdate () {
		clearDebrisTimer -= Time.deltaTime;

		//after x seconds remove collider components from children
		if (clearDebrisTimer <= 2.0f) {
			RemoveCollidersRecursively();
		}

		//after x seconds destroy children
		if (clearDebrisTimer <= 0.0f) {
			DestroyAllChildren ();
		}
	}
}
