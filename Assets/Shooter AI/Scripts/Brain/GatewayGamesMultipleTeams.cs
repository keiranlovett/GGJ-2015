using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace GatewayGames.ShooterAI
{
	
	[AddComponentMenu("Shooter AI/Multiple Teams")]
	
	/// <summary>
	/// Handles the selection of the current enemy teams from a list.
	/// </summary>
	public class GatewayGamesMultipleTeams : MonoBehaviour {
		
		
		public List<string> listOfTags = new List<string>(); //list containing all the different enemy teams' tags
		
		private int framesToCheck = 250; //once how many frames should we check on the enemy situation
		private int framesChecked = 0; //how many frames we checked
		
		void Start()
		{
			//anti-bottlenecking
			framesToCheck += Random.Range( -30, 30);
		}
		
		
		void Update()
		{
			framesChecked += 1;
			
			if(framesChecked > framesToCheck)
			{
				framesChecked = 0;
				
				//check the closest team
				float smallesDistance = 100000000f;
				string closestTeam = "";
				
				foreach(string team in listOfTags)
				{
					
					foreach(GameObject ai in GameObject.FindGameObjectsWithTag(team) )
					{
						
						float dis = Vector3.Distance( transform.position, ai.transform.position);
						
						if(dis < smallesDistance)
						{
							smallesDistance = dis;
							closestTeam = team;
						}
						
					}
					
				}
				
				if(closestTeam != "")
				{
					GetComponent<GatewayGamesBrain>().tagOfEnemy = closestTeam;
				}
				
				
			}
			
			
		}
		
		
		
	}
	
	
}