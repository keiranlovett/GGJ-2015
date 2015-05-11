using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace GatewayGames.ShooterAI
{
	
	/// <summary>
	/// Controls the eyes.
	/// </summary>
	public class GatewayGamesEyes : MonoBehaviour 
	{
		public GatewayGamesBrain brain; //the brain reference
		public float fieldOfView = 60f; //the field of view of the ai
		public float maxSeeDistance = float.MaxValue; //the max view distance of the ai
		public bool canSeeEnemy = false; //whether we can see the enemy
		public bool debug = false; //whether to debug or not
		public float secondsToRememberLastSeenLoc = 20f; //the amount of time that this ai remebers the player last seen location
		
		private bool canSeeEnemyResult = false; //whether we can see the enemy or not
		private Vector3 lastSeenLocation = Vector3.zero; //the temp last seen location
		private float timeLeftTillForgetLastSeenLoc = 0f; //the amount of time left before we forget the last seen location of the enemy
		
		
		
		void Update()
		{
			//update last seen location
			if( canSeeEnemy == false )
			{
				timeLeftTillForgetLastSeenLoc -= Time.deltaTime;
				if( timeLeftTillForgetLastSeenLoc <= 0f )
				{
					brain.lastSeenEnemyLocation = Vector3.zero;
				}
			}
			
			
		}
		
		
		/// <summary>
		/// Ticks this instance.
		/// </summary>
		public virtual void Tick()
		{
			//set enemies and bullets data correctly in the brain
			canSeeEnemy = CanSeeEnemy( brain.currentEnemy );
			
			//set last seen data
			if(canSeeEnemy == true)
			{
				brain.lastSeenEnemyLocation = lastSeenLocation;
				timeLeftTillForgetLastSeenLoc = secondsToRememberLastSeenLoc;
			}
		}
		
		
		
		/// <summary>
		/// Determines whether this ai can see the specified enemy.
		/// </summary>
		/// <returns><c>true</c> if this instance can see enemy the specified enemy; otherwise, <c>false</c>.</returns>
		/// <param name="enemy">Enemy.</param>
		public bool CanSeeEnemy(GameObject enemy)
		{
			//check null reference errors
			if(enemy == null)
			{
				return false;
			}
			
			
			//reset vars
			canSeeEnemyResult = false;
			RaycastHit hit;
			
			
			//debug
			if(debug)
			{
				Debug.DrawLine( (transform.position + transform.forward * 0.5f), enemy.transform.position, Color.green);
			}
			
			//if we're closer than 0.6 to the enemy and within 180 FOV, we automatically see it
			if( Vector2.Distance( new Vector2( brain.transform.position.x, brain.transform.position.z), 
			                     new Vector2(enemy.transform.position.x, enemy.transform.position.z) ) <= 0.6f && 
			   Mathf.Abs (enemy.transform.position.y - brain.transform.position.y) <= 1.5f && 
			   Vector3.Angle( transform.forward, (enemy.transform.position - transform.position)) < 180f )
			{	
				canSeeEnemyResult = true;
				lastSeenLocation = enemy.transform.position;
				return true;
			}
			
			
			//we have to make the eye level smaller or else the ai wont see standard players
			Vector3 rayToCheck = enemy.transform.position - (transform.position + transform.forward * 0.5f) + new Vector3( 0f, 0.1f, 0f);
			
			//if the object is wihtin our field of view and is closer than the max distance
			if((Vector3.Angle(rayToCheck, transform.forward)) < fieldOfView/2f && rayToCheck.magnitude < maxSeeDistance)
			{
				
				//send out the ray
				if(Physics.Raycast(transform.position, rayToCheck, out hit))
				{
					//check if this is the enemy
					if( brain.ObjectAtLocation( enemy.transform, hit.point) == true )
					{
						canSeeEnemyResult = true;
						lastSeenLocation = enemy.transform.position;
					}
					
					//debug
					if(debug)
					{
						Debug.Log( hit.collider.gameObject );
					}
					
				}
				
			}
			
			//return result
			return canSeeEnemyResult;
			
		}
		
		
		/// <summary>
		/// Determines whether this instance can see enemy the specified enemy at the eyeLevel.
		/// </summary>
		/// <returns><c>true</c> if this instance can see enemy the specified enemy eyeLevel; otherwise, <c>false</c>.</returns>
		/// <param name="enemy">Enemy.</param>
		/// <param name="eyeLevel">Eye level in absolute position.</param>
		public bool CanSeeEnemy(GameObject enemy, Vector3 eyeLevel)
		{
			
			//check null reference errors
			if(enemy == null)
			{
				return false;
			}
			
			
			//reset vars
			bool canSeeEnemyResult2 = false;
			RaycastHit hit;
			
			
			//debug
			if(debug)
			{
				Debug.DrawLine( (eyeLevel + transform.forward * 0.5f), enemy.transform.position, Color.green);
			}
			
			//if we're closer than 0.6 to the enemy and within 180 FOV, we automatically see it
			if( Vector2.Distance( new Vector2( brain.transform.position.x, brain.transform.position.z), 
			                     new Vector2(enemy.transform.position.x, enemy.transform.position.z) ) <= 0.6f && 
			   Mathf.Abs (enemy.transform.position.y - brain.transform.position.y) <= 1.5f && 
			   Vector3.Angle( transform.forward, (enemy.transform.position - eyeLevel)) < 180f )
			{	
				canSeeEnemyResult2 = true;
				lastSeenLocation = enemy.transform.position;
				return true;
			}
			
			
			//we have to make the eye level smaller or else the ai wont see standard players
			Vector3 rayToCheck = enemy.transform.position - (eyeLevel + transform.forward * 0.5f) + new Vector3( 0f, 0.1f, 0f);
			
			//if the object is wihtin our field of view and is closer than the max distance
			if((Vector3.Angle(rayToCheck, transform.forward)) < fieldOfView/2f && rayToCheck.magnitude < maxSeeDistance)
			{
				
				//send out the ray
				if(Physics.Raycast(eyeLevel, rayToCheck, out hit))
				{
					//check if this is the enemy
					if( brain.ObjectAtLocation( enemy.transform, hit.point) == true )
					{
						canSeeEnemyResult2 = true;
						lastSeenLocation = enemy.transform.position;
					}
					
					//debug
					if(debug)
					{
						Debug.Log( hit.collider.gameObject );
					}
					
				}
				
			}
			
			//return result
			return canSeeEnemyResult2;
			
		}
		
		
		/// <summary>
		/// Determines whether this instance can see the position specified in posToSee.
		/// </summary>
		/// <returns><c>true</c> if this instance can see position the specified posToSee; otherwise, <c>false</c>.</returns>
		/// <param name="posToSee">Position to see.</param>
		public bool CanSeePosition(Vector3 posToSee)
		{
			RaycastHit hit;
			
			
			//if we're closer than 0.6 to the enemy and within 180 FOV, we automatically see it
			if( Vector2.Distance( new Vector2( brain.transform.position.x, brain.transform.position.z), 
			                     new Vector2(posToSee.x, posToSee.z) ) <= 0.6f && 
			   Mathf.Abs (posToSee.y - brain.transform.position.y) <= 1.5f && 
			   Vector3.Angle( transform.forward, (posToSee - transform.position)) < 180f )
			{	
				return true;
			}
			
			
			//we have to make the eye level smaller or else the ai wont see standard players
			Vector3 rayToCheck = posToSee - (transform.position + transform.forward * 0.5f) + new Vector3( 0f, 0.1f, 0f);
			
			//if the object is wihtin our field of view and is closer than the max distance
			if((Vector3.Angle(rayToCheck, transform.forward)) < fieldOfView/2f && rayToCheck.magnitude < maxSeeDistance)
			{
				
				//send out the ray
				if(Physics.Raycast(transform.position, rayToCheck, out hit))
				{
					//check if we hit close enough
					if( Vector3.Distance( hit.point, posToSee ) <= 0.1f )
					{
						return true;
					}
					
					
				}
				
			}
			
			return false;
		}
		
		
		/// <summary>
		/// Determines whether this instance can see the specified posToSee from the eyeLevel.
		/// </summary>
		/// <returns><c>true</c> if this instance can see position the specified posToSee eyeLevel; otherwise, <c>false</c>.</returns>
		/// <param name="posToSee">Position to see.</param>
		/// <param name="eyeLevel">Eye level.</param>
		public bool CanSeePosition(Vector3 posToSee, Vector3 eyeLevel)
		{
			RaycastHit hit;
			
			
			//if we're closer than 0.6 to the enemy and within 180 FOV, we automatically see it
			if( Vector2.Distance( new Vector2( brain.transform.position.x, brain.transform.position.z), 
			                     new Vector2(posToSee.x, posToSee.z) ) <= 0.6f && 
			   Mathf.Abs (posToSee.y - brain.transform.position.y) <= 1.5f && 
			   Vector3.Angle( transform.forward, (posToSee - eyeLevel)) < 180f )
			{	
				return true;
			}
			
			
			//we have to make the eye level smaller or else the ai wont see standard players
			Vector3 rayToCheck = posToSee - (eyeLevel + transform.forward * 0.5f) + new Vector3( 0f, 0.1f, 0f);
			
			//if the object is wihtin our field of view and is closer than the max distance
			if((Vector3.Angle(rayToCheck, transform.forward)) < fieldOfView/2f && rayToCheck.magnitude < maxSeeDistance)
			{
				
				//send out the ray
				if(Physics.Raycast(eyeLevel, rayToCheck, out hit))
				{
					//check if we hit close enough
					if( Vector3.Distance( hit.point, posToSee ) <= 0.1f )
					{
						return true;
					}
					
					
				}
				
			}
			
			return false;
		}
	
		void OnDrawGizmosSelected()
		{
			//draw vieing frustrum
			Gizmos.color = Color.green;
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
			Gizmos.DrawFrustum( transform.position, fieldOfView, maxSeeDistance, 0.1f, 1f);
		}
		
	}
}
