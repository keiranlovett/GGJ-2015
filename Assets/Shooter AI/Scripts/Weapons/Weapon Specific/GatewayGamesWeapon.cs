using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace GatewayGames.ShooterAI
{
	
	/// <summary>
	/// Which method the weapon should use to fire.
	/// </summary>
	public enum WeaponShootMethod { RayCast, Projectile };
	
	
	/// <summary>
	/// Which damage model to use.
	/// </summary>
	public enum DamageModel { Global, RegionBased };
	
	
	
	/// <summary>
	/// Controls the weapon that this script is attached to.
	/// </summary>
	public class GatewayGamesWeapon : MonoBehaviour 
	{
		[Space(20f)]
		//reference data
		public Transform holdingAI; //reference to tha AI that is holding this weapon
		public Transform bulletCreationPosition = null; //the position at which to create the bullets
		public Transform ikLeftHand = null; //the left hand ref
		public Transform ikRightHand = null; //the right hand ref
		public Transform muzzleFlashManager = null; //the muzzle flash manager ref
		
		[Space(20f)]
		//meta data
		public bool debug = false; //debug data
		public bool beingHeld = false; //whether this weapon is currently being held or not
		public FireType weaponType = FireType.Ranged; //the type of weapon that this is
		public IKTypes ikType = IKTypes.BothHands; //how this weapon is held
		public float maxDistanceToShoot = 100f; //the max distance from which to use this weapon
		public bool fallBackToMelee = false; //if the weapon runs out of ammo, could it be used as a melee
		public string damageMethodName = "Damage"; //the damage method name
		
		
		[Space(20f)]
		//firing values
		public WeaponShootMethod weaponShootMethod = WeaponShootMethod.RayCast; //the way that the weapon fires
		public Rigidbody projectilePrefab = null; //if the weapon uses projectiles, specifiy the prefab
		public float projectileForce = 10000f; //the projectile force applied, if there is a projectile
		
		[Space(20f)]
		public int ammoInMagazine = 10; //the ammount of ammo in the current magazine
		public int ammoPerMagazine = 10; //the amount of ammo in each new magazine
		public int amountOfMagazines = 5; //the amount of magazines for this weapon that the AI has
		
		[Space(20f)]
		public bool allowedToShoot = true; //whether the weapon can shoot
		public float timeBetweenShots = 0.1f; //the amount of time between shots
		
		[Space(20f)]
		public Vector3 recoilVector = Vector3.up; //the vector for the recoil
		public float recoilAmount = 0.1f; //the recoil applied to the weapon
		
		[Space(20f)]
		public DamageModel damageModel = DamageModel.RegionBased; //what type of model to use for damage
		public float damage = 0.3f; //the amount of damage this weapon applies
		public bool allowTeamKilling = false; //whether to allow team killing or not
		
		[Space(20f)]
		//reload data
		public bool reloading = false; //whether the weapon is reloading
		public float reloadTime = 2f; //the realod time for this weapon
		public Rigidbody emptyMagazine = null; //empty magazine object
		public Vector3 emptyMagazineThrowVector = Vector3.down; //where to throw the magazine
		public float emptyMagazineThrowForce = 5f; //the throw force for the empty magazine
		
		[Space(20f)]
		//impact effects
		public Transform impactEffect = null; //impact effect prefab
		
		[Space(20f)]
		//bullet shots
		public List<AudioClip> bulletShotSounds = new List<AudioClip>(); //a list with the possible shot sounds
		public float distanceOfAIHearingBulletShot = 30f; //the distance from which this weapon can be heard firing
		public List<AudioClip> meleeSound = new List<AudioClip>(); //a list with the possible melee sounds to play
		
		[Space(20f)]
		//melee effects
		public float meleeDefaultLength = 0.3f; //if not using standard animator components, how long does the animation take
		public float meleeAttackDistance = 0.5f; //the attack distance to use for melee
		
		
		
		private RaycastHit hitInfo; //used often for raycasting
		private float rigidbodyMass; //the mass of the weapon
		private AudioSource aSource; //ref to audio source
		
		
		void Awake()
		{
			//set caches correctly
			rigidbodyMass = GetComponent<Rigidbody>().mass;
			
			//create audio source and ref
			if(GetComponent<AudioSource>() == null)
			{
				gameObject.AddComponent<AudioSource>();
			}
			aSource = GetComponent<AudioSource>();
		}
		
		
		void Update()
		{
			//check if we need reloading
			if(ammoInMagazine <= 0 && reloading == false)
			{
				//send message updwards
				holdingAI.SendMessage( "WeaponNeedsReloading", SendMessageOptions.DontRequireReceiver );
			}
			
			//check if we need to fallback to melee
			if( fallBackToMelee == true)
			{
				//go to melee
				if(ammoInMagazine <= 0 && amountOfMagazines <= 0)
				{
					weaponType = FireType.Melee;
				}
				else
				{
					//go back to normal if we've got ammo
					weaponType = FireType.Ranged;
				}
			}
		}
		
		
		/// <summary>
		/// Drop this weapon.
		/// </summary>
		public virtual void Drop()
		{
			//apply vars
			beingHeld = false;
			
			//enable collider & rigidbody
			GetComponent<Collider>().enabled = true;
			gameObject.AddComponent<Rigidbody>();
			GetComponent<Rigidbody>().mass = rigidbodyMass;
		}
		
		
		
		/// <summary>
		/// Sets rigidobdy correctly for being picked up
		/// </summary>
		public virtual void BePickedUp( )
		{
			//apply vars
			beingHeld = true;
			
			//disable collider & rigidbody
			GetComponent<Collider>().enabled = false;
			rigidbodyMass = GetComponent<Rigidbody>().mass;
			Destroy( GetComponent<Rigidbody>() );
		}
		
		
		/// <summary>
		/// Sets rigidobdy correctly for being picked up and other vars.
		/// </summary>
		public virtual void BePickedUp( Transform newHoldingAI )
		{
			//apply vars
			beingHeld = true;
			holdingAI = newHoldingAI;
			
			//disable collider & rigidbody
			GetComponent<Collider>().enabled = false;
			rigidbodyMass = GetComponent<Rigidbody>().mass;
			Destroy( GetComponent<Rigidbody>() );
		}
		
		
		/// <summary>
		/// Reload this weapon.
		/// </summary>
		public virtual void Reload()
		{
			//start the reload co-routine
			StartCoroutine( ReloadCycle( reloadTime ) );
		}
		
		
		
		
		/// <summary>
		/// Fire this weapon.
		/// </summary>
		public virtual bool Fire()
		{
			//check if we can fire
			if(allowedToShoot == false || ammoInMagazine <= 0)
			{
				return false;
			}
			
			if(debug)
			{
				Debug.Log( "SAI: Fire request registered and being applied; " + Time.time );
			}
			
			
			//apply different attack types
			if( weaponType == FireType.Ranged)
			{
				//do muzzle flash
				ApplyMuzzleFlash();

				//apply new data
				ammoInMagazine -= 1;
				allowedToShoot = false;
				StartCoroutine( ResetTimeBetweenShots(timeBetweenShots) );


				//ranged attack
				
				if( weaponShootMethod == WeaponShootMethod.RayCast )
				{
					
					Debug.DrawRay(bulletCreationPosition.position,  bulletCreationPosition.forward * float.MaxValue, Color.green);
					
					//raycasting method
					if( Physics.Raycast( bulletCreationPosition.position,  bulletCreationPosition.forward, out hitInfo, float.MaxValue) )
					{	
						//check for self/team killing
						Transform testTransform = hitInfo.collider.transform;
						for(int x = 0; x < 10; x++)
						{
							if( testTransform.IsChildOf(holdingAI) )
							{
								return false;
							}
							if(allowTeamKilling == true && testTransform.tag == holdingAI.tag)
							{
								return false;
							}
							if(testTransform.parent != null)
							{
								testTransform = testTransform.parent;
							}
						}
						
						//debug
						if(debug)
						{
							Debug.Log("SAI: Hit: " + hitInfo.transform);
						}
						
						//apply damage, based on model
						if(damageModel == DamageModel.Global)
						{
							hitInfo.transform.SendMessageUpwards( damageMethodName, damage, SendMessageOptions.DontRequireReceiver );
							ApplyImpactEffects( hitInfo.normal, hitInfo.point );
						}
						else
						{
							hitInfo.transform.SendMessageUpwards( damageMethodName, SendMessageOptions.DontRequireReceiver );
							ApplyImpactEffects( hitInfo.normal, hitInfo.point );
						}

						
					}
					
				}
				else
				{
					//create the bullet projectile at the position
					Rigidbody bullet = Instantiate( projectilePrefab, bulletCreationPosition.position, Quaternion.Euler( bulletCreationPosition.transform.forward) ) as Rigidbody;
					
					//apply force
					bullet.AddForce( transform.forward * projectileForce);

				}
				
				
				
			}
			else
			{
				//melee attack
				
				// enable the collider as trigger
				GetComponent<Collider>().enabled = true;
				GetComponent<Collider>().isTrigger = true;
				
				
				//apply reset, based on animation info
				if( holdingAI.GetComponent<GatewayGamesBrain>() != null )
				{
					StartCoroutine( ResetTimeBetweenShotsMelee( holdingAI.GetComponent<GatewayGamesBrain>().modelManager.GetComponent<Animator>().GetCurrentAnimatorStateInfo(1).length ) );	
				}
				else
				{
					StartCoroutine( ResetTimeBetweenShotsMelee( meleeDefaultLength ) );
				}
				
				//do sound
				if(meleeSound.Count > 0)
				{
					aSource.clip = meleeSound[ (int)Random.Range( 0, meleeSound.Count) ];
					aSource.Play();
				}
			}

			return true;
		}
		
		
		
		
		
		public void OnTriggerEnter(Collider colliderInfo)
		{
			//if this weapon got triggered, it means that during melee it hit something
			
			if(beingHeld == true && weaponType == FireType.Melee)
			{
				//apply damage, based on model
				if(damageModel == DamageModel.Global)
				{
					colliderInfo.transform.SendMessage( damageMethodName, damage, SendMessageOptions.DontRequireReceiver );
				}
				else
				{
					colliderInfo.transform.SendMessage( damageMethodName, SendMessageOptions.DontRequireReceiver );
				}
				
				if(debug)
				{
					Debug.Log( "SAI: Melee hit something; " + colliderInfo.gameObject, gameObject );
				}
				
			}
			
		}
		
		
		
		
		
		/// <summary>
		/// Applies the impact effects.
		/// </summary>
		/// <param name="hitNormal">Hit normal.</param>
		/// <param name="hitPos">Hit position.</param>
		public virtual void ApplyImpactEffects( Vector3 hitNormal, Vector3 hitPos)
		{
			if( impactEffect != null)
			{
				//create the new impact effect with reversed normal
				Instantiate( impactEffect, hitPos, Quaternion.Euler( - hitNormal) );
			}
			
		}
		
		
		
		
		//<--------------------------------------------------------------- HELPER FUNCTIONS ---------------------------------------->



		/// <summary>
		/// Applies the muzzle flash and sound.
		/// </summary>
		private void ApplyMuzzleFlash()
		{
			//make sound
			//aSource.PlayOneShot( bulletShotSounds[ (int)Random.Range( 0, bulletShotSounds.Count) ] );
			aSource.clip = bulletShotSounds[ (int)Random.Range( 0, bulletShotSounds.Count) ];
			aSource.Play();
			
			//send message to muzzle flash manager
			muzzleFlashManager.SendMessage( "MuzzleFlash", SendMessageOptions.DontRequireReceiver );
			
			//apply AI hearing bullet
			Collider[] potentialAI = Physics.OverlapSphere( transform.position, distanceOfAIHearingBulletShot);
			for(int x = 0; x < potentialAI.Length; x++)
			{
				if( potentialAI[x].transform.IsChildOf( holdingAI ) == false)
				{
					potentialAI[x].SendMessageUpwards( "BulletFired", transform.position, SendMessageOptions.DontRequireReceiver);
				}
			}
			
		}

		
		/// <summary>
		/// Resets the time between shots.
		/// </summary>
		/// <returns>The time between shots.</returns>
		/// <param name="time">Time.</param>
		private IEnumerator ResetTimeBetweenShots(float time)
		{	
			//apply recoil
			//transform.Rotate( recoilVector, recoilAmount * Random.value );
			
			//set vars
			allowedToShoot = false;
			
			//wait
			yield return new WaitForSeconds( time);
			
			//reset vars
			allowedToShoot = true;
		}	
		
		
		/// <summary>
		/// Resets the time between shots for melee.
		/// </summary>
		/// <returns>The time between shots.</returns>
		/// <param name="time">Time.</param>
		private IEnumerator ResetTimeBetweenShotsMelee(float time)
		{
			allowedToShoot = false;
			
			yield return new WaitForSeconds( time);
			
			allowedToShoot = true;
			GetComponent<Collider>().enabled = false;
			GetComponent<Collider>().isTrigger = false;
		}

		
		
		private IEnumerator ReloadCycle(float time)
		{
			//set vars correctly
			reloading = true;
			allowedToShoot = false;
			
			//wait
			yield return new WaitForSeconds( time);
			
			//set new data
			ammoInMagazine = ammoPerMagazine;
			amountOfMagazines -= 1;
			reloading = false;
			allowedToShoot = true;
			
			//create empty representative
			if( emptyMagazine != null)
			{
				Rigidbody emptyMagazineObject = Instantiate( emptyMagazine, transform.position, Quaternion.identity ) as Rigidbody;
				emptyMagazineObject.AddForce( emptyMagazineThrowVector * emptyMagazineThrowForce );
			}
			
		}
		
		
	}
	
	
	
	
}