using UnityEngine;
using System.Collections;
using GatewayGames.ShooterAI;


[AddComponentMenu("Shooter AI/Force AI To Engage Enemy") ]
public class UpdateEnforcedEnemy : MonoBehaviour {
	
	public GameObject enemy;
	
	private GatewayGamesBrain brain;
	
	void Awake()
	{
		//set caches
		brain = GetComponent<GatewayGamesBrain>();
	}
	
	
	void Update()
	{
		if(brain == null)
		{
			return;
		}
		
		if(enemy == null)
		{
			brain.eyes.canSeeEnemy = false;
			brain.ears.canHearEnemy = false;
			return;
		}
		
		//set varaibles correctly
		brain.eyes.canSeeEnemy = brain.eyes.CanSeeEnemy( enemy);
		brain.ears.canHearEnemy = CanHearEnemy();
		brain.searchForNewAI = false;
		brain.currentEnemy = enemy;
		
		if(brain.eyes.canSeeEnemy == true)
		{
			brain.lastSeenEnemyLocation = enemy.transform.position;
		}
		
		if(brain.ears.canHearEnemy == true)
		{
			brain.lastHeardEnemyLocation = enemy.transform.position;
		}
	}
	

	
	bool CanHearEnemy()
	{
		float dist = Vector3.Distance( transform.position, enemy.transform.position);
		
		if(dist <= brain.ears.distanceOfHearingEnemy)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}
