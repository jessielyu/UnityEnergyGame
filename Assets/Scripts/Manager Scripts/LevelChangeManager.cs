using UnityEngine;
using System.Collections;

public class LevelChangeManager : MonoBehaviour {

	private bool callPosChangeOnce;

	// Use this for initialization
	void Start () {
	
	}

	public void positionOnLevelChange(int level, GameObject gO, Vector3 newStartPosition) {
		if (level == Application.loadedLevel) {
			gO.transform.position = newStartPosition;
			return;
		}
	}
	
	public void positionOnLevelChange(int level, GameObject gO, Vector3 newStartPosition, Quaternion newStartRotation) {
		if (level == Application.loadedLevel) {
			gO.transform.position = newStartPosition;
			gO.transform.rotation = newStartRotation;
			return;
		}
	}

	void OnLevelWasLoaded() {


	}

	
	// Update is called once per frame
	void Update () {
	
	}
}
