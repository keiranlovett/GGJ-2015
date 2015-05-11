using UnityEngine;
using System.Collections;

namespace GatewayGames.ShooterAI
{
	
	/// <summary>
	/// Handles damage areas. Attach to collider.
	/// </summary>
	public class GatewayGamesDamageRegion : MonoBehaviour 
	{
		public GatewayGamesHealthManager healthManager; //the health manager
		public float hitAreaDefaultDamage = 0.3f; //by default, how much this area applies damage to the ai
		public bool deathArea = false; //whether a hit to this area kills the AI instantly
		public bool knockDownArea = false; //whether a hit to this area will knock the AI down
		public float knockDownTime = 5f; //(ONLY USE IF "knockDownArea" IS SET TO TRUE) how many seconds this ai will be knocked down for
		
		
		
		void Start()
		{
			//set collider to trigger
			GetComponent<Collider>().isTrigger = true;
		}
		
		
		
		void OnTriggerEnter(Collider colData)
		{
			//check if a bullet hit us
			if( healthManager.brain.tagOfBullet != null && colData.gameObject.CompareTag( healthManager.brain.tagOfBullet) )
			{
				//apply default damage
				healthManager.ApplyDamageToAI( hitAreaDefaultDamage, deathArea, knockDownArea, knockDownTime);
			}
			
		}
		
		
		/// <summary>
		/// Applies the damage. Used by UFPS.
		/// </summary>
		/// <param name="amount">Amount.</param>
		public void ApplyDamage()
		{
			//apply damage
			healthManager.ApplyDamageToAI( hitAreaDefaultDamage, deathArea, knockDownArea, knockDownTime);
		}


		/// <summary>
		/// Applies the damage. Used by UFPS.
		/// </summary>
		/// <param name="amount">Amount.</param>
		public void ApplyDamage(float amount)
		{
			//apply damage
			healthManager.ApplyDamageToAI( amount, deathArea, knockDownArea, knockDownTime);
		}

		
		/// <summary>
		/// Applies the damage. Used by RFPS.
		/// </summary>
		/// <param name="amount">Amount.</param>
		public void Damage()
		{
			//apply damage
			healthManager.ApplyDamageToAI( hitAreaDefaultDamage, deathArea, knockDownArea, knockDownTime);
		}
		
		/// <summary>
		/// Applies the damage. Used by RFPS.
		/// </summary>
		/// <param name="amount">Amount.</param>
		public void Damage(float amount)
		{
			//apply damage
			healthManager.ApplyDamageToAI( amount, deathArea, knockDownArea, knockDownTime);
		}
		
		
		
	}
	
}
