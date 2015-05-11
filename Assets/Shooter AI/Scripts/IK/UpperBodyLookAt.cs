using UnityEngine;
using System.Collections;

namespace GatewayGames.ShooterAI {

	/// <summary>
	/// Rotating the upper body bones (spine, neck and/or head) to face camera forward, imitate recoil and to help with holding the weapon.
	/// </summary>
	public class UpperBodyLookAt : MonoBehaviour {

		/// <summary>
		/// Spine/head bone that rotates along with the camera and can be rotated to imitate recoil and to help with holding the weapon.
		/// </summary>
		[System.Serializable]
		public class Bone {
			/// <summary>
			/// Reference to the spine/neck/head bone.
			/// </summary>
			[Tooltip("Reference to the spine/neck/head bone")]
			public Transform transform;

			/// <summary>
			/// Local axis of the bone facing forward.
			/// </summary>
			[Tooltip("Local axis of the bone facing forward")]
			public Vector3 forwardAxis;

			/// <summary>
			/// The weight of rotating this bone's forward to face the transform.forward of the "Forward".
			/// </summary>
			[Tooltip("The weight of rotating this bone's forward to face the transform.forward of the 'Forward'")]
			[Range(0f, 1f)] public float lookAtWeight;

			/// <summary>
			/// Euler angles offset applied to the localRotation of the bone.
			/// </summary>
			[Tooltip("Euler angles offset applied to the localRotation of the bone.")]
			public Vector3 localRotationOffset;

			/// <summary>
			/// Euler angles offset applied to the localRotation of the bone based on the amount of current recoil offset.
			/// </summary>
			[Tooltip("Euler angles offset applied to the localRotation of the bone based on the amount of current recoil offset")]
			public Vector3 recoilRotationOffset;

			[HideInInspector] public Rigidbody rigidbody;
		}

		/// <summary>
		/// Euler angles offset applied to the localRotation of the bone based on the amount of current recoil offset.
		/// </summary>
		[Tooltip("Euler angles offset applied to the localRotation of the bone based on the amount of current recoil offset")]
		public Transform forward;

		/// <summary>
		/// Reference to the WeaponControl component, used only for recoil.
		/// </summary>
		[Tooltip("Reference to the GatewayGamesWeaponManager component, used only for recoil")]
		public GatewayGamesWeaponManager weaponManager;

		/// <summary>
		/// Weight of this effect while the character is in normal stance.
		/// </summary>
		[Tooltip("Weight of this effect while the character is in normal stance")]
		[Range(0f, 1f)] public float normalWeight;

		/// <summary>
		/// Weight of this effect while the character is in engaged stance.
		/// </summary>
		[Tooltip("Weight of this effect while the character is in engaged stance")]
		[Range(0f, 1f)] public float engagedWeight = 1f;

		/// <summary>
		/// Weight of this effect while the character is in melee stance.
		/// </summary>
		[Tooltip("Weight of this effect while the character is in melee stance")]
		[Range(0f, 1f)] public float meleeWeight;

		/// <summary>
		/// The list of spine/neck/head bones.
		/// </summary>
		[Tooltip("The list of spine/neck/head bones")]
		public Bone[] bones;

		void Start() {
			foreach (Bone bone in bones) {
				if (bone.transform != null) bone.rigidbody = bone.transform.GetComponent<Rigidbody>();
			}
		}

		void LateUpdate() {
			// Check for null references
			if (forward == null) {
				Debug.LogWarning("No 'Forward' Transform assigned in UpperBodyLookAt", transform);
				return;
			}

			// Calculate the weight
			float recoilAngle = Quaternion.Angle(Quaternion.identity, weaponManager.recoilOffset);
			float w = Mathf.Lerp(normalWeight, engagedWeight, weaponManager.engageFactor);
			w = Mathf.Lerp(w, meleeWeight, weaponManager.arbitraryFactor);

			if (w <= 0f) return;

			// Return if we are in ragdoll
			foreach (Bone bone in bones) {
				if (bone.rigidbody != null && !bone.rigidbody.isKinematic) return;
			}

			foreach (Bone bone in bones) {
				// Check for null references
				if (bone.transform == null) {
					Debug.LogWarning("Bone Transform is null in UpperBodyLookAt", transform);
					return;
				}

				// Look At target forward
				bone.transform.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(bone.transform.rotation * bone.forwardAxis, forward.forward), bone.lookAtWeight * w) * bone.transform.rotation;

				// LocalRotation offset
				if (bone.localRotationOffset != Vector3.zero) {
					bone.transform.localRotation = Quaternion.Euler(bone.localRotationOffset * w) * bone.transform.localRotation;
				}

				// Recoil
				if (bone.recoilRotationOffset != Vector3.zero) {
					bone.transform.localRotation = Quaternion.AngleAxis(recoilAngle * w, bone.recoilRotationOffset) * bone.transform.localRotation;
				}
			}
		}
	}
}
