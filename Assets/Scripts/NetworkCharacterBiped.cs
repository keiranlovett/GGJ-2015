using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkCharacterBiped : Photon.MonoBehaviour {

	// This script is responsible for actually moving a character.
	// For local character, we read things like "direction" and "isJumping"
	// and then affect the character controller.
	// For remote characters, we skip that and simply update the raw transform
	// position based on info we received over the network.


	// NOTE! Only our local character will effectively use this.
	// Remove character will just give us absolute positions.
	public float speed = 10f;		// The speed at which I run
	public float jumpSpeed = 6f;	// How much power we put into our jump. Change this to jump higher.

	// Bookeeping variables
	[System.NonSerialized]
	public Vector3 direction = Vector3.zero;	// forward/back & left/right
	[System.NonSerialized]
	public bool isJumping = false;
	[System.NonSerialized]
	public float aimAngle = 0;
    public float DropOffset = 5f;
	float   verticalVelocity = 0;		// up/down

	Vector3 realPosition = Vector3.zero;
	Quaternion realRotation = Quaternion.identity;
	float realAimAngle = 0;

	Animator anim;

	bool gotFirstUpdate = false;

	CharacterController cc;

	// Shooting Stuff
	FXManager fxManager;
	WeaponData weaponData;
	float cooldown = 0;

	// Use this for initialization
	void Start () {
		CacheComponents();
	}

	void CacheComponents() {
		if(anim == null) {
			anim = GetComponent<Animator>();
			if(anim == null) {
				Debug.LogError ("ZOMG, you forgot to put an Animator component on this character prefab!");
			}

			cc = GetComponent<CharacterController>();
			if(cc == null) {
				Debug.LogError("No character controller!");
			}

			fxManager = GameObject.FindObjectOfType<FXManager>();

			if(fxManager == null) {
				Debug.LogError("Couldn't find an FXManager.");
			}
		}

		// Cache more components here if required!
	}

	void Update() {
		cooldown -= Time.deltaTime;
	}

	// FixedUpdate is called once per physics loop
	// Do all MOVEMENT and other physics stuff here.
	void FixedUpdate () {
		if( photonView.isMine ) {
			// Do nothing -- the character motor/input/etc... is moving us
			DoLocalMovement();
			DoItemManagement();

		}
		else {
			transform.position = Vector3.Lerp(transform.position, realPosition, 0.1f);
			transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.1f);
			anim.SetFloat("AimAngle", Mathf.Lerp(anim.GetFloat("AimAngle"), realAimAngle, 0.1f ) );
		}
	}


	void DoItemManagement() {
 		if (Input.GetKeyDown("space"))
	 		foreach (PickupItem item in PickupItem.DisabledPickupItems)
	            {
	                if (item.PickupIsMine && item.SecondsBeforeRespawn <= 0)
	                {

	                   //	item.Drop();    // drops the item at the place where it originates


	                   // GameObject playerCharGo = PhotonNetwork.player.TagObject as GameObject;
	                  //  if (playerCharGo != null && GUILayout.Button("Drop here " + item.name))
	                   // {
	                        // drop the item at some other place. next to the user's character would be fine...
	                        //Vector3 random = Random.insideUnitSphere;
	                        //random.y = 0;
	                        //random = random.normalized;
	                        Vector3 itempos = new Vector3(-58,1,6);

	                        item.Drop(itempos);
	                 //   }
	                }
	            }

	}

	void DoLocalMovement () {

		// "direction" is the desired movement direction, based on our player's input
		Vector3 dist = direction * speed * Time.deltaTime;

		if(isJumping) {
			isJumping = false;
			if(cc.isGrounded) {
				verticalVelocity = jumpSpeed;
			}
		}

		if(cc.isGrounded && verticalVelocity < 0) {
			// We are currently on the ground and vertical velocity is
			// not positive (i.e. we are not starting a jump).

			// Ensure that we aren't playing the jumping animation
			anim.SetBool("Jumping", false);

			// Set our vertical velocity to *almost* zero. This ensures that:
			//   a) We don't start falling at warp speed if we fall off a cliff (by being close to zero)
			//   b) cc.isGrounded returns true every frame (by still being slightly negative, as opposed to zero)
			verticalVelocity = Physics.gravity.y * Time.deltaTime;
		}
		else {
			// We are either not grounded, or we have a positive verticalVelocity (i.e. we ARE starting a jump)

			// To make sure we don't go into the jump animation while walking down a slope, make sure that
			// verticalVelocity is above some arbitrary threshold before triggering the animation.
			// 75% of "jumpSpeed" seems like a good safe number, but could be a standalone public variable too.
			//
			// Another option would be to do a raycast down and start the jump/fall animation whenever we were
			// more than ___ distance above the ground.
			if(Mathf.Abs(verticalVelocity) > jumpSpeed*0.75f) {
				anim.SetBool("Jumping", true);
			}

			// Apply gravity.
			verticalVelocity += Physics.gravity.y * Time.deltaTime;
		}

		// Add our verticalVelocity to our actual movement for this frame
		dist.y = verticalVelocity * Time.deltaTime;

		// Adjust our aim angle animation
		anim.SetFloat("AimAngle", aimAngle);

		// Set our animation "Speed" parameter. This will move us from "idle" to "run" animations,
		// but we could also use this to blend between "walk" and "run" as well.
		anim.SetFloat("Speed", direction.magnitude);

		// Apply the movement to our character controller (which handles collisions for us)
		cc.Move( dist );
	}





	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		CacheComponents();

		if(stream.isWriting) {
			// This is OUR player. We need to send our actual position to the network.

			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(anim.GetFloat("Speed"));
			stream.SendNext(anim.GetBool("Jumping"));
			stream.SendNext(anim.GetFloat("AimAngle"));
		}
		else {
			// This is someone else's player. We need to receive their position (as of a few
			// millisecond ago, and update our version of that player.

			// Right now, "realPosition" holds the other person's position at the LAST frame.
			// Instead of simply updating "realPosition" and continuing to lerp,
			// we MAY want to set our transform.position to immediately to this old "realPosition"
			// and then update realPosition


			realPosition = (Vector3)stream.ReceiveNext();
			realRotation = (Quaternion)stream.ReceiveNext();
			anim.SetFloat("Speed", (float)stream.ReceiveNext());
			anim.SetBool("Jumping", (bool)stream.ReceiveNext());
			realAimAngle = (float)stream.ReceiveNext();

			if(gotFirstUpdate == false) {
				transform.position = realPosition;
				transform.rotation = realRotation;
				anim.SetFloat("AimAngle", realAimAngle );
				gotFirstUpdate = true;
			}

		}

	}

	public void FireWeapon(Vector3 orig, Vector3 dir) {
		if(weaponData==null) {
			weaponData = gameObject.GetComponentInChildren<WeaponData>();
			if(weaponData==null) {
				Debug.LogError("Did not find any WeaponData in our children!");
				return;
			}
		}

		if(cooldown > 0) {
			return;
		}

		Debug.Log ("Firing our gun!");

		Ray ray = new Ray(orig, dir);
		Transform hitTransform;
		Vector3   hitPoint;

		hitTransform = FindClosestHitObject(ray, out hitPoint);

		if(hitTransform != null) {
			Debug.Log ("We hit: " + hitTransform.name);

			// We could do a special effect at the hit location
			// DoRicochetEffectAt( hitPoint );

			Health h = hitTransform.GetComponent<Health>();

			while(h == null && hitTransform.parent) {
				hitTransform = hitTransform.parent;
				h = hitTransform.GetComponent<Health>();
			}

			// Once we reach here, hitTransform may not be the hitTransform we started with!

			if(h != null) {
				// This next line is the equivalent of calling:
				//    				h.TakeDamage( damage );
				// Except more "networky"
				PhotonView pv = h.GetComponent<PhotonView>();
				if(pv==null) {
					Debug.LogError("Freak out!");
				}
				else {

					TeamMember tm = hitTransform.GetComponent<TeamMember>();
					TeamMember myTm = this.GetComponent<TeamMember>();

					if(tm==null || tm.teamID==0 || myTm==null || myTm.teamID==0 || tm.teamID != myTm.teamID ) {
						h.GetComponent<PhotonView>().RPC ("TakeDamage", PhotonTargets.AllBuffered, weaponData.damage);
					}
				}

			}

			if(fxManager != null) {

				DoGunFX(hitPoint);
			}
		}
		else {
			// We didn't hit anything (except empty space), but let's do a visual FX anyway
			if(fxManager != null) {
				hitPoint = Camera.main.transform.position + (Camera.main.transform.forward*100f);
				DoGunFX(hitPoint);
			}

		}

		cooldown = weaponData.fireRate;
	}

	void DoGunFX(Vector3 hitPoint) {
		fxManager.GetComponent<PhotonView>().RPC ("SniperBulletFX", PhotonTargets.All, weaponData.transform.position, hitPoint);
	}

	Transform FindClosestHitObject(Ray ray, out Vector3 hitPoint) {

		RaycastHit[] hits = Physics.RaycastAll(ray);

		Transform closestHit = null;
		float distance = 0;
		hitPoint = Vector3.zero;

		foreach(RaycastHit hit in hits) {
			if(hit.transform != this.transform && ( closestHit==null || hit.distance < distance ) ) {
				// We have hit something that is:
				// a) not us
				// b) the first thing we hit (that is not us)
				// c) or, if not b, is at least closer than the previous closest thing

				closestHit = hit.transform;
				distance = hit.distance;
				hitPoint = hit.point;
			}
		}

		// closestHit is now either still null (i.e. we hit nothing) OR it contains the closest thing that is a valid thing to hit

		return closestHit;

	}

}
