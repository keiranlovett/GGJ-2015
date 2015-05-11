using UnityEngine;
using System.Collections;


namespace GatewayGames.ShooterAI
{
	
	
	
	
	/// <summary>
	/// Contains movement data that can be accessed from any script.
	/// </summary>
	[System.Serializable]
	public class MovementData
	{
		
		
		/// <summary>
		/// The current world position.
		/// </summary>
		public Vector3 position; 
		
		
		/// <summary>
		/// The destination.
		/// </summary>
		public Vector3 destination;
		
		
		/// <summary>
		/// The forward speed without the Y axis.
		/// </summary>
		public float forwardSpeed;
		
		
		/// <summary>
		/// The angular speed.
		/// </summary>
		public float angularSpeed;
		
		
		/// <summary>
		/// The velocity.
		/// </summary>
		public Vector3 velocity;
		
		
		/// <summary>
		/// The state of the strafing.
		/// </summary>
		public StrafingState strafingState = StrafingState.NoStrafing;
		
		
		/// <summary>
		/// Whether the AI is enroute to a destination, or just standing.
		/// </summary>
		public bool enRouteToDestination;
	}
	
	
	/// <summary>
	/// Strafing state.
	/// </summary>
	public enum StrafingState { StrafingLeft, NoStrafing, StrafingRight };
	
	
	
	
	/// <summary>
	/// Controls the AI movement and their respective calculations.
	/// </summary>
	
	public class GatewayGamesMovementContoller : MonoBehaviour 
	{
		
		public MovementData movementData = new MovementData(); //contains our movement data
		public float minDistanceToDestination = 0.9f; //the min distance to destination
		
		
		//caches
		NavMeshAgent agent;
		[HideInInspector]
		public GatewayGamesBrain brain;
		
		//strafing data
		[HideInInspector]
		public bool strafing = false;
		[HideInInspector]
		public Vector3 targetStrafingLocation = Vector3.zero;
		[HideInInspector]
		public float targetYRotation = 0f; 
		[HideInInspector]
		public float strafingYRotationLerp = 0.1f;
		[HideInInspector]
		public Vector3 relativeMovement = Vector3.zero;
		
		//movement data
		[HideInInspector]
		public Vector3 previousPos = Vector3.zero;
		[HideInInspector]
		public float prevAngularSpeed = 0f;
		[HideInInspector]
		public Vector3 targetDestination;
		
		
		
		
		void Awake()
		{
			//set caches
			agent = GetComponent<NavMeshAgent>();
			brain = GetComponent<GatewayGamesBrain>();
		}
		
		
		void Update()
		{
			//apply variables
			ApplyVariables();
			
			//control movement
			ControlMovement();
			
			//control strafing
			ControlStrafing();
			
			//calculate the data
			CalculateData();
		}
		
		
		
		
		/// <summary>
		/// Calculates the data.
		/// </summary>
		public virtual void CalculateData()
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
			if( agent.remainingDistance > minDistanceToDestination || movementData.forwardSpeed > 0.2f )
			{
				movementData.enRouteToDestination = true;
			}
			else
			{
				movementData.enRouteToDestination = false;
			}
			
			//set destination
			movementData.destination = agent.destination;
			
			//lastly, set the previous pos & angular speed
			previousPos = transform.position;
			prevAngularSpeed = movementData.angularSpeed;
		}
		
		
		
		/// <summary>
		/// Applies variables that are needed for functioning.
		/// </summary>
		public virtual void ApplyVariables()
		{
			//set min distance on the patol manager
			brain.patrolManager.criticalDistanceToWaypoint = minDistanceToDestination;
		}
		
		
		
		/// <summary>
		/// Sets the new destination.
		/// </summary>
		/// <param name="newDestination">New destination.</param>
		public virtual void SetNewDestination(Vector3 newDestination)
		{
			//set new destination if its new
			if( newDestination != movementData.destination)
			{
				agent.SetDestination( newDestination);
				targetDestination = newDestination;
				StopCoroutine( "StopAtGivenDistance" );
			}
			
		}


		/// <summary>
		/// Sets the speed.
		/// </summary>
		/// <param name="speed">Speed.</param>
		public virtual void SetSpeed(float speed)
		{
			//set the agent
			agent.speed = speed;

		}


