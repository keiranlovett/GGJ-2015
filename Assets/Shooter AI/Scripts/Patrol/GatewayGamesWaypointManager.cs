//attach to each waypoint that the ai needs to go through

using UnityEngine;
using System.Collections;


namespace GatewayGames.ShooterAI
{

public class GatewayGamesWaypointManager : MonoBehaviour {
	
	
	
	private Vector3 keepPos; //to keep the position at the same spot
	private Quaternion keepRot; //to keep the rotation independant of the parent
	
	
	void Start()
	{
		//get our inital position and rotation
		keepPos = transform.position;
		keepRot = transform.rotation;
	}
	
	
	void Update()
	{
		//set our position/rotation to our inital, so that we are independant of the parent
		transform.position = keepPos;
		transform.rotation = keepRot;
	}
	
	
}

}