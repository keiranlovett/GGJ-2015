using UnityEngine;
using System.Collections;
using System;

namespace GatewayGames.ShooterAI {

	/// <summary>
	/// 3-segment trigonometric IK solver designed for Shooter AI by Pärtel Lang
	/// support@root-motion.com
	/// </summary>
	[System.Serializable]
	public class IKSolverLimb {

		#region Public

		// Adding duplicate XML comments here for automatic documentation generators

		/// <summary>
		/// The first bone of the limb (upper arm/thigh).
		/// </summary>
		[Tooltip("The first bone of the limb (upper arm/thigh)")]
		public Transform bone1;

		/// <summary>
		/// The second bone of the limb (forearm/calf).
		/// </summary>
		[Tooltip("The second bone of the limb (forearm/calf)")]
		public Transform bone2;

		/// <summary>
		/// The third bone of the limb (hand/foot).
		/// </summary>
		[Tooltip("The third bone of the limb (hand/foot)")]
		public Transform bone3;

		/// <summary>
		/// The target Transform, will solve to it's position and rotation.
		/// </summary>
		[Tooltip("The target Transform, will solve to it's position and rotation")]
		public Transform target;

		/// <summary>
		/// The weight of reaching to the target position.
		/// </summary>
		[Tooltip("The weight of reaching to the target position")]
		[Range(0f, 1f)] public float positionWeight = 1f;

		/// <summary>
		/// The weight of rotating the 3rd bone to the target rotation.
		/// </summary>
		[Tooltip("The weight of rotating the 3rd bone to the target rotation")]
		[Range(0f, 1f)] public float rotationWeight = 1f;

		/// <summary>
		/// If assigned will keep the limb bent in the direction from the first bone to the Bend Goal.
		/// </summary>
		[Tooltip("If assigned will keep the limb bent in the direction from the first bone to the Bend Goal")]
		public Transform bendGoal;

		/// <summary>
		/// The weight of bending the limb towards the Bend Goal.
		/// </summary>
		[Tooltip("The weight of bending the limb towards the Bend Goal")]
		[Range(0f, 1f)] public float bendGoalWeight;

		/// <summary>
		/// If true, the limb will be rotated to it's initial pose each time before solving. 
		/// This gets rid of animation on the limb. To sample a pose, call ShooterAIIK.SamplePose().
		/// </summary>
		[Tooltip("If true, the limb will be rotated to it's initial pose each time before solving. This gets rid of animation on the limb. To sample a pose, call ShooterAIIK.SamplePose()")]
		[ContextMenuItem("Sample Pose", "SamplePose")]
		public bool fixToSampledPose;

		/// <summary>
		/// If true, the rotation of the second bone will be limited to the range of a hemisphere so the elbow/knee can not be flipped/snapped. 
		/// Check this if you experience joint flipping because of animation compression/keyframe reduction/interpolation issues. 
		/// An alternative solution would be to use a Bend Goal to always have full control over the bending direction of the limb.
		/// </summary>
		[Tooltip("If true, the rotation of the second bone will be limited to the range of a hemisphere so the elbow/knee can not be flipped/snapped. Check this if you experience joint flipping because of animation compression/keyframe reduction/interpolation issues. An alternative solution would be to use a Bend Goal to always have full control over the bending direction of the limb")]
		public bool clampJointRange;

		/// <summary>
		/// The clamping factor for the elbow/knee joint (0 - 1). 0 is completely free, 1 is completely fixed to 90 degree angle, 0.5 is the range of PI.
		/// </summary>
		[System.NonSerializedAttribute] public float clampF = 0.505f;

		/// <summary>
		/// Sending messages on update events.
		/// </summary>
		public delegate void UpdateDelegate();
		/// <summary>
		/// Called each time before solving.
		/// </summary>
		public UpdateDelegate OnPreSolve;
		/// <summary>
		/// Called each time after solving.
		/// </summary>
		public UpdateDelegate OnPostSolve;

		/// <summary>
		/// returns true if the bone setup seems valid.
		/// </summary>
		public bool Validate() {
			validated = false;
			
			if (bone1 == null || bone2 == null || bone3 == null) {
				Debug.LogWarning("a Bone is unassigned in IK solver");
				return false;
			}
			
			if (bone1 == bone2) {
				Debug.LogWarning("Bone 1 is the same as Bone 2 in IK solver, please assign another bone.", bone1);
				return false;
			}
			
			if (bone2 == bone3) {
				Debug.LogWarning("Bone 2 is the same as Bone 3 in IK solver, please assign another bone.", bone2);
				return false;
			}
			
			if (bone3 == bone1) {
				Debug.LogWarning("Bone 3 is the same as Bone 1 in IK solver, please assign another bone.", bone3);
				return false;
			}
			
			if (!HierarchyIsValid()) {
				Debug.LogWarning("Invalid bone hierarchy detected in IK solver. The Bones need to belong to the same branch of hierarchy and be assigned in descending order. In case of an arm, first bone should be the upper arm, second the forearm and third the hand.", bone1); 
			}
			
			if (bone1.position == bone2.position || bone2.position == bone3.position || bone3.position == bone1.position) {
				Debug.LogWarning("Bone length is zero in IK solver. Make sure the bones are not in the exact same position.", bone1);
				return false;
			}
			
			_bone1 = bone1;
			_bone2 = bone2;
			_bone3 = bone3;
			
			validated = true;
			return true;
		}

