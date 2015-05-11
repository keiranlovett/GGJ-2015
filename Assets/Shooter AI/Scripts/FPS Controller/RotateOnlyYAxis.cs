using UnityEngine;
using System.Collections;

public class RotateOnlyYAxis : MonoBehaviour {

	public Transform source; //the soruce roation
	public Transform target; //where to apply the rotation
	
	
	void Update()
	{
		//apply the rotation
		target.eulerAngles = new Vector3( target.eulerAngles.x, source.eulerAngles.y, target.eulerAngles.z);
		

	}
	
	
}
