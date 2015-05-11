using UnityEngine;
using System.Collections;


namespace GatewayGames.ShooterAI
{
	
	/// <summary>
	/// Forces the attached object to follow the reference's position.
	/// </summary>
	public class GatewayGamesPositionFollower : MonoBehaviour 
	{
		
		public Transform target = null; //this is the target to which this object should reference
		public bool forceChildrenSamePosition = true; //whether to force the children e.g. weapons to be the same position or not
		public bool followRelative = true; //whether to follow the exact position of the target, or just the bobbing
		public bool damping = true; //whether to apply damping in the calculations
		public float dampingSpeed = 5f;
		public Vector3 offset; //the offset to the target if using absolute values
		
		private Vector3 prevTargetPos = Vector3.zero;
		private Vector3 initLocPos = Vector3.zero;
		
		void Start()
		{
			//set offset
			if(offset == Vector3.zero && target != null)
			{
				offset = target.position - transform.position;
			}
			
			//set prev pos
			if(target != null)
			{
				prevTargetPos = target.position;
			}
			
			//set init pos
			initLocPos = transform.localPosition;
		}
		
		
		void LateUpdate()
		{
			if (target == null) return;

			if(followRelative)
			{
				transform.localPosition = initLocPos + (target.position - prevTargetPos);
					
				if(damping == true)
				{
					transform.rotation = Quaternion.Lerp( transform.rotation, target.rotation, Time.deltaTime * dampingSpeed);
				}
				else
				{
					transform.rotation = target.rotation;
				}
			}
			else
			{
				if(offset != Vector3.zero)
				{
					transform.position = target.position + target.TransformDirection( offset);
				}
				else
				{
					transform.position = target.position;
				}

				if(damping == true)
				{
					transform.rotation = Quaternion.Lerp( transform.rotation, target.rotation, Time.deltaTime * dampingSpeed);
				}
				else
				{
					transform.rotation = target.rotation;
				}
			}
				
				
			//position children
			if( forceChildrenSamePosition == true )
			{
				for(int x = 0; x < transform.childCount; x++ )
				{
					transform.GetChild( x).localPosition = Vector3.zero;
				}
			}

			//set vars for next cycle
			prevTargetPos = target.position;
		}
		
		
	}
	
	
}