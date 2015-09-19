using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class RedBar : MonoBehaviour {
	private float redstrength;
	private float bluestrength;
	private PlayerEnergy playerEnergyReference;
	
	// Update is called once per frame
	void LateUpdate () {
		playerEnergyReference = GameObject.Find("PlayerRobot").GetComponent<PlayerEnergy>();
		bluestrength = playerEnergyReference.p1Energy / 100;
		redstrength = playerEnergyReference.p2Energy / 100;

		Image redbar = GetComponent<Image> ();
		float offset = redstrength * 0.4f;
		float left = 0.4f - offset;
		redbar.rectTransform.anchorMin = new Vector2 (left, 0.5f);

		float ratio = redstrength / (bluestrength + redstrength) * 0.2f;
		float right = 0.4f + ratio;
		redbar.rectTransform.anchorMax = new Vector2 (right, 0.5f);
		if (left >= 0.4f)
			redbar.rectTransform.anchorMin = new Vector2 (0.4f, 0.5f);
	}
}
