using UnityEngine;
using System.Collections;

public class WeaponControl : MonoBehaviour {

	public Transform weapon; //the weapon
	public Transform posNormal; //the normal pos
	public Transform posZoom; //the zoom pos
	public AudioClip bulletShot; //the bullet shot
	
	public float zoomFov = 20f; //the zoom fov
	
	public GameObject muzzleFlashObject; //the muzzleFlash object
	public Transform muzzleFlashPos; //the position of the muzzle flash
	
	
	private float recoilFactor = 0.05f; //the recoil factor
	
	private float initFov = 60f; //the init fov
	
	
	void Awake()
	{
		initFov = GetComponent<Camera>().fieldOfView;
	}
	
	
	
	void Update()
	{
		
		//zoom location control
		if( Input.GetButton("Fire2") )
		{
			weapon.transform.localPosition = Vector3.Lerp( weapon.transform.localPosition, posZoom.transform.localPosition, 0.1f);
			GetComponent<Camera>().fieldOfView = Mathf.Lerp( GetComponent<Camera>().fieldOfView, zoomFov, 0.1f );
		}
		else
		{
			weapon.transform.localPosition = Vector3.Lerp( weapon.transform.localPosition, posNormal.transform.localPosition, 0.1f);
			GetComponent<Camera>().fieldOfView = Mathf.Lerp( GetComponent<Camera>().fieldOfView, initFov, 0.1f );
		}
		
		
		//recoil + sound effects
		if( Input.GetButtonDown("Fire1") )
		{
			//recoid
			weapon.transform.localPosition += Random.insideUnitSphere * recoilFactor;
			
			//sound effects
			var newAudio = gameObject.AddComponent<AudioSource>();
			newAudio.PlayOneShot( bulletShot, 0.3f);
			Destroy( newAudio, bulletShot.length);
			
			//muzzle flash
			
			var mF = Instantiate( muzzleFlashObject, muzzleFlashPos.transform.position, muzzleFlashPos.transform.rotation ) as GameObject;
			mF.transform.parent = muzzleFlashPos;
			
			
		}
		
	}
	
	
}
