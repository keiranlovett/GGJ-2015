using UnityEngine;
using System.Collections;

namespace GatewayGames.ShooterAI
{
	
	/// <summary>
	/// Health state.
	/// </summary>
	public enum HealthState { Normal, KockedDown, Dead };
	
	
	/// <summary>
	/// Manages the AI's health.
	/// </summary>
	public class GatewayGamesHealthManager : MonoBehaviour 
	{
		
		public GatewayGamesBrain brain; //the brain of the AI
		public HealthState healthState = HealthState.Normal; //the current health state
		public float healthPoints = 1f; //the amount of health points that this ai has
		
		
		private float secondsToGetUp = 2.63f; //the amount of time required to get up
		private GatewayGamesRagdollHelper ragdollManager; //the ragdoll reference
		private NavMeshAgent agent;
		private Transform modelRoot;
		
		
		void Awake()
		{
			//set references
			ragdollManager = brain.modelManager.GetComponent<GatewayGamesRagdollHelper>();
			agent = brain.GetComponent<NavMeshAgent>();
			modelRoot = brain.modelManager.GetComponent<Animator>().GetBoneTransform( HumanBodyBones.Hips );
		}
		
		
		void Update()
		{
			//check if still alive
			if( healthPoints <= 0f )
			{
				Die ();
			}
			
			//activate correct scripts when needed
			if( ragdollManager.state == GatewayGamesRagdollHelper.RagdollState.animated && agent != null && agent.enabled == false)
			{
				agent.enabled = true;
			}
			
			//set rotation correctly if lying on the floor
			if( healthState != HealthState.Normal )
			{
				transform.rotation = modelRoot.rotation;
			}
			else
			{
				transform.rotation = Quaternion.identity;
			}
			
		}
		
		
		
		
		
		/// <summary>
		/// Applies damage to the AI.
		/// </summary>
		/// <param name="amountOfDamage">Amount of damage.</param>
		/// <param name="death">If set to <c>true</c> instant death.</param>
		/// <param name="knockDown">If set to <c>true</c> knock down.</param>
		/// <param name="knockDownTime">Knock down time.</param>
		public virtual void ApplyDamageToAI(float amountOfDamage, bool death, bool knockDown, float knockDownTime)
		{
			//send message to brain that we're hit
			brain.SendMessage( "AIHit", SendMessageOptions.DontRequireReceiver );

			//apply the damage
			healthPoints -= amountOfDamage;
			
			//apply knock down if needed
			if(knockDown == true)
			{
				KnockDown( knockDownTime);
			}
			
			//apply death if needed
			if(death == true)
			{
				Die();
			}
			
			
		}
		
		
		/// <summary>
		///  Kills the AI.
		/// </summary>
		public virtual void Die()
		{
			//set vars
			healthState = HealthState.Dead;
			brain.SendMessage( "AIDead", SendMessageOptions.DontRequireReceiver );
			
			//Activate ragdoll
			ragdollManager.ragdolled = true;
			
			//Destroy all unneded components
			Destroy( brain.eyes.gameObject);
			Destroy( brain.GetComponent<GatewayGamesSearchCover>());
			Destroy( brain.GetComponent<GatewayGamesMovementContoller>());
			Destroy( brain.modelManager.GetComponent<ShooterAIIK>());
			Destroy( brain.modelManager.GetComponent<UpperBodyLookAt>());
			//Destroy( brain.modelManager.GetComponent<PhotonAnimatorView>());
			//Destroy( brain.modelManager.GetComponent<Animator>() );
			Destroy( brain.modelManager);
			if(agent != null)
			{
				Destroy( agent );
			}
			Destroy( brain.GetComponent<GatewayGamesWeaponManager>() );
			Destroy( brain );
			Destroy( gameObject );
			
			//reset tag
			brain.tag = "Untagged";
			
		}
		
		
		
		/// <summary>
		/// Knocks down the AI for the specified amount of time.
		/// </summary>
		/// <param name="time">Time.</param>
		public virtual void KnockDown(float time)
		{
			//set vars
			healthState = HealthState.KockedDown;
			
			//Activate ragdoll
			ragdollManager.ragdolled = true;
			
			//deactivate correct scripts
			if(agent != null)
			{
				agent.enabled = false;
			}
			brain.GetComponent<GatewayGamesMovementContoller>().enabled = false;
			brain.modelManager.GetComponent<ShooterAIIK>().enabled = false;
			brain.modelManager.GetComponent<UpperBodyLookAt>().enabled = false;
			
			//time getting back up
			StartCoroutine( AIStandUpAfterTime(time) );
			
		}
		
		
		
		
		//<---------------------------------------------------- HELPER FUNCTIONS --------------------------------------->
		
		
		
		/// <summary>
		/// Makes the AI stand back up after the specified amount of time.
		/// </summary>
		/// <returns>The stand up after time.</returns>
		/// <param name="time">Time.</param>
		private IEnumerator AIStandUpAfterTime(float time)
		{
			//first wait
			yield return new WaitForSeconds( time);
			
			//reactivate the model
			ragdollManager.ragdolled = false;
			
			//wait for getting up
			yield return new WaitForSeconds( secondsToGetUp );
			
			//activate all other scripts
			brain.GetComponent<GatewayGamesMovementContoller>().enabled = true;
			if(agent != null)
			{
				agent.enabled = true;
			};
			healthState = HealthState.Normal;
			brain.modelManager.GetComponent<ShooterAIIK>().enabled = true;
			brain.modelManager.GetComponent<UpperBodyLookAt>().enabled = true;
		}
		
	}
	
	
}
