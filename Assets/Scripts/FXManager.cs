using UnityEngine;
using System.Collections;

public class FXManager : MonoBehaviour {

	public GameObject sniperBulletFXPrefab;

	[RPC]
	void SniperBulletFX( Vector3 startPos, Vector3 endPos ) {
		Debug.Log ("SniperBulletFX");

		if(sniperBulletFXPrefab != null) {
			GameObject sniperFX = (GameObject)Instantiate(sniperBulletFXPrefab, startPos, Quaternion.LookRotation( endPos - startPos ) );

			LineRenderer lr = sniperFX.transform.Find("LineFX").GetComponent<LineRenderer>();
			if(lr != null) {
				lr.SetPosition(0, startPos);
				lr.SetPosition(1, endPos);
			}
			else {
				Debug.LogError("sniperBulletFXPrefab's linerenderer is missing.");
			}
		}
		else {
			Debug.LogError("sniperBulletFXPrefab is missing!");
		}

	}

}
