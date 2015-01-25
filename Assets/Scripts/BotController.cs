using UnityEngine;
using System.Collections;

public class BotController : MonoBehaviour {

	// This script controls all aspect of bot AI, including movement and shooting.

	// This script is only for "my bot" -- in other words, only the "local" client will have this
	// enabled.  In practice, this means the MASTER client -- which is probably responsible for
	// spawning bots.
	// REMOTE bots will have this script disabled.

	NetworkCharacterBiped netChar;
	static Waypoint[] waypoints;

	Waypoint destination;
	float waypointTargetDistance = 1f;

	public float aggroRange = 40f;
	float distToWP;

	TeamMember myTarget = null;
	float targettingCooldown = 0;
	float targAngleCriteria = 10f; // The angle at which our target needs to be for us to start spraying bullets
	float targInnaccuracy = 2f; // Extra innaccuray to simulate mouse hand shake or something

	public NavMeshAgent agentMonster;
	public Vector3 targetWaypoint;
	private enum AIMode {NextWP,Patrolling,Hunting};
	AIMode agentMode;

	// Use this for initialization
	void Start () {
		netChar = GetComponent<NetworkCharacterBiped>();

		if(waypoints == null) {
			waypoints = GameObject.FindObjectsOfType<Waypoint>();
		}
		agentMode = AIMode.NextWP;
		agentMonster = GetComponent<NavMeshAgent>();
		targetWaypoint = waypoints[Random.Range(0,waypoints.Length)].transform.position;

		//destination = GetClosestWaypoint();
	}

	// Update is called once per frame
	void Update () {
		targettingCooldown -= Time.deltaTime;
		if(targettingCooldown <= 0) {
			DoTargetting();
			targettingCooldown = 0.5f;
		}

		//Check distance to WP if patrolling
		if (agentMode == AIMode.Patrolling)
			distToWP = Vector3.Distance(transform.position,targetWaypoint);

		//Check distance to current WP
		if (distToWP < 1.0 && agentMode == AIMode.Patrolling) {
			agentMode = AIMode.NextWP;
		}

		//Get new WP
		if (agentMode == AIMode.NextWP) {
			SetPatrol();
			agentMode = AIMode.Patrolling;
		}

		if (agentMode == AIMode.Hunting) {
			StrikePlayer();
		}

		/*
		DoDestination();
		DoDirection();
		DoRotation();
		DoFire();
		*/
	}

	void SetPatrol()
	{
		targetWaypoint = waypoints[Random.Range(0,waypoints.Length)].transform.position;
		agentMonster.SetDestination(targetWaypoint);
		return;
	}

	void SetQuarry()
	{
		agentMonster.SetDestination(myTarget.transform.position);
		return;
	}

	void DoTargetting() {

		// Do we have an enemy target in range?
		TeamMember closest = null;
		float dist = 0;
		foreach(TeamMember tm in GameObject.FindObjectsOfType<TeamMember>()) {	// WARNING: SLOW!
			if(tm == GetComponent<TeamMember>()) {
				// How Zen! We found ourselves.
				// Loop to the next possible target!
				continue;
			}

			if(tm.teamID==0 || tm.teamID != GetComponent<TeamMember>().teamID) {
				// Target is on the enemy team!
				float d = Vector3.Distance(tm.transform.position, transform.position);
				if( d <= aggroRange ) {
					// Target is in range!

					// TODO: Do a raycast to make sure we actually have line of sight!

					// Is the target closer than the last target we found?
					if(closest==null || d < dist) {
						closest = tm;
						dist = d;
					}
				}
			}
		}

		myTarget = closest;
		if (myTarget != null) {
			SetQuarry();
			agentMode = AIMode.Hunting;
		}
	}

