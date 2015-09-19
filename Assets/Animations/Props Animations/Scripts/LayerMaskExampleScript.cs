using UnityEngine;
using System.Collections;

public class LayerMaskExampleScript : MonoBehaviour {

	// Use this for initialization
	
	Animator animator;
	public float weight = 0;
	public float startTime;
	public float timer;
	public bool timerBool = true;
	
	void Start () {
		animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		
		if(weight == 1 && timerBool == true){
			startTime = Time.time;
			timerBool = false;
		}else if (weight == 0 && timerBool == false){
			startTime = Time.time;
			timerBool = true;
		}
		
		timer = (Time.time - startTime) * 0.7f;
		
		if(timerBool){
			weight = Mathf.Lerp(0,1,timer);
		} else{
			weight = Mathf.Lerp(1,0,timer);
		}
		animator.SetLayerWeight(1, weight);
	}
	
	
}
