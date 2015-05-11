using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace GatewayGames.ShooterAI
{

	/// <summary>
	/// Gateway games muzzle flash manager.
	/// </summary>
	public class GatewayGamesMuzzleFlashManager : MonoBehaviour 
	{
		public List<Transform> muzzleFlashes = new List<Transform>(); //muzzle flashes options
		public float lengthOfMuzzleFlash = 0.3f; //the length of the muzzle flash in seconds

		private Transform tempMuzzleFlash = null; //temp muzzle flash ref


		public void MuzzleFlash()
		{
			//instiate random muzzle flash
			tempMuzzleFlash = Instantiate( muzzleFlashes[ (int)Random.Range(0, muzzleFlashes.Count) ], transform.position, transform.rotation ) as Transform;

			//parent it
			tempMuzzleFlash.parent = transform;

			//destroy again aftet time
			StartCoroutine( DestroyMuzzleFlash( tempMuzzleFlash, lengthOfMuzzleFlash ) );

		}



		/// <summary>
		/// Destroys the muzzle flash after time.
		/// </summary>
		/// <returns>The muzzle flash.</returns>
		/// <param name="muzzleFlash">Muzzle flash.</param>
		/// <param name="time">Time.</param>
		private IEnumerator DestroyMuzzleFlash(Transform muzzleFlash, float time )
		{
			//wait
			yield return new WaitForSeconds( time);

			//destroy
			Destroy( muzzleFlash.gameObject);
		}

	}

}