using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace GatewayGames.ShooterAI
{
	[AddComponentMenu("Shooter AI/Spawn Prefab upon Death") ]
	
	
	public class DeathSpawnPrefabs : MonoBehaviour 
	{
		
		
		public List<GameObject> objectsToSpawn = new List<GameObject>(); //a list containing the stuff you need to spawn
		
		
		public void AIDead()
		{
			
			foreach(GameObject objectToSpawn in objectsToSpawn)
			{
				Instantiate( objectToSpawn, transform.position, transform.rotation);
			}
			
		}
		
		
		
	}
	
}