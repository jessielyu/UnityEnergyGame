using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

	private float Mytime  = 10.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Mytime -= Time.deltaTime;

		if (Mytime <= 0.0f) {
			//TODO ADD functions 
		}
	}

	void OnGUI () {
		GUIStyle myStyle = new GUIStyle();
		myStyle.fontSize = 40;
		Font myFont = (Font)Resources.Load("Assets/Font Resources/Montague", typeof(Font));
		myStyle.font = myFont;
		myStyle.normal.textColor = Color.red;

		if (Mytime > 0.0f) { //TODO Add some conditions to prompt up timer text 
			GUI.Box (new Rect (700, 150, 200, 50), Mytime.ToString ("0"), myStyle);
		} 
		else {
			//TODO Add Some Messages
		}
	}
}
