using UnityEngine;
using System.Collections;

public class TransformFollowTarget : MonoBehaviour {
	
	public Transform source;
	public Transform target;
	
	
	void Update()
	{
		
		
		source.transform.position = target.transform.position;
		
	}
	
	
}
