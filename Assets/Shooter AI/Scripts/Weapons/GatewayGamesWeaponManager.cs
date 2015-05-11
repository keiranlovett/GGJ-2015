using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace GatewayGames.ShooterAI
{
	
	/// <summary>
	/// Types of weapon.
	/// </summary>
	public enum FireType { Ranged, Melee };
	
	
	/// <summary>
	/// Contains data vital for multiple weapons.
	/// </summary>
	[System.Serializable]
	public class WeaponData
	{
		/// <summary>
		/// Reference to the weapon object
		/// </summary>
		public Transform weaponObject;
			
	}
	
	
	
	/// <summary>
	/// Data about the secondary, throwable weapon, e.g. grenade.
	/// </summary>
	[System.Serializable]
	public class SecondaryWeapon
	{
		/// <summary>
		/// the secondary weapon prefab that gets thrown, e.g. a grenade
		/// </summary>
		public Rigidbody secondaryWeaponPrefab = null;
		
		
		/// <summary>
		/// the amount of the secondary weapon.
		/// </summary>
		public int secondaryWeaponAmount = 3;
		
		
		/// <summary>
		/// the throw force for the secondary weapon
		/// </summary>
		public float throwForce = 100f;
		
		
		/// <summary>
		/// The throw vector.
		/// </summary>
		public Vector3 throwVector = Vector3.up + Vector3.forward;
		
		
		/// <summary>
		/// The trigger distance under which to throw the weapon.
		/// </summary>
		public float triggerDistance = 10f;
		
		
		/// <summary>
		/// The time when a secondary was last thrown.
		/// </summary>
		public float lastThrowTime = 0f;
	}
	
	
	/// <summary>
	/// IK targets.
	/// </summary>
	[System.Serializable]
	public class IKTargets
	{
		/// <summary>
		/// The left hand target.
		/// </summary>
		public Transform leftHandTarget;
		
		/// <summary>
		/// The right hand target.
		/// </summary>
		public Transform rightHandTarget;
		
	}
	
	
	
	/// <summary>
	/// The AI's weapon manager.
	/// </summary>
	public class GatewayGamesWeaponManager : MonoBehaviour 
	{
		
		
		
		
		public bool debug = false; //whether to debug or not
		public Vector3 aimTargetPos = Vector3.zero; //the target at which to aim
		public Transform currentHoldingWeapon = null; //the weapon we're currently holding
		public FireType currentWeaponFiretype = FireType.Ranged; //the firing type of the current weapon
		public IKTypes currentWeaponIK = IKTypes.BothHands; //the IK setting of the current weapon
		public float currentWeaponMaxDistance = 100f; //the max firing distance of this weapon
		[ Range( 0, 180f) ]
		public float maxWeaponAngle = 40f; //the max angle of the weapon until the AI turns as a whole
		public IKTargets currentIKTargets = new IKTargets(); //the IK targets
		public Transform defaultWeaponHoldingLocation = null, engageWeaponHoldingLocation; //the default parent under which to hold the gun
		public HumanBodyBones bodyPartHoldingReference = HumanBodyBones.RightHand; //the reference for where to hold the weapon
		[Range( 0f, 1f)]
		public float engageTime = 0.2f; //the speed with which the ai can aim its camera
		public List<WeaponData> multipleWeapons = new List<WeaponData>(); //the weapons that this ai can use; must be referenced to deactivated weapons inside the AI
		public SecondaryWeapon secondaryWeaponData = new SecondaryWeapon(); //the data about the secondary throwable weapon, e.g. grenade
		public float weaponAngle = 0f;
		
		[HideInInspector]
		public Transform IKBendGoalLeft;
		
		[HideInInspector]
		public float meleeAttackDistance = 0.5f; //the attack distance for the current melee weapon
		
		
	
		private GatewayGamesBrain brain; //brain reference
		private RaycastHit weaponSeeEnemyData; //used to determine whether the weapon can see the enemy
		private Vector3 aimTempPos; //the temp position of where we're aiming; used for learping
		
		
		public bool engageFactorSetExternally = false; //whether the engage factor should be set an external script
		
		
		void Awake()
		{
			//set caches correctly
			brain = GetComponent<GatewayGamesBrain>();
			
			//set variables
			LateUpdate();
		}
		
		
		void OnDestroy()
		{
			//apply last update before destruction (e.g. death)
			LateUpdate();
		}

		public Quaternion recoilOffset { get; private set; }
		public float engageFactor;
		public float arbitraryFactor { get; private set; }
		private float engageTarget = 0f;

		[HideInInspector] public Transform arbitraryHoldingLocation;
		private Transform lastArbitraryHoldingLocation;
		private float arbitraryVel;
		private float engageVel;

		private float SmoothDampSnap(float current, float target, ref float currentVelocity, float smoothTime, float snap) {
			float x = Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime);
			if (Mathf.Abs(x) < snap) return 0f;
			if (Mathf.Abs(x - 1f) < snap) return 1f;
			return x;
		}

		void LateUpdate()
		{
			
			
			//set variables correctly
			if(currentHoldingWeapon != null)
			{
				//get weapon angle, based on whether we're yielding the weapon or not
				weaponAngle = (brain.currentState == CurrentState.Engage || brain.currentState == CurrentState.Investigate ) ? 
					Vector3.Angle( transform.forward, currentHoldingWeapon.forward) : 0f;
				
				//set aiming correctly with smoothing
				if(aimTargetPos != Vector3.zero) // @todo what if the character IS supposed to be aiming at Vector3.zero?
				{
					//check if we need to turn, rotate the character before everything else
					if( weaponAngle > maxWeaponAngle && brain.movement.movementData.enRouteToDestination == false)
					{
						//turn towards where the weapon is facing
						Vector3 direction = aimTargetPos - transform.position;
						direction.y = 0f;
						transform.rotation = Quaternion.Lerp( transform.rotation, Quaternion.LookRotation(direction), 0.1f);
					}
				}
				
				//determine the engage aiming state
				if( (brain.currentState == CurrentState.Chase || brain.currentState == CurrentState.Engage || brain.currentState == CurrentState.Investigate) &&
				   (brain.eyes.canSeeEnemy == true || brain.ears.canHearEnemy == true)  )
				{
					//see if the angle is too large when we're already wielding
					if(Vector3.Angle( aimTargetPos - Vector3.Lerp(defaultWeaponHoldingLocation.position, engageWeaponHoldingLocation.position, engageFactor), transform.forward) < maxWeaponAngle)
					{
						engageTarget = 1f;
					}
					else
					{
						engageTarget = 0f;
					}
				}
				else
				{
					engageTarget = 0f;
				}
				
				if(engageFactorSetExternally == false)
				{
					
					//apply engage factor
					engageFactor = SmoothDampSnap(engageFactor, engageTarget, ref engageVel, engageTime, 0.01f);
				}
				

				// Translate/rotate the weapon
				if( brain.healthManager.healthState == HealthState.Normal) 
				{
				
					Vector3 pos = Vector3.Lerp(defaultWeaponHoldingLocation.position, engageWeaponHoldingLocation.position, engageFactor);
					Quaternion rot = Quaternion.Lerp(defaultWeaponHoldingLocation.rotation, Quaternion.LookRotation(aimTargetPos - pos), engageFactor);

					// Arbitrary holding location can be set by an external script for any purpose, such as positioning the weapon to a hand for melee attacks
					arbitraryFactor = SmoothDampSnap(arbitraryFactor, arbitraryHoldingLocation != null? 1f: 0f, ref arbitraryVel, engageTime, 0.01f);

					if (arbitraryHoldingLocation != null) lastArbitraryHoldingLocation = arbitraryHoldingLocation;

					if (lastArbitraryHoldingLocation != null) {
						pos = Vector3.Lerp(pos, lastArbitraryHoldingLocation.position, arbitraryFactor);
						rot = Quaternion.Lerp(rot, lastArbitraryHoldingLocation.rotation, arbitraryFactor);
					}

					currentHoldingWeapon.position = pos;
					currentHoldingWeapon.rotation = recoilOffset * rot;
				}

				// Fade out the recoil offset
				recoilOffset = Quaternion.Lerp(recoilOffset, Quaternion.identity, Time.deltaTime * 3f);

				if(currentHoldingWeapon.GetComponent<GatewayGamesWeapon>() != null)
				{
					//set being held and fire type, if info is given
					currentWeaponFiretype = currentHoldingWeapon.GetComponent<GatewayGamesWeapon>().weaponType;
					meleeAttackDistance = currentHoldingWeapon.GetComponent<GatewayGamesWeapon>().meleeAttackDistance;
					currentWeaponIK = currentHoldingWeapon.GetComponent<GatewayGamesWeapon>().ikType;
					currentWeaponMaxDistance = currentHoldingWeapon.GetComponent<GatewayGamesWeapon>().maxDistanceToShoot;
					currentIKTargets.leftHandTarget = currentHoldingWeapon.GetComponent<GatewayGamesWeapon>().ikLeftHand;
					currentIKTargets.rightHandTarget = currentHoldingWeapon.GetComponent<GatewayGamesWeapon>().ikRightHand;

					//apply dropping/picking back up based on health e.g. knocked down
					if( brain.healthManager.healthState != HealthState.Normal)
					{
						if( currentHoldingWeapon.GetComponent<Rigidbody>() == null )
						{
							currentHoldingWeapon.GetComponent<GatewayGamesWeapon>().Drop();
						}
					}
					else
					{
						if( currentHoldingWeapon.GetComponent<Rigidbody>() != null )
						{
							currentHoldingWeapon.GetComponent<GatewayGamesWeapon>().BePickedUp( transform );
							
						}
					}
					
					
				}
				else
				{
					//default back to ranged weapon if nothing specified
					currentWeaponFiretype = FireType.Ranged;
					currentWeaponIK = IKTypes.BothHands;
					currentWeaponMaxDistance = 100;
				}
				
				
				
			}
			else
			{
				//reset vars
				currentWeaponIK = IKTypes.NoHands;
				currentIKTargets.leftHandTarget = null;
				currentIKTargets.rightHandTarget = null;
			}
			
			//smooth out aiming 
			aimTargetPos = Vector3.Lerp( aimTargetPos, aimTempPos, 8f * Time.fixedDeltaTime );


			
		}
		
		
		
		
		
		/// <summary>
		/// Changes the weapon to the specified one.
		/// </summary>
		/// <param name="newWeapon">New weapon.</param>
		public virtual void ChangeWeapon(Transform newWeapon)
		{
			//cycle and find the right one
			for(int x = 0; x < multipleWeapons.Count; x++)
			{
				if(multipleWeapons[x].weaponObject == newWeapon)
				{
					ChangeWeapon( x);
					break;
				}
			}
			
		}
		
		
		
		
		
		/// <summary>
		/// Fires/throws the secondary weapon.
		/// </summary>
		public virtual void FireSecondaryWeapon()
		{
			//check null reference
			if(secondaryWeaponData.secondaryWeaponPrefab == null)
			{
				return;
			}
			
			//check if we have enough ammo
			if(secondaryWeaponData.secondaryWeaponAmount > 0)
			{
				//create the weapon to throw
				Rigidbody newSecondaryWeapon = Instantiate( secondaryWeaponData.secondaryWeaponPrefab, defaultWeaponHoldingLocation.position,
				                                        Quaternion.Euler( defaultWeaponHoldingLocation.forward ) ) as Rigidbody;
				
				//apply the correct force
				newSecondaryWeapon.AddRelativeForce( secondaryWeaponData.throwVector * secondaryWeaponData.throwForce );
				
				//apply new data
				secondaryWeaponData.secondaryWeaponAmount -= 1;
				secondaryWeaponData.lastThrowTime = Time.time;
				
				//set the animation
				brain.modelManager.ApplySecondaryWeaponAnimation();
				
			}
			
		}
		
		
		
		
		/// <summary>
		/// Changes the weapon to the specified one's index.
		/// </summary>
		/// <param name="newWeapon">New weapon.</param>
		public virtual void ChangeWeapon(int newWeapon)
		{
			//deactivate the old weapon
			currentHoldingWeapon.gameObject.SetActive( false);
			
			//activate the new weapon
			currentHoldingWeapon = multipleWeapons[newWeapon].weaponObject;
			currentHoldingWeapon.gameObject.SetActive( true);
			
		}
		
		
		/// <summary>
		/// Fires the current weapon.
		/// </summary>
		public virtual void Fire()
		{
			//call the main function, but with default values
			Fire ( currentWeaponFiretype );

		}
						
		
		/// <summary>
		/// Fires the weapon.
		/// </summary>
		/// <param name="fireType">Fire type.</param>
		public virtual void Fire( FireType fireType )
		{
			//check if we're holding the weapon in normal mode, and thus might shoot ourselves :D
			if(engageFactor < 0.7f)
			{
				return;
			}
			
			//send a message to the weapon with the way we're firing it
			//currentHoldingWeapon.SendMessage( "Fire", SendMessageOptions.DontRequireReceiver );
			if (currentHoldingWeapon.GetComponent<GatewayGamesWeapon>().Fire()) {
				var w = currentHoldingWeapon.GetComponent<GatewayGamesWeapon>();
				recoilOffset = Quaternion.AngleAxis(w.recoilAmount * (1f + Random.value * 0.3f), w.recoilVector);
			}

			//make the animation play if needed
			if(fireType == FireType.Melee)
			{
				brain.modelManager.ApplyMeleeAnimation();
			}
			
			//debug if needed
			if(debug)
			{
				Debug.Log( "SAI: Requesting bullet firing; " + Time.time );
			}
			
			//send multiplayer if needed
			gameObject.SendMessage("MultiplayerFire");
			
		}
		
		
		
		/// <summary>
		/// Aims at specified position.
		/// </summary>
		/// <param name="target">Target.</param>
		public virtual void AimAtPosition(Vector3 target)
		{
			//set the new target
			aimTempPos = target;
			
		}
		
		
		/// <summary>
		/// Turns aiming off.
		/// </summary>
		public virtual void TurnOffAiming()
		{
			//set new target to zero to signify that we don't need to aim any more
			aimTargetPos = Vector3.zero;
		}
		
		
		
		/// <summary>
		/// Reloads the weapon.
		/// </summary>
		public virtual void ReloadWeapon()
		{
			
			currentHoldingWeapon.SendMessage( "Reload", SendMessageOptions.DontRequireReceiver );
		}
		
		
		
		/// <summary>
		/// Determines whether the weapon can see the enemy.
		/// </summary>
		/// <returns><c>true</c> if this instance can see enemy from the weapon; otherwise, <c>false</c>.</returns>
		public virtual bool CanSeeEnemyFromWeapon()
		{
			//set some variables
			bool result = false;
			
			
			if(currentHoldingWeapon != null)
			{
				//raycast
				if( Physics.Raycast( currentHoldingWeapon.position, ( brain.currentEnemy.transform.position + new Vector3(0, 0.1f, 0) - currentHoldingWeapon.position ).normalized, out weaponSeeEnemyData,
				                    ( brain.currentEnemy.transform.position + new Vector3(0, 0.1f, 0) - currentHoldingWeapon.position ).magnitude ) )
				{
					//before, resulting in a negative result, check whether the hit data is the enemy
					if( brain.ObjectAtLocation( brain.currentEnemy.transform, weaponSeeEnemyData.point ) == true )
					{
						result = true;
					}
					
				}
				else
				{
					//if theres nothing, report that its visible
					result = true;
				}
				
				
			}
			
			
			return result;
		}
		
		
		/// <summary>
		/// Sends a message to the brain, saying that the AI needs to reload its weapon.
		/// </summary>
		public void WeaponNeedsReloading()
		{
			brain.ReloadWeapon();
		}
		
		
		/// <summary>
		/// Call this function if you need to simulate the 
		/// </summary>
		/// <param name="posOfBullet">Position of bullet.</param>
		public void BulletFired(Vector3 posOfBullet)
		{
			brain.ears.BulletFired( posOfBullet);
		}
	}
	
	
}
