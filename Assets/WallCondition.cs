using UnityEngine;
using System.Collections;

public class WallCondition : MonoBehaviour {
	private float wallEnergy;
	private PlayerEnergy playerEnergyReference;
	private float energyP1;
	private float energyP2;

	// Use this for initialization
	void Start () {
		playerEnergyReference = GameObject.Find("PlayerRobot").GetComponent<PlayerEnergy>();
		wallEnergy = 0.0f;
		energyP1 = playerEnergyReference.p1Energy;
		energyP2 = playerEnergyReference.p2Energy;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
