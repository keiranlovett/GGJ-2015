using UnityEngine;
using System.Collections;

namespace GatewayGames.ShooterAI {

	/// <summary>
	/// Adds the possibility to assign an arbitrary pivot transform to rotate the weapon around. 
	/// This is useful for connecting characters to the FPS weapon because in real world, the weapon is not rotated around the eyes (FPS camera), but the shoulder of the shooter.
	/// </summary>
	public class WeaponControlIK : WeaponControl {
		/*

		/// <summary>
		/// (optional) if assigned, will rotate the camera around this Pivot Transform, maintaining the initial offset. This should be an empty GameObject parented to one of the bones of the character, usually the right shoulder.
		/// </summary>
		[Tooltip("(optional) if assigned, will rotate the camera around this Pivot Transform, maintaining the initial offset. This should be an empty GameObject parented to one of the bones of the character, usually the right shoulder.")]
		public Transform weaponPivot;
		/// <summary>
		/// (optional) the normal position of the camera. If not assigned, will keep the default camera position
		/// </summary>
		[Tooltip("(optional) the normal position of the camera. If not assigned, will keep the default camera position")]
		public Transform camPosNormal;
		/// <summary>
		/// (optional) the zoom position of the camera. If not assigned, will keep the default camera position
		/// </summary>
		[Tooltip("(optional) the zoom position of the camera. If not assigned, will keep the default camera position")]
		public Transform camPosZoom;

		private Quaternion pivotRelativeToCamera;

		protected override void Awake() {
			base.Awake();

			// Remember the position of the camera relative to the pivot
			if (weaponPivot != null) {
				if (transform == weaponPivot) Debug.LogWarning("WeaponControl game object should not be assigned as the Weapon Pivot. Assign an empty GameObject parented to one of the character bones (usually the right shoulder) that you wish to pivot the weapon around.", transform);
				if (weaponPivot.parent == transform) Debug.LogWarning("The Weapon Pivot should not be parented to the camera. Parent it to the character bone that you wish to pivot the weapon around (usually the right shoulder).", transform);

				pivotRelativeToCamera = Quaternion.Inverse(transform.rotation) * weaponPivot.rotation;
			}

			if (camPosNormal.parent == transform) Debug.LogWarning("Cam Pos Normal should not be parented to the camera. This would make for a circular dependency.", transform);
			if (camPosZoom.parent == transform) Debug.LogWarning("Cam Pos Zoom should not be parented to the camera. This would make for a circular dependency.", transform);
		}

		protected override void LateUpdate() {
			// Rotate the weapon pivot with the camera
			if (weaponPivot != null) weaponPivot.rotation = transform.rotation * pivotRelativeToCamera;

			// LateUpdate the base WeaponControl.cs
			base.LateUpdate();

			// Position the camera
			if (camPosNormal != null && camPosZoom != null) {
				if (weaponPivot == null) {
					Debug.LogWarning("Cam Pos Normal and Cam Pos Zoom can only be used when Weapon Pivot is also assigned. Use an empty GameObject parented to one of the bones of the character (usually the right shoulder) as the Pivot and parent Pos Normal, Pos Zoom, Camera Pos Normal and Camera Pos Zoom to that Pivot.", transform);
				} else {
					transform.position = Vector3.Lerp(camPosNormal.position, camPosZoom.position, lerp);
				}
			}

		}
		*/
	}
}
