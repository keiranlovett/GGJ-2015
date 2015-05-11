using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace GatewayGames.ShooterAI
{
	[AddComponentMenu("Shooter AI/Multiplayer Spawn Prefab upon Death") ]
	
	
	public class MultiplayerDeathSpawnPrefabs : MonoBehaviour 
	{
		
		
		public List<string> objectsToSpawn = new List<string>(); //a list containing the stuff you need to spawn, located in a Resource folder
		
		
		public void AIDead()
		{
			
			foreach(string objectToSpawn in objectsToSpawn)
			{
				PhotonNetwork.Instantiate( objectToSpawn, transform.position, transform.rotation, 0);
			}
			
		}
		
		
		
	}
	
}