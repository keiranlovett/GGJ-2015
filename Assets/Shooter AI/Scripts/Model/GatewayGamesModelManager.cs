using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace GatewayGames.ShooterAI
{
	
	
	public enum IKTypes { NoHands, LeftHand, RightHand, BothHands };
	
	
	
	/// <summary>
	/// Manages the models.
	/// </summary>
	public class GatewayGamesModelManager : MonoBehaviour 
	{
		
		public GatewayGamesMovementContoller movement; //reference to the movement manager
		public GatewayGamesBrain brain; //reference to brain
		public float animationLocomotionFactor = 1f; //the factor with which to apply animations if they're too quick or too slow
		//public List<AnimationClip> meleeAttackTypes = new List<AnimationClip>(); //this contains all the different 
		
		
		//references
		private Animator animator;
		private GatewayGamesWeaponManager weaponManager;
		private ShooterAIIK ikManager;
		private UpperBodyLookAt upperBodyIk;
		
		//names of different animator states
		private string forwardSpeedName = "Speed";
		private string angularSpeedName = "AngularSpeed";
		private string crouchName = "Crouch";
		private string strafeName = "Strafing";
		private string secondaryWeaponName = "FireSecondaryWeapon";
		
		//name of state
		private string stateMeleeName = "Melee Attack";

		
		
		
		void Awake()
		{
			//set references correctly
			animator = GetComponent<Animator>();
			weaponManager = brain.GetComponent<GatewayGamesWeaponManager>();
			ikManager = GetComponent<ShooterAIIK>();
			upperBodyIk = GetComponent<UpperBodyLookAt>();
		}
		
		void Start()
		{
			//set IK correctly
			SetIK();
		}
		
		
		void Update()
		{
			//set model correctly
			ApplyVariablesToModel();
			//SetIK();
		}
		
		
		
		/// <summary>
		/// Applies the variables to model.
		/// </summary>
		void ApplyVariablesToModel()
		{
			//set speed
			animator.SetFloat( forwardSpeedName, movement.movementData.forwardSpeed * animationLocomotionFactor);
			
			//set angular speed
			animator.SetFloat( angularSpeedName, movement.movementData.angularSpeed * animationLocomotionFactor);
			
			//set crouching var
			animator.SetBool( crouchName, brain.crouching);
			
			//set strafing ( 1 left, 0 no strafing, -1 right)
			if( movement.movementData.strafingState == StrafingState.NoStrafing )
			{
				animator.SetFloat( strafeName, 0f );
			}
			if( movement.movementData.strafingState == StrafingState.StrafingLeft )
			{
				animator.SetFloat( strafeName, 1f );
			}
			if( movement.movementData.strafingState == StrafingState.StrafingRight )
			{
				animator.SetFloat( strafeName, -1f );
			}
			
			
		}
		
		
		
		
		
		/// <summary>
		/// Applies the melee animation by choosing a random one from the provided list.
		/// </summary>
		public void ApplyMeleeAnimation()
		{
			ExecuteMeleeAnimation();
		}
		
		
		
		/// <summary>
		/// Executes the melee animation.
		/// </summary>
		private void ExecuteMeleeAnimation()
		{
			//attach weapon to hand
			if(weaponManager.currentWeaponIK == IKTypes.BothHands || weaponManager.currentWeaponIK == IKTypes.LeftHand)
			{
				weaponManager.arbitraryHoldingLocation = animator.GetBoneTransform( HumanBodyBones.LeftHand);
			}
			else
			{
				weaponManager.arbitraryHoldingLocation = animator.GetBoneTransform( HumanBodyBones.RightHand);
			}
			
			//deactivate IK
			ikManager.enabled = false;
			upperBodyIk.enabled = false;
			
			//execute animation
			animator.CrossFade( stateMeleeName, 0.3f);
			
			//prepare for reset
			StartCoroutine( ResetMelee( animator.GetCurrentAnimatorStateInfo(1).length) );
		}
		
		
		
		/// <summary>
		/// Applies the secondary weapon animation.
		/// </summary>
		public void ApplySecondaryWeaponAnimation()
		{
			//apply animation
			animator.SetBool( secondaryWeaponName, true );
			
			//apply reset
			StartCoroutine( ResetSecondaryWeapon() );
		}
		
		
		
		
		
		
		//<---------------------------------------- HELPER FUNCTIONS ------------------------------------------>
		
		
		
		/// <summary>
		/// Sets the IK correctly.
		/// </summary>
		public void SetIK()
		{
			//set left hand
			if(weaponManager.currentWeaponIK == IKTypes.BothHands || weaponManager.currentWeaponIK == IKTypes.LeftHand)
			{
				//set bones correctly
				ikManager.leftArm.bone1 = animator.GetBoneTransform( HumanBodyBones.LeftUpperArm);
				ikManager.leftArm.bone2 = animator.GetBoneTransform( HumanBodyBones.LeftLowerArm);
				ikManager.leftArm.bone3 = animator.GetBoneTransform( HumanBodyBones.LeftHand);
				
				//set target correctly
				ikManager.leftArm.target = weaponManager.currentIKTargets.leftHandTarget;
				
				//set goal
				ikManager.leftArm.bendGoal = weaponManager.IKBendGoalLeft;
				
			}
			
			//set right hand
			if(weaponManager.currentWeaponIK == IKTypes.BothHands || weaponManager.currentWeaponIK == IKTypes.RightHand)
			{
				//set bones correctly
				ikManager.rightArm.bone1 = animator.GetBoneTransform( HumanBodyBones.RightUpperArm);
				ikManager.rightArm.bone2 = animator.GetBoneTransform( HumanBodyBones.RightLowerArm);
				ikManager.rightArm.bone3 = animator.GetBoneTransform( HumanBodyBones.RightHand);
				
				//set target correctly
				ikManager.rightArm.target = weaponManager.currentIKTargets.rightHandTarget;
			}
			
			//upper body
			upperBodyIk.forward = brain.transform;
			upperBodyIk.weaponManager = weaponManager;
			upperBodyIk.bones = new UpperBodyLookAt.Bone[4];
			upperBodyIk.bones[0] = new UpperBodyLookAt.Bone();
			for(int x = 0; x < upperBodyIk.bones.Length; x++)
			{
				upperBodyIk.bones[x] = new UpperBodyLookAt.Bone();
			}
			
			//add in the bones
			upperBodyIk.bones[0].transform = animator.GetBoneTransform( HumanBodyBones.Spine);
			upperBodyIk.bones[0].forwardAxis = upperBodyIk.bones[0].transform.InverseTransformDirection(animator.transform.forward);
			upperBodyIk.bones[0].lookAtWeight = 0.2f;
			upperBodyIk.bones[0].localRotationOffset = new Vector3( 0f, 15f, 0f);
			upperBodyIk.bones[0].recoilRotationOffset = new Vector3( -50f, 0, 0 );
			
			upperBodyIk.bones[1].transform = animator.GetBoneTransform( HumanBodyBones.Neck).parent.parent;
			upperBodyIk.bones[1].forwardAxis = upperBodyIk.bones[1].transform.InverseTransformDirection(animator.transform.forward);
			upperBodyIk.bones[1].lookAtWeight = 0f;
			upperBodyIk.bones[1].localRotationOffset = new Vector3( 0f, 30f, 0f);
			upperBodyIk.bones[1].recoilRotationOffset = new Vector3( -50f, 0, 0 );
			
			upperBodyIk.bones[2].transform = animator.GetBoneTransform( HumanBodyBones.Neck).parent;
			upperBodyIk.bones[2].forwardAxis = upperBodyIk.bones[2].transform.InverseTransformDirection(animator.transform.forward);
			upperBodyIk.bones[2].lookAtWeight = 0.2f;
			
			upperBodyIk.bones[3].transform = animator.GetBoneTransform( HumanBodyBones.Head);
			upperBodyIk.bones[3].forwardAxis = upperBodyIk.bones[3].transform.InverseTransformDirection(animator.transform.forward);
			upperBodyIk.bones[3].lookAtWeight = 0.7f;
			upperBodyIk.bones[3].localRotationOffset = new Vector3( 0f, 0f, 0f);
			upperBodyIk.bones[3].recoilRotationOffset = new Vector3( 50f, 0, 0 );
		}
		
		
		
		/// <summary>
		/// Resets the secondary weapon.
		/// </summary>
		/// <returns>The secondary weapon.</returns>
		private IEnumerator ResetSecondaryWeapon()
		{
			yield return new WaitForFixedUpdate();
			animator.SetBool( secondaryWeaponName, false );
			
		}
		
		/// <summary>
		/// Resets the melee.
		/// </summary>
		/// <returns>The melee.</returns>
		private IEnumerator ResetMelee(float timeToResetAfter)
		{
			//wait
			yield return new WaitForSeconds( timeToResetAfter);
			
			//attach weapon to hand
			weaponManager.arbitraryHoldingLocation = null;

			//activate IK
			ikManager.enabled = true;
			upperBodyIk.enabled = true;
			
		}	
		
	}
	
	
}