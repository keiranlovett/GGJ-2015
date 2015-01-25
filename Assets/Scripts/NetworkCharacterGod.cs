using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/RtsCamera-Keyboard")]
public class NetworkCharacterGod : Photon.MonoBehaviour {

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

	float   verticalVelocity = 0;		// up/down

	Vector3 realPosition = Vector3.zero;
	Quaternion realRotation = Quaternion.identity;
	float realAimAngle = 0;

	bool gotFirstUpdate = false;

	// Shooting Stuff
	private int _currWalkerIndex = 0;


	// Camera Key Commands
 	public bool AllowMove;
    public float MoveSpeed;

    public bool AllowFastMove;
    public float FastMoveSpeed;
    public KeyCode FastMoveKeyCode1;
    public KeyCode FastMoveKeyCode2;

    public bool AllowRotate;
    public float RotateSpeed;

    public bool AllowZoom;
    public float ZoomSpeed;

    public bool AllowTilt;
    public float TiltSpeed;

    public KeyCode ResetKey;
    public bool IncludePositionOnReset;

    public bool MovementBreaksFollow;

    public string HorizontalInputAxis = "Horizontal";
    public string VerticalInputAxis = "Vertical";

    public bool RotateUsesInputAxis = false;
    public string RotateInputAxis = "KbCameraRotate";
    public KeyCode RotateLeftKey = KeyCode.Q;
    public KeyCode RotateRightKey = KeyCode.E;

    public bool ZoomUsesInputAxis = false;
    public string ZoomInputAxis = "KbCameraZoom";
    public KeyCode ZoomOutKey = KeyCode.Z;
    public KeyCode ZoomInKey = KeyCode.X;

    public bool TiltUsesInputAxis = false;
    public string TiltInputAxis = "KbCameraTilt";
    public KeyCode TiltUpKey = KeyCode.R;
    public KeyCode TiltDownKey = KeyCode.F;


    // Camera Mosue Commands
    public KeyCode MouseOrbitButton;

    public bool AllowScreenEdgeMove;
    public bool ScreenEdgeMoveBreaksFollow;
    public int ScreenEdgeBorderWidth;
    public float MoveSpeedCamera;

    public bool AllowPan;
    public bool PanBreaksFollow;
    public float PanSpeed;

    public bool AllowRotateCamera;
    public float RotateSpeedCamera;

    public bool AllowTiltCamera;
    public float TiltSpeedCamera;

    public bool AllowZoomCamera;
    public float ZoomSpeedCamera;

    public string RotateInputAxisCamera = "Mouse X";
    public string TiltInputAxisCamera = "Mouse Y";
    public string ZoomInputAxisCamera = "Mouse ScrollWheel";
    public KeyCode PanKey1 = KeyCode.LeftShift;
    public KeyCode PanKey2 = KeyCode.RightShift;

    //

    private RtsCamera _rtsCamera;

    //

    protected void Reset()
    {
        AllowMove = true;
        MoveSpeed = 20f;

        AllowFastMove = true;
        FastMoveSpeed = 40f;
        FastMoveKeyCode1 = KeyCode.LeftShift;
        FastMoveKeyCode2 = KeyCode.RightShift;

        AllowRotate = true;
        RotateSpeed = 180f;

        AllowZoom = true;
        ZoomSpeed = 20f;

        AllowTilt = true;
        TiltSpeed = 90f;

        ResetKey = KeyCode.C;
        IncludePositionOnReset = false;

        MovementBreaksFollow = true;

        /////

        MouseOrbitButton = KeyCode.Mouse2;    // middle mouse by default (probably should not use right mouse since it doesn't work well in browsers)

        AllowScreenEdgeMove = true;
        ScreenEdgeMoveBreaksFollow = true;
        ScreenEdgeBorderWidth = 4;
        MoveSpeedCamera = 30f;

        AllowPan = true;
        PanBreaksFollow = true;
        PanSpeed = 50f;
        PanKey1 = KeyCode.LeftShift;
        PanKey2 = KeyCode.RightShift;

        AllowRotateCamera = true;
        RotateSpeedCamera = 360f;

        AllowTiltCamera = true;
        TiltSpeedCamera = 200f;

        AllowZoomCamera = true;
        ZoomSpeedCamera = 500f;

        RotateInputAxisCamera = "Mouse X";
        TiltInputAxisCamera = "Mouse Y";
        ZoomInputAxisCamera = "Mouse ScrollWheel";
    }

