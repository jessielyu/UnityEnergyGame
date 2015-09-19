using UnityEngine;
using System.Collections;
using SWS;

public class DebugManager : MonoBehaviour {

	public bool skipIntro = false;
	public int debugStartingWaypoint;
	public PathManager skipToPath;

	private GameObject playerRobot;

	public int levelToLoad;

	void Awake() {
		Application.targetFrameRate = 70;
	}

	void Start () {
		LoadLevel (levelToLoad);
		playerRobot = GameObject.Find ("PlayerRobot");


		//Enable in the inspector to disable non-gameplay oriented actions, useful for debugging quickly.
		if (skipIntro == true) {
			if (skipToPath != null) {
				playerRobot.GetComponent<SWS.PlayerSplineMove> ().SetPath (skipToPath);
			}

			playerRobot.GetComponent<SWS.PlayerSplineMove> ().startPoint = debugStartingWaypoint;
			GameObject.Find ("BrokenLightCube").GetComponent<LightCollide>().enabled = false;
			GameObject.Find ("MainCamera").GetComponent<ThirdPersonCamera>().smooth = 2.25f;
			playerRobot.GetComponent<PlayerController>().awakeTriggered = true;
			playerRobot.GetComponent<PlayerController>().moveTriggered = true;
		}
	}

	void LoadLevel(int i) {
		if (i != 0) {
			Application.LoadLevel(i);
		} else {
			return;
		}
	}

}