	void StrikePlayer() {
		float dist = Vector3.Distance(transform.position,myTarget.transform.position);
		if (dist < 1.2) {
			print ("Should take damage");
			Health h = myTarget.transform.GetComponent<Health>();

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

					TeamMember tm = myTarget.GetComponent<TeamMember>();
					TeamMember myTm = this.GetComponent<TeamMember>();

					if(tm==null || tm.teamID==0 || myTm==null || myTm.teamID==0 || tm.teamID != myTm.teamID ) {
						h.GetComponent<PhotonView>().RPC ("TakeDamage", PhotonTargets.AllBuffered, 100f);
					}
				}

			}
		//go back to patrolling
		agentMode = AIMode.NextWP;
		}
	}

	/*
	Waypoint GetClosestWaypoint() {
		Waypoint closest = null;
		float dist = 0;

		foreach(Waypoint w in waypoints) {
			if(closest==null || Vector3.Distance(transform.position, w.transform.position) < dist) {
				closest = w;
				dist = Vector3.Distance(transform.position, w.transform.position);
			}
		}

		return closest;
	}

	void DoDestination() {
		if(destination != null) {
			// We have a destination -- let's check if we have arrived.
			if( Vector3.Distance(destination.transform.position, transform.position) <= waypointTargetDistance ) {
				// We have arrived!

				if(destination.connectWPs != null && destination.connectWPs.Length > 0) {
					// Pick a connected waypoint
					destination = destination.connectWPs[ Random.Range(0, destination.connectWPs.Length) ];
				}
				else {
					// Waypoint isn't connected to anything, which is kind of a problem.
					// We need proper navmesh type stuff!
				}

			}
		}

	}

	void DoDirection() {
		// We STILL have a destination, so let's move towards it.
		if(destination != null) {
			netChar.direction = destination.transform.position - transform.position;
			netChar.direction.y = 0;
			netChar.direction.Normalize();

		}
		else {
			// No destination, so let's just stop and be idle.
			netChar.direction = Vector3.zero;
		}

	}

	void DoRotation() {
		// Let's figure out where we should be facing!
		// By default: Look where we're going.
		Vector3 lookDirection = netChar.direction;

		if(myTarget != null) {
			// We have a target, so let's use that direction as our look direction!
			lookDirection = myTarget.transform.position - transform.position;
		}

		// Rotate towards our look direction
		Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
		lookRotation.eulerAngles = new Vector3(0, lookRotation.eulerAngles.y, 0);
		transform.rotation = lookRotation;


		// Now we adjust our aimAngle for animations.
		if(myTarget != null) {
			// Figure out the relative vertical angle to our target and adjust aimAngle
			Vector3 localLookDirection = transform.InverseTransformPoint(myTarget.transform.position);
			float targetAimAngle = Mathf.Atan2(localLookDirection.y, localLookDirection.z) * Mathf.Rad2Deg;
			netChar.aimAngle = targetAimAngle;
		}
		else {
			// We don't have a target, just aim casual
			netChar.aimAngle = 0;
		}

	}

	void DoFire() {
		if(myTarget == null)
			return;

		// Ignore vertical height for determining if we should shoot.
		Vector3 targetPos = myTarget.transform.position;
		targetPos.y = transform.position.y;

		if( Vector3.Angle(transform.forward, targetPos - transform.position ) < targAngleCriteria ) {


			// First, get our fire direction in local space
			Vector3 fireDir = Quaternion.Euler(-netChar.aimAngle, 0, 0) * new Vector3(0,0,1);

			// Add hand shake to make the bot less accurate

			//Vector3 innaccAngle = new Vector3( Random.Range(-targInnaccuracy, targInnaccuracy), Random.Range(-targInnaccuracy, targInnaccuracy), 0 );
			//fireDir = Quaternion.Euler(innaccAngle) * fireDir;

			//Debug.Log (fireDir);
			// Convert to global space
			fireDir = transform.TransformDirection(fireDir);

			netChar.FireWeapon( transform.position + transform.up * 1.5f , fireDir );
		}

	}
	*/

}
