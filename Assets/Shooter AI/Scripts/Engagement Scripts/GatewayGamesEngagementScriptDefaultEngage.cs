using UnityEngine;
using System.Collections;


namespace GatewayGames.ShooterAI
{

	public class GatewayGamesEngagementScriptDefaultEngage : MonoBehaviour {
		
		void Start()
		{
			//go directly into engage
			gameObject.SendMessage( "EngageEnemy" );
		}
		
	}

}