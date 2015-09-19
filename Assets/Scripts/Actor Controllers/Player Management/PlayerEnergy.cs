using UnityEngine;
using System.Collections;

public class PlayerEnergy : MonoBehaviour {
	public float p1Energy;
	public float p2Energy;
	public float totalEnergy;


	// Use this for initialization
	void Start () {
		p1Energy = 0.0f;
		p2Energy = 0.0f;
		totalEnergy = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
	
		//limit energy to values between 0 and 100
		if (p1Energy > 100.0f) {p1Energy = 100.0f;}
		if (p2Energy > 100.0f) {p2Energy = 100.0f;}
		if (p1Energy < 0.0f) {p1Energy = 0.0f;}
		if (p2Energy < 0.0f) {p2Energy = 0.0f;}
		if (totalEnergy > 100.0f) {totalEnergy = 100.0f;}
		if (totalEnergy < 0.0f) {totalEnergy = 0.0f;}

		totalEnergy = (p1Energy + p2Energy) / 2.0f; 

	}
}
