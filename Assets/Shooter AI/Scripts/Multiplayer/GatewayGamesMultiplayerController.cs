using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon;

namespace GatewayGames.ShooterAI
{
	
	[ RequireComponent( typeof( PhotonView ) ) ]
	
	
	public class GatewayGamesMultiplayerController : Photon.MonoBehaviour 
	{
	
		//debug
		public bool debug = false;
		
		
		//lerping variables
		private float minDistanceToTeleport = 3f; //the distance after which to teleport and not interpolate
		
		//cache variables
		private GameObject model;
		private Animator animator;
		private ShooterAIIK ik;
		private UpperBodyLookAt upperBodyLookAt;
		private GatewayGamesRagdollHelper rgHelper;
		private GatewayGamesWeaponManager weaponManager;
		private GatewayGamesBrain brain;
		
		//data for passing over the network
		Vector3 correctPos = Vector3.zero; //the correct position
		Quaternion correctRot = Quaternion.identity; //the correct rotation
		bool ragdollState = false; //whether we're currently in ragdoll state
		Vector3 correctAimPos = Vector3.zero; //correct aim position
		float correctEngageFactor = 0f; //correct engage factor for holding weapon
		
		
		void ApplySettings()
		{
			if(debug)
			{
				Debug.Log("Applying settings needed for MP");
			}
			
			//set variables correctly
			photonView.observed = this;
			model = GetComponent<GatewayGamesBrain>().modelManager.gameObject;
			animator = model.GetComponent<Animator>();
			brain = GetComponent<GatewayGamesBrain>();
			brain.modelManager.SetIK();
			ik = model.GetComponent<ShooterAIIK>();
			upperBodyLookAt = model.GetComponent<UpperBodyLookAt>();
			rgHelper = model.GetComponent<GatewayGamesRagdollHelper>();
			weaponManager = GetComponent<GatewayGamesWeaponManager>();
			
			//destroy/deactivate unnecceary components
			if( photonView.isMine == false)
			{
				if(debug)
				{
					Debug.Log("Destroying unneeded components");
				}
				
				//deactivate
				//GetComponent<GatewayGamesWeaponManager>().enabled = false;
				brain.modelManager.enabled = false;
				weaponManager.engageFactorSetExternally = true;
				
				//nav system
				if( GetComponent<NavMeshAgent>() != null )
				{
					GetComponent<NavMeshAgent>().enabled = false;
				}
				else
				{
					GetComponent<GatewayGamesAgent>().enabled = false;
					GetComponent<Seeker>().enabled = false;
				}

				brain.healthManager.gameObject.SetActive(false);
				brain.eyes.enabled = false;
				brain.ears.enabled = false;
				brain.patrolManager.gameObject.SetActive(false);
				GetComponent<GatewayGamesSearchCover>().enabled = false;
				GetComponent<GatewayGamesMovementContoller>().enabled = false;
				GetComponent<GatewayGamesAudioControl>().enabled = false;
				brain.enabled = false;
					
			}
			
		}
		
		
		