		/// <summary>
		/// Sets the new destination and stops at the given distance.
		/// </summary>
		/// <param name="newDestination">New destination.</param>
		/// <param name="distanceToStop">Distance to stop.</param>
		public virtual void SetNewDestination( Vector3 newDestination, float distanceToStop )
		{
			//set new destination if its new
			if( newDestination != movementData.destination)
			{
				agent.SetDestination( newDestination);
				targetDestination = newDestination;
				
				//init stopping logic
				StopCoroutine( "StopAtGivenDistance" );
				StartCoroutine( "StopAtGivenDistance", distanceToStop );
			}
		}
		
		
		
		/// <summary>
		/// Strafes to destination with the specified y target rotation.
		/// </summary>
		/// <param name="destination">Destination.</param>
		/// <param name="targetYRotation">Target Y rotation.</param>
		public virtual void StrafeToDestination( Vector3 destination, float newtargetYRotation)
		{
			//deactivate normal agent operation
			agent.updatePosition = false;
			agent.updateRotation = false;
			
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
		public virtual void StrafeToDestination( Vector3 destination)
		{
			//deactivate normal agent operation
			agent.updatePosition = false;
			agent.updateRotation = false;
			
			//apply data that is needed
			strafing = true;
			targetStrafingLocation = destination;
			targetYRotation = transform.eulerAngles.y;
			SetNewDestination( destination);
			
		}
		
		
		
		/// <summary>
		/// Calculates the length to position.
		/// </summary>
		/// <returns>The length to position.</returns>
		/// <param name="point">Point.</param>
		public virtual float CalculateLengthToPosition( Vector3 point )
		{
			
			// Create a path and set it based on a target position.
			NavMeshPath path = new NavMeshPath();
			if(agent.enabled)
				agent.CalculatePath( point, path);
			
			// Create an array of points which is the length of the number of corners in the path + 2.
			Vector3 [] allWayPoints = new Vector3[path.corners.Length + 2];
			
			// The first point is the enemy's position.
			allWayPoints[0] = transform.position;
			
			// The last point is the target position.
			allWayPoints[allWayPoints.Length - 1] = point;
			
			// The points inbetween are the corners of the path.
			for(int i = 0; i < path.corners.Length; i++)
			{
				allWayPoints[i + 1] = path.corners[i];
			}
			
			// Create a float to store the path length that is by default 0.
			float pathLength = 0;
			
			// Increment the path length by an amount equal to the distance between each waypoint and the next.
			for(int i = 0; i < allWayPoints.Length - 1; i++)
			{
				pathLength += Vector3.Distance(allWayPoints[i], allWayPoints[i + 1]);
			}
			
			return pathLength;
		}
		
		
		
		/// <summary>
		/// Controls the movement.
		/// </summary>
		public virtual void ControlMovement()
		{
			//set stopping distance to the same as min distance to destination
			agent.stoppingDistance = minDistanceToDestination;
			
			
		}
		
		
		
		/// <summary>
		/// Controls strafing.
		/// </summary>
		public virtual void ControlStrafing()
		{
			if(strafing == true)
			{
				//lerp the rotation
				transform.rotation = Quaternion.Lerp( transform.rotation, Quaternion.Euler( transform.eulerAngles.x, targetYRotation, transform.eulerAngles.z), strafingYRotationLerp);
				
				//move towards the target
				agent.Move( (targetStrafingLocation - transform.position).normalized * agent.speed );
				
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
		/// Faces the AI with the specified angle.
		/// </summary>
		/// <param name="yAngle">Y angle.</param>
		public virtual void FaceTowards(float yAngle)
		{

			//pre calculations
			if( yAngle >= 360f)
			{
				yAngle = yAngle - 360f;
			}

			//apply data
			{
				transform.eulerAngles = new Vector3( transform.eulerAngles.x, 
				                                    Mathf.LerpAngle( transform.eulerAngles.y, yAngle, 0.1f ),
				                                    transform.eulerAngles.z);
			}


		}
		
		
		//<------------------------------------------------------------- HELPER FUNCTIONS -------------------------------------------->
		
		
		/// <summary>
		/// Stops at given distance.
		/// </summary>
		/// <returns>The at given distance.</returns>
		/// <param name="distance">Distance.</param>
		private IEnumerator StopAtGivenDistance(float distance)
		{
			//wait
			yield return new WaitForEndOfFrame();
			
			//check if close enough
			if( Vector3.Distance( transform.position, movementData.destination) <= distance )
			{
				SetNewDestination( transform.position);
			}
		}
		
		
		
		
		
	}
	
	
}
