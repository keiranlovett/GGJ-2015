using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace GatewayGames.ShooterAI
{
	
	/// <summary>
	/// Controls the ears.
	/// </summary>
	public class GatewayGamesEars : MonoBehaviour 
	{
		
		
		public GatewayGamesBrain brain; //the reference to the brain
		public GatewayGamesMovementContoller movement; //the reference to the movement
		public float distanceOfHearingEnemy = 5f; //the real distance for this ai to hear the enemy
		public float distanceOfHearingBullet = 20f; //the radius from which this ai can hear bullets
		public float bulletHearingSmudgingFactor = 15f; //the smudging factor to offset the exact bullets distance
		public bool canHearEnemy = false; //whether we can here the enemy or not
		public float secondsForHearingLocationToRemainActive = 10f; //after how many seconds will the last heard location be forgotten
		public bool getPosBullet = false; //only acitavte if using projectiles
		public bool debug = false; //whether to debug
		
		
		private Collider[] getEnemyColliders; //this is used often in the GetEnemy method
		private Collider[] getBulletColliders; //this is used often in the GetBullet method
		private float getEnemyMinDistance = float.MaxValue; //this is used often in the GetEnemy method to determine the closest enemy
		private GameObject getEnemyPotential = null; //this is used often in the GetEnemy method to temp store the closest enemy
		private Vector3 getBulletPotPosition = Vector3.zero; //this is used often in the GetBullet method to store temp values about the bullets
		private GameObject[] getEnemies; //the enemy list
		
		
		//optimization
		private int optimizeDefaultFrames = 50;
		private int optimizeCurrentStep = 0;
		
		
		void Awake()
		{
			//set optimization vars
			optimizeCurrentStep = Random.Range( 0, optimizeDefaultFrames);
		}
		
		
		/// <summary>
		/// Ticks this instance.
		/// </summary>
		public virtual void Tick()
		{
			//do optimization
			optimizeCurrentStep += 1;
			if(optimizeCurrentStep < optimizeDefaultFrames)
			{
				return;
			}
			else
			{
				optimizeCurrentStep = 0;
			}
			
			if(brain.searchForNewAI == true)
			{
				
				//set enemies and bullets data correctly in the brain
				if(getPosBullet == true)
				{
					brain.lastHeardBulletLocation = GetBullet( brain.tagOfBullet);
				}
				getEnemyPotential = GetEnemyWithinEarshot( brain.tagOfEnemy);
				
				if(getEnemyPotential != null)
				{
					brain.lastHeardEnemyLocation = getEnemyPotential.transform.position + ( Random.insideUnitSphere * bulletHearingSmudgingFactor/5f );
					StopCoroutine( "ResetLastHeardLocation");
					StartCoroutine( "ResetLastHeardLocation", secondsForHearingLocationToRemainActive );
				}
				
				//set enemy correctly
				brain.currentEnemy = GetEnemy( brain.tagOfEnemy );
			}
			
			
		}
		
		
		
		
		/// <summary>
		/// Calculates the enemy using ears.
		/// </summary>
		/// <returns>The enemy.</returns>
		public GameObject GetEnemyWithinEarshot(string tagOfEnemy)
		{
			//reset vars
			getEnemyMinDistance = float.MaxValue;
			getEnemyPotential = null;
			canHearEnemy = false;

			
			//get all our enemies
			GameObject[] potentialEnemies = GameObject.FindGameObjectsWithTag( tagOfEnemy);
			
			//check each one for closeness and readiness
			foreach(GameObject potentialEnemy in potentialEnemies)
			{
				float tempDist = Vector3.Distance( potentialEnemy.transform.position, movement.transform.position ) * (1f/Vector3.Distance(potentialEnemy.transform.position, brain.lastHeardBulletLocation));
				float actualDist = Vector3.Distance( potentialEnemy.transform.position, movement.transform.position );
				
				if( actualDist < distanceOfHearingEnemy && tempDist < getEnemyMinDistance)
				{
					//set temp vars
					getEnemyPotential = potentialEnemy.gameObject;
					getEnemyMinDistance = tempDist;
					
					//set permanent vars
					canHearEnemy = true;
				}
			}

			
			//return the closest
			return getEnemyPotential;
			
		}
		
		
		
		
		/// <summary>
		/// Gets the enemy to attack.
		/// </summary>
		/// <returns>The enemy.</returns>
		/// <param name="tagOfEnemy">Tag of enemy.</param>
		public GameObject GetEnemy(string tagOfEnemy)
		{
			//reset vars
			getEnemyMinDistance = float.MaxValue;
			getEnemyPotential = null;
			
			//get the colliders of the enemy
			getEnemies = GameObject.FindGameObjectsWithTag( tagOfEnemy);
			
			
			foreach(GameObject potentialEnemy in getEnemies  )
			{
				//check if they're actually the enemies that we're searching for and is within hearing distance
				float tempDist = Vector3.Distance( potentialEnemy.transform.position, movement.transform.position ) * (1f/Vector3.Distance(potentialEnemy.transform.position, brain.lastHeardBulletLocation));
				
				if( potentialEnemy.CompareTag(tagOfEnemy) == true && tempDist < getEnemyMinDistance)
				{
					//set temp vars
					getEnemyPotential = potentialEnemy.gameObject;
					getEnemyMinDistance = tempDist;
					
				}
			}
			
			//return the closest
			return getEnemyPotential;
		}
		
		
		
		
		
		/// <summary>
		/// Gets the position of the nearest bullet, with smudging.
		/// </summary>
		/// <returns>The bullet.</returns>
		/// <param name="tagOfBullet">Tag of bullet.</param>
		public Vector3 GetBullet(string tagOfBullet)
		{
			//check if we need to search
			if(getPosBullet == false)
			{
				return Vector3.zero;
			}
			
			//reset vars
			getEnemyMinDistance = float.MaxValue;
			
			//get the colliders of the enemy
			getBulletColliders = Physics.OverlapSphere( transform.position, distanceOfHearingBullet);
			
			
			foreach(Collider potentialBullet in getBulletColliders  )
			{
				//check if they're actually the enemies that we're searching for and is within hearing distance
				float tempDist = Vector3.SqrMagnitude( potentialBullet.transform.position - transform.position );
				
				if( potentialBullet.CompareTag(tagOfBullet) == true && tempDist < getEnemyMinDistance * getEnemyMinDistance)
				{
					//set temp vars
					getBulletPotPosition = potentialBullet.transform.position;
					getEnemyMinDistance = Vector3.Distance( transform.position, potentialBullet.transform.position);
					
				}
			}
			
			//return the closest bullet with smudging
			return getBulletPotPosition + ( Random.insideUnitSphere * bulletHearingSmudgingFactor );
			
		}
		
		
		/// <summary>
		/// Call this function if you need to simulate the 
		/// </summary>
		/// <param name="posOfBullet">Position of bullet.</param>
		public void BulletFired(Vector3 posOfBullet)
		{
			brain.lastHeardBulletLocation = posOfBullet + ( Random.insideUnitSphere * bulletHearingSmudgingFactor );
			
			//Debug.Log("Heard bullet: " + posOfBullet, transform);
		}
		
		
		
		//<---------- HELPER FUNCTIONS --------------->
		
		
		
		/// <summary>
		/// Resets the last heard location.
		/// </summary>
		/// <returns>The last heard location.</returns>
		/// <param name="time">Time.</param>
		private IEnumerator ResetLastHeardLocation(float time)
		{
			//wait
			yield return new WaitForSeconds( time);
			
			//reset
			brain.lastHeardEnemyLocation = Vector3.zero;
			
		}
		
		void OnDrawGizmosSelected()
		{
			//draw vieing frustrum
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere( transform.position, distanceOfHearingEnemy);
		}
	}

}