		/// <summary>
		/// Transitions to master.
		/// </summary>
		public void TransitionToMaster()
		{
			if(debug)
			{
				Debug.Log("Becoming new master");
			}
			
			//activate
			//GetComponent<GatewayGamesWeaponManager>().enabled = false;
			brain.modelManager.enabled = true;
			weaponManager.engageFactorSetExternally = false;
			
			//nav system
			if( GetComponent<NavMeshAgent>() != null )
			{
				GetComponent<NavMeshAgent>().enabled = true;
			}
			else
			{
				GetComponent<GatewayGamesAgent>().enabled = true;
				GetComponent<Seeker>().enabled = true;
			}
			
			brain.healthManager.gameObject.SetActive(true);
			brain.eyes.enabled = true;
			brain.ears.enabled = true;
			brain.patrolManager.gameObject.SetActive(true);
			GetComponent<GatewayGamesSearchCover>().enabled = true;
			GetComponent<GatewayGamesMovementContoller>().enabled = true;
			GetComponent<GatewayGamesAudioControl>().enabled = true;
			brain.enabled = true;
		}

		
		
		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			
			if (stream.isWriting) 
			{
				//send data
				if(debug)
				{
					Debug.Log("Sending data");
				}
				
				//send the position
				correctPos = transform.position;
				stream.Serialize( ref correctPos);
				
				
				//send correct rotation
				correctRot = transform.rotation;
				stream.Serialize( ref correctRot);
				
				
				//send ragdoll state
				ragdollState = rgHelper.ragdolled;
				stream.Serialize( ref ragdollState);
				
				
				//send aim data
				if(weaponManager != null)
				{
					correctAimPos = weaponManager.aimTargetPos;
				}
				stream.Serialize( ref correctAimPos);
				
				
				//send holding data
				if(weaponManager != null)
				{
					correctEngageFactor = weaponManager.engageFactor;
				}
				stream.Serialize( ref correctEngageFactor);
				
			}
			else
			{
				//receive data
				if(debug)
				{
					Debug.Log("Receiving data");
				}
				
				
				//receive the position
				stream.Serialize( ref correctPos);
				
				
				//receive correct rotation
				stream.Serialize( ref correctRot);
				
				
				//receive ragdoll state
				stream.Serialize( ref ragdollState);
				if(rgHelper != null)
				{
					rgHelper.ragdolled = ragdollState;
				}
				
				
				
				//receive aim data
				stream.Serialize( ref correctAimPos);
				if(weaponManager != null)
				{
					weaponManager.AimAtPosition( correctAimPos);
				}
				
				
				//receive holding data
				stream.Serialize( ref correctEngageFactor);
				if(weaponManager != null)
				{
					weaponManager.engageFactor = correctEngageFactor;
				}
				
			}
			
			
		}
		
		
		void Update()
		{
			this.photonView.observed = this;
			if (this.photonView == null || this.photonView.observed != this)
			{
				Debug.LogWarning(this + " is not observed by this object's photonView! OnPhotonSerializeView() in this class won't be used. The component being observed is: " + this.photonView.observed);
			}
			
			
			//handle smooth movement and rotation
			if(photonView.isMine == false && PhotonNetwork.connected == true)
			{
				
				//if we're too far away from the correct position, jump there, else smoothly interpolate there
				if(Vector3.Distance( transform.position, correctPos) > minDistanceToTeleport)
				{
					//teleport
					transform.position = correctPos;
				}
				else
				{
					//interpolate
					transform.position = Vector3.Lerp(transform.position, correctPos, 0.1f);
				}
				
				
				//interpolate the rotation
				transform.rotation = Quaternion.Lerp( transform.rotation, correctRot, 0.1f);
			}
			
			
			//check if settings need to be applied
			if(vp_MPMaster.Phase == vp_MPMaster.GamePhase.Playing && model == null)
			{
				ApplySettings();
			}
			
			//apply ragdoll settings
			if(model != null && ik != null)
			{
				ik.enabled = !rgHelper.ragdolled;
				upperBodyLookAt.enabled = !rgHelper.ragdolled;
			}
			if(animator != null && rgHelper != null)
			{
				animator.enabled = !rgHelper.ragdolled;
				
			}
			
			//check if we need to transition to master client
			if(PhotonNetwork.isMasterClient == true && brain != null && brain.enabled == false)
			{
				//transtion to master client setting
				TransitionToMaster();
			}
		}
		
		
		
		
		/// <summary>
		/// This gets called everytime the current AI shoots. This then passes on to correctly call this event on all AIs
		/// </summary>
		public void MultiplayerFire()
		{
			//call the event on all the clients
			if(photonView.isMine)
			{
				photonView.RPC( "AIMultiplayerShoot", PhotonTargets.Others );
			}
			
			
		}
		

		
		/// <summary>
		/// This handles the multiplayer shooting on the receiving end.
		/// </summary>
		/// <param name="typeOfAttack">Type of attack.</param>
		[RPC]
		public void AIMultiplayerShoot()
		{
			if(debug)
			{
				Debug.Log("Shooting");
			}
			
			//call the event on the local client
			GetComponent<GatewayGamesWeaponManager>().Fire();
		}
		
		
		
		//health management
		
		
		/// <summary>
		/// Deducts the health.
		/// </summary>
		/// <param name="amount">Amount.</param>
		public void DeductHealth(float amount)
		{

			photonView.RPC ( "MultiplayerDeductHealth", photonView.owner, amount);
		}
		
		[RPC]
		public void MultiplayerDeductHealth(float amount)
		{
			//send the data down
			GetComponent<GatewayGamesBrain>().healthManager.ApplyDamageToAI( amount, false, false, 3f );
			
		}
		
		
		
		/// <summary>
		/// Sends info that the AI is dead.
		/// </summary>
		public void CharacterDead()
		{
			if(debug)
			{
				Debug.Log("Character Dead");
			}
			photonView.RPC ( "MultiplayerCharacterDead", photonView.owner);
		}
		
		
		[RPC]
		public void MultiplayerCharacterDead()
		{
			if(debug)
			{
				Debug.Log("Character Dead");
			}
			
			//send the data down
			GetComponent<GatewayGamesBrain>().healthManager.Die();
			
		}
		
		
		
		
		/// <summary>
		/// Sends info that the AI is knocked out.
		/// </summary>
		public void Knockout(float time)
		{
			photonView.RPC ( "MultiplayerKnockout", photonView.owner, time);
		}
		
		
		[RPC]
		public void MultiplayerKnockout(float time)
		{
			//send the data down
			GetComponent<GatewayGamesBrain>().healthManager.KnockDown( time);
			
		}
		
		/// <summary>
		/// Fires the secondary weapon.
		/// </summary>
		public void SecondaryWeapon()
		{
			photonView.RPC ( "MultiplayerSecondaryWeapon", photonView.owner);
		}
		
		
		/// <summary>
		/// Sync secondary weapon.
		/// </summary>
		[RPC]
		public void MultiplayerSecondaryWeapon()
		{
			GetComponent<GatewayGamesWeaponManager>().FireSecondaryWeapon();
		}
		
		
	}
}