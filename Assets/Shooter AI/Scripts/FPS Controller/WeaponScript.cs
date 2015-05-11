using UnityEngine;
using System.Collections;

/// <summary>
/// created by Markus Davey 22/11/2011
/// Basic weapon script
/// Skype: Markus.Davey
/// Unity forums: MarkusDavey
/// </summary>


public class WeaponScript : MonoBehaviour 
{
	
	public float damage = 30f; //the amount of damage to induce
	public float forceAplly = 5f; //the amount of hit force to apply
	
	
	void Update()
	{
		
		//shoot
		if(  Input.GetButtonDown("Fire1") )
		{
			
			//debug
			Debug.DrawRay( transform.position, transform.forward * 1000f, Color.green);
				
			RaycastHit hitInfo;
			if( Physics.Raycast( transform.position, transform.forward * 1000f, out hitInfo) )
			{
				
				//Debug.Log( hitInfo.collider.gameObject, hitInfo.collider.gameObject);
				
				
				hitInfo.collider.gameObject.SendMessageUpwards("Damage", damage, SendMessageOptions.DontRequireReceiver);
				
				if(hitInfo.collider.GetComponent<Rigidbody>() != null)
				{
					hitInfo.collider.GetComponent<Rigidbody>().AddForceAtPosition( -hitInfo.normal * forceAplly * 100f, hitInfo.point);
				}
				
			}
			
		}
		
	}
	
	
}