		/// <summary>
		/// Samples the current pose of the character to know which way the limb should be bent.
		/// </summary>
		public void SamplePose() {
			// If the bone hierarchy has changed, revalidate
			if (_bone1 != bone1 || _bone2 != bone2 || _bone3 != bone3) Validate();
			if (!validated) return;

			// Store the local rotations of the bones
			sampledLocalRotation1 = bone1.localRotation;
			sampledLocalRotation2 = bone2.localRotation;
			sampledLocalRotation3 = bone3.localRotation;

			// Check if the limb is stretched out
			if (Vector3.Dot((bone2.position - bone1.position).normalized, (bone3.position - bone2.position).normalized) >= 0.999f) {
				Debug.LogWarning("The limb {" + bone1.name + ", " + bone2.name + ", " + bone3.name + "} is completely stretched out. IK solver can not tell which way to bend the limb. Please rotate the forearm/knee bone in the scene view so the limb is bent slightly in its natural bending direction.", bone2.transform);
			}
			
			// Find the default bend direction orthogonal to the limb direction
			Vector3 direction = OrthoToBone1(OrthoToLimb(bone2.position - bone1.position));
			
			// Default bend direction relative to the first node
			sampledDirectionInBone1Space = Quaternion.Inverse(bone1.rotation) * direction;
			
			poseSampled = true;
		}

		/// <summary>
		/// Updates the solver, rotates the bones.
		/// </summary>
		public void UpdateSolver() {
			if (OnPreSolve != null) OnPreSolve();

			// If completely weighed out, do nothing
			if (positionWeight <= 0f && rotationWeight <= 0f) return;

			// If the bone hierarchy has changed, revalidate
			if (_bone1 != bone1 || _bone2 != bone2 || _bone3 != bone3) Validate();
			if (!validated) return;

			// If there is no target, do nothing
			if (target == null) {
				if (positionWeight > 0f || rotationWeight > 0f || bendGoalWeight > 0f) Debug.LogWarning("Target unassigned in IK solver.", bone1);
				return;
			}

			// Sample if not yet sampled
			if (!poseSampled) SamplePose();

			if (fixToSampledPose) {
				// Keep the bones fixed at their sampled rotations
				FixToSampledPose();
			} else if (clampJointRange) LimitBend(); // Make sure the elbow/knee is not flipped compared to the its sampled rotation

			// Read the current pose
			Read();
			
			// Solve inverse kinematics
			Solve();

			// Write changes to the bones
			Write();

			if (OnPostSolve != null) OnPostSolve();
		}

		#endregion Public

		private float sqrMag1, sqrMag2;
		private Vector3 axis1, axis2;
		private Quaternion sampledLocalRotation1, sampledLocalRotation2, sampledLocalRotation3;
		private Vector3 bendNormal;
		private Vector3 bendDirection;
		private bool poseSampled;
		private bool validated;
		private Transform _bone1, _bone2, _bone3;
		private Vector3 sampledDirectionInBone1Space;

		// Rotate the bones back to where they were at sampling time
		private void FixToSampledPose() {
			bone1.localRotation = sampledLocalRotation1;
			bone2.localRotation = sampledLocalRotation2;
			bone3.localRotation = sampledLocalRotation3;
		}

		// Read the current pose of the limb
		private void Read() {
			// Calculate square mags for the bones
			sqrMag1 = Vector3.SqrMagnitude(bone2.position - bone1.position);
			sqrMag2 = Vector3.SqrMagnitude(bone3.position - bone2.position);

			// Axis to the next bone in relative bone space
			axis1 = Quaternion.Inverse(bone1.rotation) * (bone2.position - bone1.position);
			axis2 = Quaternion.Inverse(bone2.rotation) * (bone3.position - bone2.position);

			// Normal of the bending plance
			bendNormal = Vector3.Cross(bone2.position - bone1.position, bone3.position - bone1.position).normalized;
		}
		
		private void Solve() {
			// If not reaching for the target position, no solving required
			if (positionWeight <= 0f) return;

			// Make use of the Bend Goal
			if (bendGoal != null && bendGoalWeight > 0f) {
				Vector3 goalBendNormal = Vector3.Cross(bendGoal.position - bone1.position, target.position - bone1.position);
				
				if (goalBendNormal != Vector3.zero) bendNormal = Vector3.Lerp(bendNormal, goalBendNormal, bendGoalWeight);
			}

			// Get the direction towards the solved position of the second bone
			bendDirection = GetBendDirection(target.position, bendNormal);
		}

		// Write the solved rotations to the bones
		private void Write() {
			// Interpolating the first 2 bones
			if (positionWeight > 0f) {
				bone1.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(bone1.rotation * axis1, bendDirection), positionWeight) * bone1.rotation;
				bone2.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(bone2.rotation * axis2, target.position - bone2.position), positionWeight) * bone2.rotation;
			}

