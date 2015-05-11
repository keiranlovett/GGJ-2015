using UnityEngine;
using System.Collections;
using System;

namespace GatewayGames.ShooterAI {

	/// <summary>
	/// Maintains a collection of IK solvers for Shooter AI.
	/// </summary>
	public class ShooterAIIK : MonoBehaviour {

		/// <summary>
		/// Is AnimatePhysics turned on for the character?
		/// </summary>
		public bool animatePhysics;

		/// <summary>
		/// The IK solvers
		/// </summary>
		public IKSolverLimb leftArm, rightArm;

		private bool fixedFrame;
		
		//head ik
		private Vector3 aimVector;
		private GatewayGamesWeaponManager weaponManager;
		private Transform eyes;
		private Transform head;
		
		void Start() {
			//set caches
			weaponManager = GetComponent<GatewayGamesModelManager>().brain.GetComponent<GatewayGamesWeaponManager>();
			eyes = GetComponent<GatewayGamesModelManager>().brain.eyes.transform;
			head = GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).transform;
			
			// Sample the initial pose of the limbs to know which way they bend
			leftArm.SamplePose();
			rightArm.SamplePose();
		}

		void FixedUpdate() {
			// Check if a FixedUpdate has been called
			fixedFrame = true;
		}

		public void LateUpdate() {
			// If aimatePhysics is turned on, only update when a FixedUpdate has been called
			if (animatePhysics && !fixedFrame) return;

			// Update the solvers
			leftArm.UpdateSolver();
			rightArm.UpdateSolver();
			
			//do head IK
			HeadIK();
			
			fixedFrame = false;
		}
		
		
		/// <summary>
		/// Applies head IK.
		/// </summary>
		void HeadIK()
		{
			//get aim pos;
			aimVector = weaponManager.aimTargetPos - head.position;
			
			//see whether its within a reasonable angle
			if( Vector3.Angle( eyes.forward, aimVector) < 80f )
			{
				//apply to head rotation;
				head.rotation = Quaternion.Slerp( head.rotation, Quaternion.LookRotation( aimVector ), 0.5f);
			}
		}
		
	}
}
