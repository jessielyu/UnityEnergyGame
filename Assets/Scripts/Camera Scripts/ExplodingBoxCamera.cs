using UnityEngine;
using System.Collections;

public class ExplodingBoxCamera : MonoBehaviour {
	Rect r = new Rect(.465f, 0.02f, .08f, .15f);

	void Update() {
		GetComponent<Camera>().rect = r;
	}
}
