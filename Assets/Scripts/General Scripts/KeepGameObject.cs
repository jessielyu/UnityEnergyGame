using UnityEngine;
using System.Collections;

public class KeepGameObject : MonoBehaviour {

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (this.gameObject);
	}
}
