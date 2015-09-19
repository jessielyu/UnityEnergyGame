using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class BlueBar : MonoBehaviour {
	private float bluestrength;
	private float redstrength;
	private PlayerEnergy playerEnergyReference;
	
	// Update is called once per frame
	void LateUpdate () {
		playerEnergyReference = GameObject.Find("PlayerRobot").GetComponent<PlayerEnergy>();
		bluestrength = playerEnergyReference.p1Energy / 100;
		redstrength = playerEnergyReference.p2Energy / 100;

		Image bluebar = GetComponent<Image> ();
		float offset = bluestrength * 0.4f;
		float right = 0.6f + offset;
		bluebar.rectTransform.anchorMax = new Vector2 (right, 0.5f);

		float ratio = bluestrength / (bluestrength + redstrength) * 0.2f;
		float left = 0.6f - ratio;
		bluebar.rectTransform.anchorMin = new Vector2 (left, 0.5f);

		if (right <= 0.6f)
			bluebar.rectTransform.anchorMax = new Vector2 (0.6f, 0.5f);
	}
}
