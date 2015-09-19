using UnityEngine;
using System.Collections;

public class IKScript : MonoBehaviour {

	public Animator animator;
	public Transform LHandPos1;
	public float LHandWeight;
	
	void Start(){
		animator = GetComponent<Animator>();
	}

	// Use this for initialization
	void OnAnimatorIK(int layerIndex)	{
    	
    	LHandWeight = animator.GetFloat("LHandWeight");
    	animator.SetIKPosition(AvatarIKGoal.LeftHand, LHandPos1.position);
    	animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, LHandWeight);
    	
    }
}
