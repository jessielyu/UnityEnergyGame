using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NextLevelController : MonoBehaviour {
	//set this level in the inspector
	public int level;

	private GameObject springClone;

	void OnCollisionEnter(Collision col) {
		springClone = GameObject.Find ("Spring(Clone)");
		if (springClone != null) {
			Destroy(springClone);
		}

		if (col.gameObject.name == "PlayerRobot") {
			Invoke ("LoadLevel", 6.0f);
		}
	}

	void LoadLevel() {
		Application.LoadLevel (level);
	}
}
