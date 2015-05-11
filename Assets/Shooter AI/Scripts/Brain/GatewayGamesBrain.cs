using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace GatewayGames.ShooterAI
{
	
	/// <summary>
	/// The AI's state.
	/// </summary>
	public enum CurrentState { Patrol, Investigate, Chase, Engage };
	
	
	/// <summary>
	/// The AI's engagement status.
	/// </summary>
	public enum EngageState { ClearEngage, HalfCoverEngage, Cover, Strafe, Melee };


	/// <summary>
	/// Contains data about different speeds of the AI.
	/// </summary>
	[System.Serializable]
	public class AISpeed
	{
		/// <summary>
		/// Speed during engagement.
		/// </summary>
		public float engageSpeed = 4f;

		/// <summary>
		/// The default speed.
		/// </summary>
		public float defaultSpeed = 3.5f;

		/// <summary>
		/// The chase speed.
		/// </summary>
		public float chaseSpeed = 4.5f;
	}
	
	
	/// <summary>
	/// Describes each engagement script.
	/// </summary>
	[System.Serializable]
	public class EngagementScript
	{
		/// <summary>
		/// The name of script.
		/// </summary>
		public string nameOfScript;
		
		/// <summary>
		/// The chance of this script activating.
		/// </summary>
		[Range(0f, 1f)]
		public float chanceOfActivating;
	}
	
	
	
	
	
	/// <summary>
	/// The AI's brain.
	/// </summary>
	public class GatewayGamesBrain : MonoBehaviour
	{
		
		//<----- PUBLIC VARS --------->
		
		//reference variables
		public GatewayGamesEars ears; //the ears object
		public GatewayGamesEyes eyes; //the eyes object
		public GatewayGamesModelManager modelManager; //the model manager
		public GatewayGamesPatrolManager patrolManager; //the patrol manager
		public GatewayGamesHealthManager healthManager; //the health manager


		//stats about this ai
		public CurrentState currentState = CurrentState.Patrol; //the current state of the ai
		public EngageState engagementState = EngageState.ClearEngage; //the current engagement state of the ai
		public bool canEngage = true; //whether this AI can engage or not
		public bool allowCrouching = true; //whether this ai can crouch or not
		public bool crouching = false; //whether this ai is crouching or not
		public AISpeed aiSpeeds = new AISpeed(); //data about speeds
		
		//communication with other enemies
		public bool canCallToOthers = true; //whether this AI can call to other ones for help
		public float callRadius = 30f; //radius from which this AI can call others to help
		public float timeForReinforcementsToAttack = 15f; //how long this AI will actually support the stated enemy before it switches back to its own searching algorithm
		
		//enemy variables
		public GameObject currentEnemy = null; //the current enemy, if we have one
		public Vector3 enemyTargetArea = Vector3.up; //the target to aim at
		public Vector3 enemyTargetRandomOffset = Vector3.zero; //the offset of the aim due to factors such as adrenaline etc...
		public Vector3 lastHeardEnemyLocation = Vector3.zero; //the last heard location of the enemy
		public Vector3 lastHeardBulletLocation = Vector3.zero; //the last heard location of a bullet
		public Vector3 lastSeenEnemyLocation = Vector3.zero; //the last seen enemy location
		public string tagOfEnemy = null; //the tag of the enemies
		public string tagOfBullet = null; //the tag of the bullets
		public bool searchForNewAI = true; //whether to be searching for new AI automatically or not
		public float minDistanceToEnemy = 3f; //the min distance to enemies
		
		
		//emotion variables
		[Range( 0f, 1f)]
		public float fear = 0f; //the amount of fear that this AI is experiencing
		public float adrenaline = 0f; //the amount of adrenaline that this AI has
		[Range(0f, 1f)]
		public float fightOrFlight = 0.5f; //the chance that this AI will go into fight rather than flight
		[Range( 0f, 5f )]
		public float emotionEffectFactor = 1f; //the factor with which emotions effect the AIs state
		public bool inPanic = false; //whether the AI is in panic right now

		//engagement parameters
		public List<EngagementScript> engagementScripts = new List<EngagementScript>();
		
		
		
		
		//<----- PRIVATE VARS -------->
		
		private Collider[] objectAtLocationColliderVars; //this is used often in the ObjectAtLocation method, to record which colliders are within radius
		private float objectAtLocationCheckRadius = 0.3f; //this is used often in the ObjectAtLocation method, to determine what to check
		private bool objectAtLocationResult = false; //this is used often in the ObjectAtLocation method, to store the result
		
		//temp values
		private float tempFloat; //a temp float that is used often
		private string tempString; //a temp string that is used often
		private int tempInt; //a temp int that is used often
		private Vector3 tempVector3; //a temp vector3 that is used often
		private bool tempBool; //a temp bool that is used often
		private GatewayGamesBrain tempBrain; //a temp brain that is used often
		
		//state vars
		private int investigateTicks = 0; //this is to count how many ticks the ai has been in investigate mode
		private int investigateTicksToBreakOff = 500; //how many ticks without hearing anything before going from investigate to patrol
		private float engagementTillChase = 2.5f; //how many seconds to wait to go from engagement to chase
		private float engagementStateTime = 0f; //how much time is left in this engagement sub-state; designed so that the AI doesn't change its tactics mid-attack
		private float engagementTimePerState = 4f; //how much time in seconds is given for each decision in the engagement sub-state
		private float crouchFactor = 0.5f; //by how much to modify scripts to account for crouching
		private float actualSpeed = 3.5f; //the actual speed of the AI
		private float emotionSpeedModifier = 0f; //how much emotions modify the speed
		private float initFear = 0f; //the initial fear of the AI
		private float initFOV = 150f; //the initial FOV
		private float initFOF = 0.5f; //the initial fight or flight state
		private float initHealthManagerYScale = 1f; //the initial Y scale of the health manager; used for crouching
		private float minSecondsAtCover = 2.5f; //the amount of time the AI should stay at cover
		private Vector3 prevLastHeardBulletPos; //this is to recognize change in bullets
		
		//weapons extra data
		List<GatewayGamesWeapon> weapons = new List<GatewayGamesWeapon>(); //list of the current weapons
		
		//optimization
		private int currentTick = 0;
		private int defaultMaxTick = 60; //once how many frames to update heavy vars
		private int tickDeviation = 20; //the random factor for best optimization
		private int aCurrentTick = 0; //advanced optimization ticks; used for even more optimization for methods than should be called every ~10 seconds
		private int aDefaultMaxTick = 10; //advanced default max ticks
		
		//caches
		public GatewayGamesMovementContoller movement; //cache to movement manager
		private GatewayGamesWeaponManager weaponsManager; //cache to weapons manager
		private GatewayGamesSearchCover coverSystem; //cache to cover system
		
		
		
		
		//<----------------------------------------- MAIN FUNCTIONS ---------------------------------------------------------------------------->
		
		
		void Awake()
		{
			//set caches
			movement = GetComponent<GatewayGamesMovementContoller>();
			weaponsManager = GetComponent<GatewayGamesWeaponManager>();
			coverSystem = GetComponent<GatewayGamesSearchCover>();
			
			//set positions
			ears.transform.position = modelManager.GetComponent<Animator>().GetBoneTransform( HumanBodyBones.Head ).position;

			//set emotions
			initFear = fear;
			adrenaline += 0.3f;
			initFOF = fightOrFlight;

			//set sensors
			initFOV = eyes.fieldOfView;

			//set health manager
			initHealthManagerYScale = healthManager.transform.localScale.y;
			
			//set ticks
			currentTick += 1;
			defaultMaxTick += Random.Range( -tickDeviation, tickDeviation);
		}
		
		
		void Update()
		{
			//find errors before they start
			if( tagOfBullet == "" || tagOfEnemy == "" )
			{
				return;
			}
			
			//apply the ticks in all needed scripts so that they reapply their vars; useful for optimization
			Tick();
			ears.Tick();
			eyes.Tick();
			
			//update heavy variables once every n number of frames
			currentTick += 1;
			if(currentTick >= defaultMaxTick)
			{
				currentTick = 0;
				VariablesUpdatedOptimized();
			}
		}
		
		
		
		/// <summary>
		/// Tick this instance. Happens once per frame by default.
		/// </summary>
		public virtual void Tick()
		{
			//apply variables
			coverSystem.avoidGameobject = currentEnemy;
			
			//only apply actions if the ai is in normal condition
			if( healthManager.healthState == HealthState.Normal)
			{
				//control the state of the ai
				ControlState();
				ControlEmotions();
				ControlCrouching();
				ControlSpeed();

				//switch and apply the state
				switch( currentState )
				{
				case CurrentState.Patrol: StatePatrol(); break;
				case CurrentState.Investigate: StateInvestigation(); break;
				case CurrentState.Chase: StateChase(); break;
				case CurrentState.Engage: StateEngage(); break;
				}

			}
			
			//apply prev vars
			prevLastHeardBulletPos = lastHeardBulletLocation;
			
		}

		
		/// <summary>
		/// In this method, only those variables are updated which take up a lot of CPU, but don't always need to be updated (Heavy Variables).
		/// </summary>
		void VariablesUpdatedOptimized()
		{
			//update cover
			coverSystem.FindCoverPostion( eyes.transform.localPosition, false );
			
			//update whether we can call others
			//communicate with others to call in reinforcements
			if(currentState == CurrentState.Engage && canCallToOthers == true)
			{
				//get all our team mates
				GameObject[] teamMates = GameObject.FindGameObjectsWithTag( gameObject.tag);
				
				//check each one for closeness and readiness
				foreach(GameObject teamMate in teamMates)
				{
					tempFloat = Vector3.Distance( transform.position, teamMate.transform.position );
					if(tempFloat <= callRadius)
					{
						tempBrain = teamMate.GetComponent<GatewayGamesBrain>();
						if(tempBrain != null && (tempBrain.currentState == CurrentState.Patrol || tempBrain.currentState == CurrentState.Investigate))
						{
							tempBrain.AttackEnemy( currentEnemy, lastSeenEnemyLocation );
						}				
					}
				}
			}
			
			//optimize half cover even more
			aCurrentTick += 1;
			if(aCurrentTick > aDefaultMaxTick)
			{
				aCurrentTick = 0;
				//try getting half cover
				if(currentState == CurrentState.Engage)
				{
					coverSystem.FindCoverPostion( eyes.transform.localPosition, true);
				}
			}
			
			
		}
		
		
		
		
		#region Control State
		
		/// <summary>
		/// Controls the state of this AI, and the transitions. Strategic part of brain.
		/// </summary>
		public virtual void ControlState()
		{
			
			
			//<------------------- PATROL --------------------------------->
			
			
			//try investigate mode from patrol, and we can see/hear the enemy
			if( currentState == CurrentState.Patrol && ( ears.canHearEnemy == true || eyes.canSeeEnemy == true || lastHeardBulletLocation != prevLastHeardBulletPos) && canEngage == true)
			{
				//go into investigate
				currentState = CurrentState.Investigate;
			}
			

			
			
			//<------------------- INVESTIGATE ---------------------------->
			
			
			//if we're in investigate/chase and can see the enemy, activate and egagement script
			if( currentState == CurrentState.Investigate && ( ears.canHearEnemy == true || eyes.canSeeEnemy == true))
			{
				//activate engagement script
				ActivateEngagementScript();
				
				//reset vars
				investigateTicks = 0;
			}
			
			
			
			//test if we should go back to patrol
			if(currentState == CurrentState.Investigate)
			{
				investigateTicks += 1;
				if( investigateTicks >= investigateTicksToBreakOff)
				{
					//go back to patrol if we don't hear anything for a long time
					currentState = CurrentState.Patrol;
					investigateTicks = 0;
					lastHeardEnemyLocation = Vector3.zero;
				}
			}
			
			
			
			
			
			
			//<----------------------------- ENGAGEMENT ------------------------>
			
			
			//test if we can actually see the enemy, or else go into chase
			if( currentState == CurrentState.Engage && eyes.canSeeEnemy == false )
			{
				//wait, then go into chase
				StartCoroutine( "TestEngagementAfterTime", engagementTillChase );
			}
			if( currentState == CurrentState.Engage && eyes.canSeeEnemy == true )
			{
				//break off all unnecasry coroutines
				StopCoroutine( "TestEngagementAfterTime" );
			}
			
			
			
			
			//<---------------------------- CHASE -------------------------------->
			
			
			//test if we need to run to the last place where we heard/saw the player, else go into investigate
			if(currentState == CurrentState.Chase)
			{
				//check if we're already moving
				if(movement.movementData.enRouteToDestination == false)
				{
					//check if we heard or saw the enemy
					if( lastHeardEnemyLocation != Vector3.zero || lastSeenEnemyLocation != Vector3.zero || lastHeardBulletLocation != prevLastHeardBulletPos )
					{
						//first try going to the last seen location
						if( lastSeenEnemyLocation != Vector3.zero )
						{
							movement.SetNewDestination( lastSeenEnemyLocation );
						}
						else
						{
							//if we've only heard the enemy
							if( lastHeardEnemyLocation != Vector3.zero )
							{
								movement.SetNewDestination( lastHeardEnemyLocation);
							}
							else
							{
								//if we've only heard a bullet
								movement.SetNewDestination( lastHeardBulletLocation );
							}
						}
					}
					else
					{
						//else go into investigation
						currentState = CurrentState.Investigate;
					}
					
				}
				
				//go into engagement if we can see the enemy
				if(eyes.canSeeEnemy == true)
				{
					//activate engagement script
					ActivateEngagementScript();
				}

				//watch out for hearing the enemy
				if(ears.canHearEnemy == true)
				{
					if( lastHeardEnemyLocation != Vector3.zero )
					{
						movement.SetNewDestination( lastHeardEnemyLocation);
					}
				}
				
			}
			
			
			
		}
		
		
		
		#endregion
		
		
		
		
		#region StateManagement
		
		
		
		
		/// <summary>
		/// Controls the Patrol state in the AI.
		/// </summary>
		public virtual void StatePatrol()
		{
			//move to the next waypoint, if we're not already going there
			if( Vector3.Distance( movement.movementData.position, movement.movementData.destination ) < movement.minDistanceToDestination )
			{
				movement.SetNewDestination( patrolManager.nextDestination );
			}
		}
		
		
		
		/// <summary>
		/// Controls the Investigation state in the AI.
		/// </summary>
		public virtual void StateInvestigation()
		{
		
			//if we're in investigate, but can't see/hear the enemy, go to the last heard/seen location
			if( ears.canHearEnemy == false && eyes.canSeeEnemy == false && 
			   ( lastHeardEnemyLocation != Vector3.zero || lastSeenEnemyLocation != Vector3.zero || lastHeardBulletLocation != prevLastHeardBulletPos) )
			{
				
				//first try going to the last seen location
				if( lastSeenEnemyLocation != Vector3.zero )
				{
					movement.SetNewDestination( lastSeenEnemyLocation );
				}
				else
				{
					//if we've only heard the enemy
					if( lastHeardEnemyLocation != Vector3.zero )
					{
						movement.SetNewDestination( lastHeardEnemyLocation);
					}
					else
					{
						//if we've only heard a bullet
						movement.SetNewDestination( lastHeardBulletLocation );
					}
				}
				
				//reset vars
				investigateTicks = 0;
			}
			
			//if we can see the place where we're moving, stop and watch
			if( eyes.CanSeePosition( movement.movementData.destination) == true )
			{
				movement.SetNewDestination( transform.position );
			}
			
			
		}
		
		
		
		/// <summary>
		/// Controls the Chase state in the AI. NOTE: Main Chase logic is the ControlState loop, due to how its integral for state management.
		/// </summary>
		public virtual void StateChase()
		{
			
			
		}
		
		
		
		/// <summary>
		/// Controls the full Engage state in the AI.
		/// </summary>
		public virtual void StateEngage()
		{
			//check if we even have an enemy
			if(currentEnemy == null)
			{
				return;
			}

			//apply management logic
			StateEngageManagement();
			
			//apply executive logic
			StateEngageExecutiveLogic();
				
		}
		
		
		
		
		/// <summary>
		/// Handles the sub-state engagement management.
		/// </summary>
		public virtual void StateEngageManagement()
		{
			
			//apply variables for state deciding
			engagementStateTime -= Time.deltaTime;
			
			
			//decide between melee and ranged logic
			if( weaponsManager.currentWeaponFiretype == FireType.Ranged )
			{
				//ranged logic
				
				//can we see the enemy -> (True) Fight or flight reaction -> (True) Clear attack / (False) Cover
				// -> (False) Fight or flight reaction -> (True) Try strafe or half cover / (False) Cover
				
				
				//if the engagement time is up, it means that we can change to a different engagement sub-state
				if(engagementStateTime <= 0f)
				{
					//based on whether we can see the enemy or not, we will choose our tactics
					if( eyes.canSeeEnemy == true)
					{
						//try fight or flight
						if( FightOrFlight() == true)
						{
							//do clear attack
							engagementState = EngageState.ClearEngage;
							//apply time to do that state
							engagementStateTime = engagementTimePerState;
						}
						else
						{
							//do cover
							engagementState = EngageState.Cover;
							//apply time to do that state
							engagementStateTime = engagementTimePerState;
						}
						
					}
					else
					{
						//try fight or flight
						if( FightOrFlight() == true)
						{
							//try strafe, or else go half-cover attack
							if( TryStrafe() == true )
							{
								//apply strafe
								engagementState = EngageState.Strafe;
								//apply time to do that state
								engagementStateTime = engagementTimePerState;
							}
							else
							{
								//else go to half-cover
								movement.SetNewDestination( coverSystem.halfCoverPosition );
								//apply cover
								engagementState = EngageState.HalfCoverEngage;
								//apply time to do that state
								engagementStateTime = engagementTimePerState;
							}
							
						}
						else
						{
							//do cover
							engagementState = EngageState.Cover;
							//apply time to do that state
							engagementStateTime = engagementTimePerState;
						}
					}
				}
				
			}
			else
			{
				//melee logic
				
				//apply melee
				engagementState = EngageState.Melee;
				
			}



			//if the enemy is really close, go automatically into fight and turn towards him
			if( Vector3.Distance( transform.position, currentEnemy.transform.position ) < movement.minDistanceToDestination * 3f && 
			   Vector2.Angle( new Vector2( transform.forward.x, transform.forward.z), new Vector2( currentEnemy.transform.position.x, currentEnemy.transform.position.z) - new Vector2( transform.position.x, transform.position.z) ) >= 90f)
			{

				//face enemy in the correct direction
				if( transform.InverseTransformPoint( currentEnemy.transform.position).z > 0f )
				{
					//right
					movement.FaceTowards( transform.eulerAngles.y + Vector2.Angle( new Vector2( transform.forward.x, transform.forward.z), 
					                                                              new Vector2( currentEnemy.transform.position.x, currentEnemy.transform.position.z) - new Vector2( transform.position.x, transform.position.z) ) );
				}
				else
				{
					//left
					movement.FaceTowards( transform.eulerAngles.y - Vector2.Angle( new Vector2( transform.forward.x, transform.forward.z), 
					                                                              new Vector2( currentEnemy.transform.position.x, currentEnemy.transform.position.z) - new Vector2( transform.position.x, transform.position.z) ) );
				}

				//go into correct fight sequence based on weapon
				if( weaponsManager.currentWeaponFiretype == FireType.Melee )
				{
					engagementState = EngageState.ClearEngage;
				}
				else
				{
					engagementState = EngageState.Melee;
				}
			}
			
			
			//if we can fight the enemy, but aren't in range, run towards it
			if(engagementState == EngageState.ClearEngage && movement.movementData.enRouteToDestination == false 
			   && Vector3.Distance(transform.position, currentEnemy.transform.position) > weaponsManager.currentWeaponMaxDistance)
			{
				movement.SetNewDestination( currentEnemy.transform.position);
				
				//try switching to a better weapon based on their ranges
				if(weaponsManager.multipleWeapons.Count > 0)
				{
					TrySwitchingToBetterWeapon();
				}
				
			}
			
			
			//if we're too close to the enemy, try finding a new fighting pos
			if(weaponsManager.currentWeaponFiretype == FireType.Ranged && currentState == CurrentState.Engage && 
			   movement.movementData.enRouteToDestination == false && Vector3.Distance(transform.position, currentEnemy.transform.position) <= minDistanceToEnemy)
			{
				movement.SetNewDestination( FindNewFightingPosition() );
			}
			
		
			
		}
		
		
		
		/// <summary>
		/// Handles the execution of engagement.
		/// </summary>
		public virtual void StateEngageExecutiveLogic()
		{
			//fire if we can see the enemy and are within range
			if( engagementState != EngageState.Melee && eyes.canSeeEnemy == true && weaponsManager.CanSeeEnemyFromWeapon() == true 
			   && Vector3.Distance(transform.position, currentEnemy.transform.position) <= weaponsManager.currentWeaponMaxDistance)
			{
				
				TryFire();
			}
			
			
			//if we're in cover mode and still have a lot of time left, go to full cover
			if( engagementState == EngageState.Cover && engagementStateTime > (engagementTimePerState - 0.1f))
			{	
				//go to cover
				movement.SetNewDestination( coverSystem.coverPostion );
				
				//set correct time
				engagementStateTime = Mathf.Min( (movement.CalculateLengthToPosition( movement.movementData.destination ) / movement.movementData.forwardSpeed), engagementTimePerState * 2 ) + engagementTimePerState + minSecondsAtCover;
			}
			
			
			//if we can see the enemy and we're moving whilst in clear engage, stop
			if( engagementState == EngageState.ClearEngage && movement.movementData.enRouteToDestination == true && eyes.canSeeEnemy == true && weaponsManager.CanSeeEnemyFromWeapon() == true )
			{
				movement.SetNewDestination( transform.position);
			}


			//if in clear engage, always face the enemy
			if( engagementState == EngageState.ClearEngage && Vector2.Angle( new Vector2( transform.forward.x, transform.forward.z), new Vector2( currentEnemy.transform.position.x, currentEnemy.transform.position.z) - new Vector2( transform.position.x, transform.position.z) ) > 15f )
			{
				//face enemy in the correct direction
				if( transform.InverseTransformPoint( currentEnemy.transform.position).x > 0f )
				{
					//right
					movement.FaceTowards( transform.eulerAngles.y + Vector2.Angle( new Vector2( transform.forward.x, transform.forward.z), 
					                                                              new Vector2( currentEnemy.transform.position.x, currentEnemy.transform.position.z) - new Vector2( transform.position.x, transform.position.z) ) );
				}
				else
				{
					//left
					movement.FaceTowards( transform.eulerAngles.y - Vector2.Angle( new Vector2( transform.forward.x, transform.forward.z), 
					                                                              new Vector2( currentEnemy.transform.position.x, currentEnemy.transform.position.z) - new Vector2( transform.position.x, transform.position.z) ) );
				}
			}


			//do melee fight logic
			if( engagementState == EngageState.Melee )
			{
				
				//always move to the enemies last known location
				if( lastSeenEnemyLocation != Vector3.zero)
				{
					movement.SetNewDestination( lastSeenEnemyLocation, weaponsManager.meleeAttackDistance );
				}
				else
				{
					if(lastHeardEnemyLocation != Vector3.zero)
					{
						movement.SetNewDestination( lastHeardEnemyLocation, weaponsManager.meleeAttackDistance);
					}
				}
				
				
				//try attacking if we can see the enemy and we're close enough
				if(eyes.canSeeEnemy == true
				&& Vector2.Distance( new Vector2( transform.position.x, transform.position.z), 
				                    new Vector2(currentEnemy.transform.position.x, currentEnemy.transform.position.z) ) <= weaponsManager.meleeAttackDistance * 1.3f 
				   && Mathf.Abs(currentEnemy.transform.position.y - transform.position.y) <= weaponsManager.meleeAttackDistance * 2f)
				{
					TryFire();
				}
			}
			
			
			//throw secondary weapon if needed
			if( engagementState != EngageState.Melee && (Time.time - weaponsManager.secondaryWeaponData.lastThrowTime) > 30f * Random.Range( 1f, 2f) 
			   && Vector3.Distance( transform.position, currentEnemy.transform.position) <= weaponsManager.secondaryWeaponData.triggerDistance )
			{
				weaponsManager.FireSecondaryWeapon();
			}
			
		}
		
		
		
		
		#endregion
		
		
		
		
		/// <summary>
		/// Controls the emotions of the AI, and applies their effects
		/// </summary>
		public virtual void ControlEmotions()
		{

			//apply extra adrenaline if we can see/hear the enemy
			if(eyes.canSeeEnemy == true || ears.canHearEnemy == true)
			{
				adrenaline = adrenaline * 1.01f + 0.06f;
			}

			//apply fight or flight offset
			fightOrFlight =  initFOF + (adrenaline/15f) - (fear * 20f);


			//decrease fear and adrenaline
			adrenaline = adrenaline - 0.05f * ( (adrenaline * adrenaline)/25f );
			fear -= 0.01f;

			//max the emotions
			fightOrFlight = Mathf.Clamp( fightOrFlight, 0f, 1f);
			fear = Mathf.Clamp( fear, 0f, 1f);
			if(adrenaline <= 0f)
			{
				adrenaline = 0f;
			}


			//apply aim offset
			Vector3 enemyTargetRandomOffsetT = 0.1f * emotionEffectFactor * Random.insideUnitSphere * (adrenaline + 1f) * (fear + 1f);
			enemyTargetRandomOffset = Vector3.Lerp(enemyTargetRandomOffset, enemyTargetRandomOffsetT, Time.deltaTime * 3f);

			//apply FOV modifications
			eyes.fieldOfView =  initFOV * ( adrenaline - (fear * 20f) + 1f);
			eyes.fieldOfView = Mathf.Clamp( eyes.fieldOfView, 60f, initFOV + 30f );

			//apply speed modifications
			emotionSpeedModifier = (adrenaline/10f) - (fear * 10f);

			//induce panic when over 80% fear
			if( fear >= 0.8f )
			{
				//apply var
				inPanic = true;
				
				
				
				//try firing non-stop
				TryFire();

				//aim at random location
				enemyTargetArea = fear * 100f * Random.insideUnitSphere;

				//stay put
				movement.SetNewDestination( transform.position);
			}
			else
			{
				//reset to normal
				inPanic = false;
			}

		}


		
		
		
		
		
		#region HelperFunctions


		//<---------------------------------------- HELPER FUNCTIONS ---------------------------------------------------------------------------->
		
		
		/// <summary>
		/// Finds the new fighting position which isn't too far or too close to the enemy.
		/// </summary>
		/// <returns>The new fighting position.</returns>
		private Vector3 FindNewFightingPosition()
		{
			//init search
			Vector3 attempNewPos = transform.position;
			
			//find new pos via raycasts + random
			for(int x = 0; x < 15; x++)
			{
				attempNewPos = transform.position + (Random.insideUnitSphere * ( 0.2f * weaponsManager.currentWeaponMaxDistance));
				
				if(Vector3.Distance(attempNewPos, currentEnemy.transform.position) > minDistanceToEnemy && !Physics.Raycast(transform.position + Vector3.up, ( (attempNewPos + Vector3.up) - (transform.position + Vector3.up) )))
				{
					break;
				}
			}
			
			//new position to go
			return attempNewPos;
		}
		
		
		
		/// <summary>
		/// Tries to switch to a better weapon.
		/// </summary>
		private void TrySwitchingToBetterWeapon()
		{
			//first create and fill the weapons list
			for(int x = 0; x < weaponsManager.multipleWeapons.Count; x++)
			{
				GatewayGamesWeapon tempWeapon = weaponsManager.multipleWeapons[x].weaponObject.GetComponent<GatewayGamesWeapon>();
				
				if(tempWeapon != null && tempWeapon.amountOfMagazines != 0 && tempWeapon.ammoInMagazine != 0)
				{
					weapons.Add(tempWeapon);
				}			
			}
			
			tempFloat = 0f;
			tempInt = -1;
			float dist = Vector3.Distance(transform.position, currentEnemy.transform.position);
			
			//then sort it based on their range to the actual distance
			for(int x = 0; x < weapons.Count; x++)
			{
				if( Mathf.Abs(dist - weapons[x].maxDistanceToShoot) < tempFloat)
				{
					tempFloat = Mathf.Abs(dist - weapons[x].maxDistanceToShoot);
					tempInt = x;
				}
			}
			
			//then switch weapons to the one that suits the most
			if(tempInt != -1)
			{
				weaponsManager.ChangeWeapon( weapons[tempInt].transform);
			}
			
		}
		

		/// <summary>
		/// Controls the speed.
		/// </summary>
		private void ControlSpeed()
		{
			//get by state the correct speed
			switch(currentState)
			{
			case CurrentState.Engage: actualSpeed = aiSpeeds.engageSpeed; break;
			case CurrentState.Chase: actualSpeed = aiSpeeds.chaseSpeed; break;
			default: actualSpeed = aiSpeeds.defaultSpeed; break;
			}


			//apply emotion factor
			actualSpeed += emotionSpeedModifier;

			//set the speeds
			movement.SetSpeed( actualSpeed);
		}



		/// <summary>
		/// Controls crouching.
		/// </summary>
		private void ControlCrouching()
		{

			//set crouching when needed
			if( allowCrouching == true)
			{
				//check in cover/hald cover engage
				if(engagementState == EngageState.Cover || engagementState == EngageState.HalfCoverEngage)
				{
					crouching = true;
				}
				else
				{
					crouching = false;
				}
				
				//check in normal engage whether we see the enemy if we croch, and if we can, then crouch
				if(currentState == CurrentState.Engage && eyes.CanSeeEnemy(currentEnemy, new Vector3( eyes.transform.position.x, (eyes.transform.position.y - transform.position.y) * crouchFactor, eyes.transform.position.z )))
				{
					crouching = true;
				}
				else
				{
					crouching = false;
				}
				
			}


			//check and set crouching
			if(crouching == true)
			{
				//check if we're still standing whilst we should be crouching
				if(healthManager.transform.localScale.y > initHealthManagerYScale * crouchFactor + 0.1f)
				{
					healthManager.transform.localScale = Vector3.Lerp( healthManager.transform.localScale,
					                                                  new Vector3( healthManager.transform.localScale.x, 
					            initHealthManagerYScale * crouchFactor,
					            healthManager.transform.localScale.z), 
					                                                  0.1f);
				}

			}
			else
			{
				//check if we're still crouching when we should be standing
				if(healthManager.transform.localScale.y < initHealthManagerYScale - 0.1f)
				{
					healthManager.transform.localScale = Vector3.Lerp( healthManager.transform.localScale,
					                                                  new Vector3( healthManager.transform.localScale.x, 
					            initHealthManagerYScale,
					            healthManager.transform.localScale.z), 
					                                                  0.1f);
				}
			}

		}




		/// <summary>
		/// Messgae used from the health manager that we're hit.
		/// </summary>
		public void AIHit()
		{
			//spike addrenaline
			adrenaline = adrenaline * 1.3f;

			//spike fear
			fear += initFear * 0.5f;
		}

		
		/// <summary>
		/// Reloads the weapon and applies correct vars.
		/// </summary>
		public virtual void ReloadWeapon()
		{
			//apply reloading
			weaponsManager.ReloadWeapon();
		}
		
		
		
		/// <summary>
		/// Attempts to fire/swing the main weapon.
		/// </summary>
		public void TryFire()
		{
			//calculate final aim position
			if( currentEnemy != null )
			{
				tempVector3 = currentEnemy.transform.position + enemyTargetArea + enemyTargetRandomOffset;
			}
			else
			{
				tempVector3 = enemyTargetArea + enemyTargetRandomOffset;
			}
			
			//aim
			weaponsManager.AimAtPosition( tempVector3);
			
			//fire
			weaponsManager.Fire();
		}
		
		
		
		/// <summary>
		/// Tries to strafe attack the enemy. Returns true if the method was applied to the AI.
		/// </summary>
		/// <returns><c>true</c>, if strafe was applied, <c>false</c> otherwise false.</returns>
		public bool TryStrafe()
		{
			//temp bool will serve as the result var
			tempBool = false;
			
			//calculate the potential target position
			tempVector3 = currentEnemy.transform.position - new Vector3( currentEnemy.transform.position.x, 
			                                                            eyes.transform.position.y, 
			                                                            ( transform.position.z - currentEnemy.transform.position.z ) );
			
			//calculate whether this location is close enough that we don't switch state mid-strafe
			if( (Vector3.Distance( tempVector3, eyes.transform.position)/movement.movementData.forwardSpeed) < (engagementTimePerState + 0.5f) )
			{
				//do a physics check whether that point is free
				if( !Physics.Raycast( tempVector3, ( currentEnemy.transform.position - tempVector3 ) ) )
				{
					//if not hit response, apply strafe
					
					//apply correct return status
					tempBool = true;
					
					//apply correct movement
					movement.StrafeToDestination( tempVector3, Vector3.Angle( -tempVector3, transform.eulerAngles ) );
				}
				
			}
			
			
			//return the temp bool
			return tempBool;
		}
		
		
		
		
		/// <summary>
		/// Chooses whether the AI should fight (true) or flight (false).
		/// </summary>
		/// <returns><c>true</c>, if the AI should fight, <c>false</c> otherwise flee.</returns>
		private bool FightOrFlight()
		{
			//get random value
			tempFloat = Random.value;
			
			//check if its smaller that the fight or flight chance
			if(tempFloat <= fightOrFlight)
			{
				return true;
			}
			else
			{
				return false;
			}
			
		}
		
		
		
		
		/// <summary>
		/// Engages the enemy instantly.
		/// </summary>
		public void EngageEnemy()
		{
			//go into engagement
			currentState = CurrentState.Engage;
			
		}
		
		/// <summary>
		/// Makes the AI stand down.
		/// </summary>
		public void StandDown()
		{
			//go into patrol
			currentState = CurrentState.Patrol;
			
		}
		
		
		/// <summary>
		/// Activates randomly an engagement script.
		/// </summary>
		private void ActivateEngagementScript()
		{
			
			//cycle and check randomly each script
			for(int x = 0; x < engagementScripts.Count; x++)
			{
				//find a random value
				tempFloat = Random.value;
				
				//check if its smaller than the given
				if( tempFloat < engagementScripts[x].chanceOfActivating)
				{
					//create the new script
					UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent( gameObject, "Assets/Shooter AI/Scripts/Brain/GatewayGamesBrain.cs (1167,6)", engagementScripts[x].nameOfScript );
					
					//break off the loop
					break;
				}
				
				//reset if needed for multiple loops
				if( x >= engagementScripts.Count)
				{
					x = 0;
				}
			}
			
			
		}
		
		
		
		/// <summary>
		/// Checks whether the provided object is a the location
		/// </summary>
		/// <returns><c>true</c>, if at location was objected, <c>false</c> otherwise.</returns>
		/// <param name="objectToCheck">Object to check.</param>
		/// <param name="posToCheck">Position to check.</param>
		public bool ObjectAtLocation( Transform objectToCheck, Vector3 posToCheck )
		{
			//reset vars
			objectAtLocationResult = false;
			
			//get colliders at that point
			objectAtLocationColliderVars = Physics.OverlapSphere( posToCheck, objectAtLocationCheckRadius);
			
			//cycle through and check if they're part of the object to check
			for(int x = 0; x < objectAtLocationColliderVars.Length; x++)
			{
				
				//see if this is the object that we're checking, or a child of it
				if( objectAtLocationColliderVars[x].transform == objectToCheck || objectAtLocationColliderVars[x].transform.IsChildOf(objectToCheck) )
				{
					//set vars correctly and break off cycle
					objectAtLocationResult = true;
					break;
					
				}
			}
			
			//return result
			return objectAtLocationResult;
			
		}
		
		
		
		
		/// <summary>
		/// Checks whether the provided object is a the location
		/// </summary>
		/// <returns><c>true</c>, if at location was objected, <c>false</c> otherwise.</returns>
		/// <param name="objectToCheck">Object to check.</param>
		/// <param name="posToCheck">Position to check.</param>
		/// <param name="radiusToCheck">Radius to check.</param>
		public bool ObjectAtLocation( Transform objectToCheck, Vector3 posToCheck, float radiusToCheck )
		{
			//reset vars
			objectAtLocationResult = false;
			
			//get colliders at that point
			objectAtLocationColliderVars = Physics.OverlapSphere( posToCheck, radiusToCheck);
			
			//cycle through and check if they're part of the object to check
			for(int x = 0; x < objectAtLocationColliderVars.Length; x++)
			{
				
				//see if this is the object that we're checking, or a child of it
				if( objectAtLocationColliderVars[x].transform == objectToCheck || objectAtLocationColliderVars[x].transform.IsChildOf(objectToCheck) )
				{
					//set vars correctly and break off cycle
					objectAtLocationResult = true;
					break;
					
				}
			}
			
			//return result
			return objectAtLocationResult;
			
		}
		
		
		/// <summary>
		/// Tests engagement after time.
		/// </summary>
		/// <returns>The engagement after time.</returns>
		private IEnumerator TestEngagementAfterTime(float time)
		{
			yield return new WaitForSeconds(time);

			//reset vars
			engagementState = EngageState.ClearEngage;
			currentState = CurrentState.Chase;
		}
		
		
		
		/// <summary>
		/// Sets and attacks a new enemy.
		/// </summary>
		/// <param name="newEnemy">New enemy.</param>
		/// <param name="lastSeenLocation">Last seen location.</param>
		public void AttackEnemy( GameObject newEnemy, Vector3 lastSeenLocation )
		{
			//check for nulls
			if(newEnemy != null)
			{
				searchForNewAI = false;
				lastSeenEnemyLocation = lastSeenLocation;
				currentEnemy = newEnemy;
				currentState = CurrentState.Engage;
				
				//start reset
				StartCoroutine( ResetEnemySearching(timeForReinforcementsToAttack) );
			}
			
		}
		
		/// <summary>
		/// Sets and attacks a new enemy.
		/// </summary>
		/// <param name="newEnemy">New enemy.</param>
		public void AttackEnemy( GameObject newEnemy )
		{
			AttackEnemy( newEnemy, newEnemy.transform.position );
		}
		
		/// <summary>
		/// Resets the enemy searching. Used in 'AttackEnemy'.
		/// </summary>
		/// <returns>The enemy searching.</returns>
		/// <param name="time">Time.</param>
		private IEnumerator ResetEnemySearching(float time)
		{
			//wait
			yield return new WaitForSeconds(time);
			
			//reset
			searchForNewAI = true;
		}
		
		#endregion
		
	}
	
	
}