			// Rotate the last bone to the target rotation
			if (rotationWeight > 0f) {
				bone3.rotation = Quaternion.Lerp(bone3.rotation, target.rotation, rotationWeight);
			}
		}
		
		// Limits the bending joint of the limb to 90 degrees from the default 90 degrees of bend direction
		private void LimitBend() {
			if (!validated) return;
			
			Vector3 normalDirection = bone1.rotation * -sampledDirectionInBone1Space;
			
			Vector3 axis2 = bone3.position - bone2.position;
			
			// Clamp the direction from knee/elbow to foot/hand to valid range (90 degrees from right-angledly bent limb)
			bool changed = false;
			Vector3 clampedAxis2 = ClampDirection(axis2, normalDirection, clampF, 0, out changed);
			
			Quaternion bone3Rotation = bone3.rotation;
			
			if (changed) {
				Quaternion f = Quaternion.FromToRotation(axis2, clampedAxis2); 
				bone2.rotation = f * bone2.rotation;
			}
			
			// Rotating bend direction to normal when the limb is stretched out
			if (positionWeight > 0f) {
				Vector3 normal = bone2.position - bone1.position;
				Vector3 tangent = bone3.position - bone2.position;
				
				Vector3.OrthoNormalize(ref normal, ref tangent);
				Quaternion q = Quaternion.FromToRotation(tangent, normalDirection);
				
				bone2.rotation = Quaternion.Lerp(bone2.rotation, q * bone2.rotation, positionWeight);
			}
			
			if (changed || positionWeight > 0f) bone3.rotation = bone3Rotation;
		}
		
		// Calculates the bend direction based on the Law of Cosines.
		private Vector3 GetBendDirection(Vector3 IKPosition, Vector3 bendNormal) {
			Vector3 direction = IKPosition - bone1.position;
			if (direction == Vector3.zero) return Vector3.zero;
			
			float directionSqrMag = direction.sqrMagnitude;
			float directionMagnitude = (float)Math.Sqrt(directionSqrMag);
			
			float x = (directionSqrMag + sqrMag1 - sqrMag2) / 2f / directionMagnitude;
			float y = (float)Math.Sqrt(Mathf.Clamp(sqrMag1 - x * x, 0, Mathf.Infinity));
			
			Vector3 yDirection = Vector3.Cross(direction, bendNormal);
			return Quaternion.LookRotation(direction, yDirection) * new Vector3(0f, y, x);
		}
		
		// Make sure the bones are in valid hierarchy
		private bool HierarchyIsValid() {
			if (bone3 == bone2.parent) return false;
			if (bone2 == bone1.parent) return false;
			if (bone1 == bone3.parent) return false;
			
			if (!IsAncestor(bone2, bone1)) return false;
			if (!IsAncestor(bone3, bone2)) return false;
			
			return true;
		}
		
		// Determines whether the second Transform is an ancestor to the first Transform.
		private static bool IsAncestor(Transform transform, Transform ancestor) {
			if (transform == null) return true;
			if (ancestor == null) return true;
			if (transform.parent == null) return false;
			if (transform.parent == ancestor) return true;
			return IsAncestor(transform.parent, ancestor);
		}
		
		// Clamps the direction to clampWeight from normalDirection, clampSmoothing is the number of sine smoothing iterations applied on the result.
		private static Vector3 ClampDirection(Vector3 direction, Vector3 normalDirection, float clampWeight, int clampSmoothing, out bool changed) {
			changed = false;
			
			if (clampWeight <= 0) return direction;
			
			if (clampWeight >= 1f) {
				changed = true;
				return normalDirection;
			}
			
			// Getting the angle between direction and normalDirection
			float angle = Vector3.Angle(normalDirection, direction);
			float dot = 1f - (angle / 180f);
			
			if (dot > clampWeight) return direction;
			changed = true;
			
			// Clamping the target
			float targetClampMlp = clampWeight > 0? Mathf.Clamp(1f - ((clampWeight - dot) / (1f - dot)), 0f, 1f): 1f;
			
			// Calculating the clamp multiplier
			float clampMlp = clampWeight > 0? Mathf.Clamp(dot / clampWeight, 0f, 1f): 1f;
			
			// Sine smoothing iterations
			for (int i = 0; i < clampSmoothing; i++) {
				float sinF = clampMlp * Mathf.PI * 0.5f;
				clampMlp = Mathf.Sin(sinF);
			}
			
			// Slerping the direction (don't use Lerp here, it breaks it)
			return Vector3.Slerp(normalDirection, direction, clampMlp * targetClampMlp);
		}
		
		// Ortho-Normalize a vector to the first bone direction
		private Vector3 OrthoToBone1(Vector3 tangent) {
			Vector3 normal = bone2.position - bone1.position;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			return tangent;
		}
		
		// Ortho-Normalize a vector to the chain direction
		private Vector3 OrthoToLimb(Vector3 tangent) {
			Vector3 normal = bone3.position - bone1.position;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			return tangent;
		}
	}
}
