using UnityEngine;
using System.Collections;
using UnityEditor;


namespace GatewayGames.ShooterAI
{
	
	
	
	[CustomEditor( typeof( GatewayGamesBrain ) )]
	
	/// <summary>
	/// Gateway games brain editor.
	/// </summary>
	public class GatewayGamesBrainEditor : Editor 
	{
		
		//fold out data
		private bool[] foldouts = new bool[ 6 ];
		
		
		
		public override void OnInspectorGUI()
		{
			//first set vars correctly
			GatewayGamesBrain myTarget = (GatewayGamesBrain) target;
			
			EditorGUILayout.Space();
			
			//foldout: references
			foldouts[0] = EditorGUILayout.Foldout( foldouts[0], "Object References" );
			
			if( foldouts[0] == true )
			{
				EditorGUILayout.Space();
				
				myTarget.ears = EditorGUILayout.ObjectField( "Ears: ", myTarget.ears, typeof( GatewayGamesEars ), true ) as GatewayGamesEars;
				myTarget.eyes = EditorGUILayout.ObjectField( "Eyes: ", myTarget.eyes, typeof( GatewayGamesEyes ), true ) as GatewayGamesEyes;
				myTarget.modelManager = EditorGUILayout.ObjectField( "Model Manager: ", myTarget.modelManager, typeof( GatewayGamesModelManager ), true ) as GatewayGamesModelManager;
				myTarget.patrolManager = EditorGUILayout.ObjectField( "Patrol Manager: ", myTarget.patrolManager, typeof( GatewayGamesPatrolManager ), true ) as GatewayGamesPatrolManager;
				myTarget.healthManager = EditorGUILayout.ObjectField( "Health Manager: ", myTarget.healthManager, typeof( GatewayGamesHealthManager ), true ) as GatewayGamesHealthManager;
				
				EditorGUILayout.Space();
			}
			
			
			//folout: stats about ai
			foldouts[1] = EditorGUILayout.Foldout( foldouts[1], "AI Stats" );
			
			if( foldouts[1] == true )
			{
				EditorGUILayout.Space();
				
				myTarget.currentState = (CurrentState) EditorGUILayout.EnumPopup( "Current State: ", myTarget.currentState );
				myTarget.engagementState = (EngageState) EditorGUILayout.EnumPopup( "Current Engagement State: ", myTarget.engagementState );
				myTarget.canEngage = EditorGUILayout.Toggle( "Can Engage: ", myTarget.canEngage );
				myTarget.allowCrouching = EditorGUILayout.Toggle( "Can Crouch: ", myTarget.allowCrouching );
				myTarget.crouching = EditorGUILayout.Toggle( "Currently Crouching: ", myTarget.crouching );
				
				EditorGUILayout.Space();
			}
			
			//foldout: speeds
			foldouts[2] = EditorGUILayout.Foldout( foldouts[2], "AI Speeds" );
			
			if(foldouts[2] == true)
			{
				EditorGUILayout.Space();
				
				myTarget.aiSpeeds.defaultSpeed = EditorGUILayout.FloatField( "Default Speed: ", myTarget.aiSpeeds.defaultSpeed );
				myTarget.aiSpeeds.chaseSpeed = EditorGUILayout.FloatField( "Chase Speed: ", myTarget.aiSpeeds.chaseSpeed );
				myTarget.aiSpeeds.engageSpeed = EditorGUILayout.FloatField( "Engage Speed: ", myTarget.aiSpeeds.engageSpeed );
				
				EditorGUILayout.Space();
			}
			
			//foldout: enemy data
			foldouts[3] = EditorGUILayout.Foldout( foldouts[3], "Enemy Data" );
			
			if( foldouts[3] == true )
			{
				EditorGUILayout.Space();
				
				myTarget.currentEnemy = EditorGUILayout.ObjectField( "Current Enemy: ", myTarget.currentEnemy, typeof( GameObject ), true ) as GameObject;
				myTarget.enemyTargetArea = EditorGUILayout.Vector3Field( "Enemy Kill Area: ", myTarget.enemyTargetArea );
				myTarget.enemyTargetRandomOffset = EditorGUILayout.Vector3Field( "Aim Offset: ", myTarget.enemyTargetRandomOffset );
				myTarget.lastHeardEnemyLocation = EditorGUILayout.Vector3Field( "Last Heard Location: ", myTarget.lastHeardEnemyLocation );
				myTarget.lastHeardBulletLocation = EditorGUILayout.Vector3Field( "Last Heard Bullet: ", myTarget.lastHeardBulletLocation );
				myTarget.lastSeenEnemyLocation = EditorGUILayout.Vector3Field( "Last Seen Location: ", myTarget.lastSeenEnemyLocation );
				myTarget.tagOfEnemy = EditorGUILayout.TagField( "Tag Of Enemy: ",  myTarget.tagOfEnemy);
				myTarget.tagOfBullet = EditorGUILayout.TagField( "Tag Of Bullet: ", myTarget.tagOfBullet );
				myTarget.searchForNewAI = EditorGUILayout.Toggle( "Search Automatically For New Enemies: ", myTarget.searchForNewAI );
				
				EditorGUILayout.Space();
			}
			
			//foldout: emotions
			foldouts[4] = EditorGUILayout.Foldout( foldouts[4], "Emotion Data" );
			
			if( foldouts[4] == true )
			{
				EditorGUILayout.Space();
				
				myTarget.fear = EditorGUILayout.Slider( "Fear: ", myTarget.fear, 0f, 1f );
				myTarget.adrenaline = EditorGUILayout.FloatField( "Adrenaline: ", myTarget.adrenaline );
				myTarget.fightOrFlight = EditorGUILayout.Slider( "Fight Or Flight Reaction: ", myTarget.fightOrFlight, 0f, 1f );
				myTarget.emotionEffectFactor = EditorGUILayout.Slider( "Emotional Effect Factor: ", myTarget.emotionEffectFactor, 0f, 5f );
				myTarget.inPanic = EditorGUILayout.Toggle( "Currently Panicking: ", myTarget.inPanic );
				
				EditorGUILayout.Space();
			}
			
			
			//foldout: engagement scripts
			foldouts[5] = EditorGUILayout.Foldout( foldouts[5], "Engagement Scripts" );
			
			if( foldouts[5] == true )
			{
				//draw each engagement script
				for( int x = 0; x < myTarget.engagementScripts.Count; x++ )
				{
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUILayout.Space();
						
					myTarget.engagementScripts[x].nameOfScript = EditorGUILayout.TextField( "Name Of Engagement Script: ", myTarget.engagementScripts[x].nameOfScript);
					myTarget.engagementScripts[x].chanceOfActivating = EditorGUILayout.Slider( "Chance Of Activatng: ", myTarget.engagementScripts[x].chanceOfActivating, 0f, 1f );
					
					if(GUILayout.Button("Delete"))
					{
						myTarget.engagementScripts.RemoveAt(x);
					}
				}
				
				
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				
				//draw button to add more
				if( GUILayout.Button("Add") )
				{
					myTarget.engagementScripts.Add( new EngagementScript() );
				}
			}
			
			
			EditorGUILayout.Space();
		}
		
		
	}
	
}