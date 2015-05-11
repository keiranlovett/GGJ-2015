using UnityEngine;
using System.Collections;
using Pathfinding;


namespace GatewayGames.ShooterAI
{
	
	public class GatewayGamesMovementControllerASTAR : GatewayGamesMovementContoller 
	{
		
		//caches
		//private Seeker seeker;
		private GatewayGamesAgent agentA;
		
		
		void Awake()
		{
			//seeker = GetComponent<Seeker>();
			agentA = GetComponent<GatewayGamesAgent>();
			brain = GetComponent<GatewayGamesBrain>();
		}
		
		
		public override void CalculateData ()
		{
			//first calculate the actual velocity
			movementData.velocity = transform.position - previousPos;
			
			//then calculate the forward velocity without the Y axis
			movementData.forwardSpeed = ( ( new Vector3( transform.position.x, 0f, transform.position.z ) - new Vector3( previousPos.x, 0f, previousPos.z ) )/Time.deltaTime).magnitude;
			
			
			//calculate the angular speed
			movementData.angularSpeed = ( Mathf.Lerp( prevAngularSpeed, Mathf.Atan2( (Quaternion.Inverse(transform.rotation) * movementData.velocity).x,
			                                                                        (Quaternion.Inverse(transform.rotation) * movementData.velocity).z ) * 180.0f / 3.14159f, 0.5f) 
			                             ) * Mathf.Deg2Rad;
			
			//apply strafing
			{
				if( strafing == true )
				{
					//calculate relative position
					relativeMovement = transform.InverseTransformPoint( targetStrafingLocation );
					
					//now calculate whether the strafing point is to the right or left
					if(relativeMovement.x > 0f)
					{
						movementData.strafingState = StrafingState.StrafingLeft;
					}
					else
					{
						movementData.strafingState = StrafingState.StrafingRight;
					}
				}
				else
				{
					movementData.strafingState = StrafingState.NoStrafing;
				}
				
			}
			
			//set position
			movementData.position = transform.position;
			
			
			//set enroute data
			if( movementData.forwardSpeed > 0.2f )
			{
				movementData.enRouteToDestination = true;
			}
			else
			{
				movementData.enRouteToDestination = false;
			}
			
			//set destination
			movementData.destination = agentA.destination;
			
			//lastly, set the previous pos & angular speed
			previousPos = transform.position;
			prevAngularSpeed = movementData.angularSpeed;
		}
		
		
		
		public override void ControlMovement ()
		{
			//set stopping distance to the same as min distance to destination
			agentA.endReachedDistance = minDistanceToDestination;
		}
		
		
		public override void ControlStrafing ()
		{
			if(strafing == true)
			{
				//lerp the rotation
				transform.rotation = Quaternion.Lerp( transform.rotation, Quaternion.Euler( transform.eulerAngles.x, targetYRotation, transform.eulerAngles.z), strafingYRotationLerp);
				
				//move towards the target
				transform.Translate ( (targetStrafingLocation - transform.position).normalized * agentA.speed );
				
				//test if we're there
				if( Vector3.Distance( transform.position, targetStrafingLocation ) <= minDistanceToDestination + 0.1f )
				{
					//deactivate strafing if we are
					strafing = false;
					targetStrafingLocation = Vector3.zero;
					targetYRotation = 0f;
					
				}
			}
		}
		
		
		/// <summary>
		/// Calculates the length to position.
		/// </summary>
		/// <returns>The length to position.</returns>
		/// <param name="point">Point.</param>
		public override float CalculateLengthToPosition (Vector3 point)
		{
			
			//do approximate vector3
			return Vector3.Distance( transform.position, point);
			
		}
		
		
		/// <summary>
		/// Strafes to destination with the specified y target rotation.
		/// </summary>
		/// <param name="destination">Destination.</param>
		/// <param name="targetYRotation">Target Y rotation.</param>
		public override void StrafeToDestination( Vector3 destination, float newtargetYRotation)
		{
			//deactivate normal agent operation
			agentA.canMove = false;
			
			//apply data that is needed
			strafing = true;
			targetStrafingLocation = destination;
			targetYRotation = newtargetYRotation;
			SetNewDestination( destination);
			
		}
		
		
		/// <summary>
		/// Strafes to destination with the current y target rotation.
		/// </summary>
		/// <param name="destination">Destination.</param>
		/// <param name="targetYRotation">Target Y rotation.</param>
		public override void StrafeToDestination( Vector3 destination)
		{
			//deactivate normal agent operation
			agentA.canMove = false;
			
			//apply data that is needed
			strafing = true;
			targetStrafingLocation = destination;
			targetYRotation = transform.eulerAngles.y;
			SetNewDestination( destination);
			
		}
		
		
		/// <summary>
		/// Sets the new destination.
		/// </summary>
		/// <param name="newDestination">New destination.</param>
		public override void SetNewDestination(Vector3 newDestination)
		{
			//set new destination if its new
			if( newDestination != movementData.destination)
			{
				agentA.SearchPath( newDestination);
				targetDestination = newDestination;
				StopCoroutine( "StopAtGivenDistance" );
			}
			
		}
		
		
		/// <summary>
		/// Sets the speed.
		/// </summary>
		/// <param name="speed">Speed.</param>
		public override void SetSpeed(float speed)
		{
			//set the agent
			agentA.speed = speed;
			
		}
		
		
		/// <summary>
		/// Sets the new destination and stops at the given distance.
		/// </summary>
		/// <param name="newDestination">New destination.</param>
		/// <param name="distanceToStop">Distance to stop.</param>
		public override void SetNewDestination( Vector3 newDestination, float distanceToStop )
		{
			//set new destination if its new
			if( newDestination != movementData.destination)
			{
				agentA.SearchPath( newDestination);
				targetDestination = newDestination;
				
				//init stopping logic
				StopCoroutine( "StopAtGivenDistance" );
				StartCoroutine( "StopAtGivenDistance", distanceToStop );
			}
		}
		
		/// <summary>
		/// Applies variables that are needed for functioning.
		/// </summary>
		public override void ApplyVariables()
		{
			//set min distance on the patol manager
			brain.patrolManager.criticalDistanceToWaypoint = minDistanceToDestination;
		}
	}
}