    public void TargetRandomWorker()
    {
        if (_rtsCamera == null)
            return; // no camera, bail!

        var walkers = GameObject.FindGameObjectsWithTag("Player");
        if (walkers != null && walkers.Length > 0)
        {
            _currWalkerIndex++;
            if (_currWalkerIndex >= walkers.Length)
                _currWalkerIndex = 0;

            var walker = walkers[_currWalkerIndex];
            if (walker != null)
            {
                _rtsCamera.Follow(walker);
            }
        }
    }

	// FixedUpdate is called once per physics loop
	// Do all MOVEMENT and other physics stuff here.
	void FixedUpdate () {
		if( photonView.isMine ) {
			// Do nothing -- the character motor/input/etc... is moving us
			DoLocalMovement();
		}
		else {
			transform.position = Vector3.Lerp(transform.position, realPosition, 0.1f);
			transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.1f);
		}
	}

	void DoLocalMovement () {

		if (Input.GetKeyDown(KeyCode.Tab))
        {
            TargetRandomWorker();
        }

		gameObject.GetComponent<Camera>().enabled = true;
		_rtsCamera = gameObject.GetComponent<RtsCamera>();
		_rtsCamera.enabled = true;

		if (_rtsCamera == null)
            return; // no camera, bail!

        if (AllowMove && (!_rtsCamera.IsFollowing || MovementBreaksFollow))
        {
            var hasMovement = false;

            var speed = MoveSpeed;
            if (AllowFastMove && (Input.GetKey(FastMoveKeyCode1) || Input.GetKey(FastMoveKeyCode2)))
            {
                speed = FastMoveSpeed;
            }

            var h = Input.GetAxisRaw(HorizontalInputAxis);
            if (Mathf.Abs(h) > 0.001f)
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(h * speed * Time.deltaTime, 0, 0);
            }

            var v = Input.GetAxisRaw(VerticalInputAxis);
            if (Mathf.Abs(v) > 0.001f)
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(0, 0, v * speed * Time.deltaTime);
            }

            if (hasMovement && _rtsCamera.IsFollowing && MovementBreaksFollow)
                _rtsCamera.EndFollow();
        }

        if (AllowRotate)
        {
            if (RotateUsesInputAxis)
            {
                var rot = Input.GetAxisRaw(RotateInputAxis);
                if (Mathf.Abs(rot) > 0.001f)
                {
                    _rtsCamera.Rotation += rot * RotateSpeed * Time.deltaTime;
                }
            }
            else
            {
                if (Input.GetKey(RotateLeftKey))
                {
                    _rtsCamera.Rotation += RotateSpeed * Time.deltaTime;
                }
                if (Input.GetKey(RotateRightKey))
                {
                    _rtsCamera.Rotation -= RotateSpeed * Time.deltaTime;
                }
            }
        }

        if (AllowZoom)
        {
            if (ZoomUsesInputAxis)
            {
                var zoom = Input.GetAxisRaw(ZoomInputAxis);
                if (Mathf.Abs(zoom) > 0.001f)
                {
                    _rtsCamera.Distance += zoom * ZoomSpeed * Time.deltaTime;
                }
            }
            else
            {
                if (Input.GetKey(ZoomOutKey))
                {
                    _rtsCamera.Distance += ZoomSpeed * Time.deltaTime;
                }
                if (Input.GetKey(ZoomInKey))
                {
                    _rtsCamera.Distance -= ZoomSpeed * Time.deltaTime;
                }
            }
        }

        if (AllowTilt)
        {
            if (TiltUsesInputAxis)
            {
                var tilt = Input.GetAxisRaw(TiltInputAxis);
                if (Mathf.Abs(tilt) > 0.001f)
                {
                    _rtsCamera.Tilt += tilt * TiltSpeed * Time.deltaTime;
                }
            }
            else
            {
                if (Input.GetKey(TiltUpKey))
                {
                    _rtsCamera.Tilt += TiltSpeed * Time.deltaTime;
                }
                if (Input.GetKey(TiltDownKey))
                {
                    _rtsCamera.Tilt -= TiltSpeed * Time.deltaTime;
                }
            }
        }

        //

        if (ResetKey != KeyCode.None)
        {
            if (Input.GetKeyDown(ResetKey))
            {
                _rtsCamera.ResetToInitialValues(IncludePositionOnReset, false);
            }
        }




        ///////



        if (AllowZoomCamera)
        {
            var scroll = Input.GetAxisRaw(ZoomInputAxisCamera);
            _rtsCamera.Distance -= scroll * ZoomSpeedCamera * Time.deltaTime;
        }

        if (Input.GetKey(MouseOrbitButton))
        {
            if (AllowPan && (Input.GetKey(PanKey1) || Input.GetKey(PanKey2)))
            {
                // pan
                var panX = -1 * Input.GetAxisRaw("Mouse X") * PanSpeed * Time.deltaTime;
                var panZ = -1 * Input.GetAxisRaw("Mouse Y") * PanSpeed * Time.deltaTime;

                _rtsCamera.AddToPosition(panX, 0, panZ);

                if (PanBreaksFollow && (Mathf.Abs(panX) > 0.001f || Mathf.Abs(panZ) > 0.001f))
                {
                    _rtsCamera.EndFollow();
                }
            }
            else
            {
                // orbit

                if (AllowTiltCamera)
                {
                    var tilt = Input.GetAxisRaw(TiltInputAxisCamera);
                    _rtsCamera.Tilt -= tilt * TiltSpeedCamera * Time.deltaTime;
                }

                if (AllowRotateCamera)
                {
                    var rot = Input.GetAxisRaw(RotateInputAxisCamera);
                    _rtsCamera.Rotation += rot * RotateSpeedCamera * Time.deltaTime;
                }
            }
        }

        if (AllowScreenEdgeMove && (!_rtsCamera.IsFollowing || ScreenEdgeMoveBreaksFollow))
        {
            var hasMovement = false;

            if (Input.mousePosition.y > (Screen.height - ScreenEdgeBorderWidth))
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(0, 0, MoveSpeedCamera * Time.deltaTime);
            }
            else if (Input.mousePosition.y < ScreenEdgeBorderWidth)
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(0, 0, -1 * MoveSpeedCamera * Time.deltaTime);
            }

            if (Input.mousePosition.x > (Screen.width - ScreenEdgeBorderWidth))
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(MoveSpeedCamera * Time.deltaTime, 0, 0);
            }
            else if (Input.mousePosition.x < ScreenEdgeBorderWidth)
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(-1 * MoveSpeedCamera * Time.deltaTime, 0, 0);
            }

            if (hasMovement && _rtsCamera.IsFollowing && ScreenEdgeMoveBreaksFollow)
            {
                _rtsCamera.EndFollow();
            }
        }

	}





	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {

		if(stream.isWriting) {
			// This is OUR player. We need to send our actual position to the network.

			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
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

			realAimAngle = (float)stream.ReceiveNext();

			if(gotFirstUpdate == false) {
				transform.position = realPosition;
				transform.rotation = realRotation;
				gotFirstUpdate = true;
			}

		}

	}

	/*public void FireWeapon(Vector3 orig, Vector3 dir) {
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
	}*/

	/*void DoGunFX(Vector3 hitPoint) {
		fxManager.GetComponent<PhotonView>().RPC ("SniperBulletFX", PhotonTargets.All, weaponData.transform.position, hitPoint);
	}*/

	/*Transform FindClosestHitObject(Ray ray, out Vector3 hitPoint) {

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

	}*/

}
