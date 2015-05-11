//attach to any object that needs to find cover that will be away from the objects specified


using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace GatewayGames.ShooterAI
{
	
	public enum CoverSystem { Procedural, Best, Predefined }
	
	
	public class GatewayGamesSearchCover : MonoBehaviour {
		
		public Vector3 coverPostion; //the final coverposition
		public Vector3 halfCoverPosition; //the final half cover position
		public Vector3 eyePosition; //the eye position modifier
		public float amountOfRays = 50; //the amount of checking rays
		public float fieldOfRays = 360; //the field of rays in degrees, like a FOV
		public string tagToAvoid; //the tag of the object that needs to be covered from
		private float distanceToCheck = 1000f; //the distance to check the rays
		public bool debugActive = false; //whether to debug or not
		private float trueCoverTest = 2f; //this heavily influences performance: the lower the better the performace, but worse cover predictions
		public List<string> listOfTagsToAvoid = new List<string>(); //a list of all tags that shouldn't be used as cover
		
		public CoverSystem coverSystemToUse = CoverSystem.Best; //the cover system to use
		public List<Vector3> predefinedPoints = new List<Vector3>(); //the list with predifined points	
		
		
		
		private bool[] rayFree;    //whether the position is cover [negative] or not [positive]
		private Vector3[] coverPosRay; //the vector3 of the position that is free
		public string[] distanceToCover; //the distance to cover
		private GameObject closest; //a variable used to find the closest enemy
		public GameObject avoidGameobject; //the internal gameobject to avoid
		private int closestID; //internal use to find the closest id
		private bool objectToAvoidExists; //internal variable to check whether the object to avoid exists
		private List<Vector3> selectionArray = new List<Vector3>(); //the different stats for each ray ist stored here
		private GatewayGamesWeaponManager weaponsManager;
		private Vector3 optimumVector3;
		
	
		
		
		void Awake()
		{
			coverPosRay = new Vector3[Mathf.RoundToInt(amountOfRays)];
			rayFree = new bool[Mathf.RoundToInt(amountOfRays)];
			distanceToCover = new string[Mathf.RoundToInt(amountOfRays)];
			coverPostion = transform.position;
			weaponsManager = GetComponent<GatewayGamesWeaponManager>();
			
			//this is against a unity bug
			if(objectToAvoidExists == false)
			{
				objectToAvoidExists = false;
			}
			
			//default values
			if(amountOfRays == 0)
			{
				amountOfRays = 50;
				fieldOfRays = 360;
				distanceToCheck = 1000f;
			}
			
		}	
		
		
		void Update()
		{
			//update any internal compuattions
			coverPostion = optimumVector3;
		}
		
		
		
		/// <summary>
		/// Finds the cover postion at relative eye leve.
		/// </summary>
		/// <returns>The cover postion.</returns>
		/// <param name="eyeLevel">Eye level.</param>
		/// <param name="halfCover">If set to <c>true</c> half cover.</param>
		public virtual Vector3 FindCoverPostion(Vector3 eyeLevel, bool halfCover)
		{
			//set fallback values
			coverPostion = transform.position;
			
			//avoid errors
			if(avoidGameobject == null)
			{
				
				return coverPostion;
			}
			
			//configure mode
			switch( coverSystemToUse )
			{
			case CoverSystem.Best: selectionArray.AddRange( predefinedPoints ); break;
			case CoverSystem.Predefined: selectionArray = predefinedPoints; goto CALCULATE_BEST_COVER;
			case CoverSystem.Procedural: break;
			}
			if(coverSystemToUse == CoverSystem.Predefined)
			{
				return coverPostion;
			}
			
			
			
			//find the closest gameobject to avoid
			if(GameObject.FindGameObjectsWithTag(tagToAvoid) == null)
			{
				objectToAvoidExists = false;
				avoidGameobject = gameObject;
			}
			
			
			//start raycasts
			float currentId = 0f;
			//repeat as many rays as we need
			while(currentId < amountOfRays)
			{
				
				//calculate the y rotation of the ray to use from the amount of rays, field of rays and current id
				float rayYRot = (transform.eulerAngles.y - (fieldOfRays/2f)) + ((fieldOfRays/amountOfRays) * currentId); 
				//calculate ray
				Vector3 rayToUse = SetVectorFromAngle(0, rayYRot, 0, distanceToCheck);
				if(debugActive == true)
				{
					Debug.DrawRay(transform.position+eyePosition, rayToUse, Color.green);
				}
				
				
				//start the actual ray cast with the vectors we calculated
				RaycastHit hit;
				if (Physics.Raycast( transform.position + eyePosition, rayToUse, out hit, rayToUse.magnitude))
				{
					
					if( TagToBeAvoided(hit.transform.tag) )
					{
						continue;
					}
					
					
					//record the vector if we hit something other than the person
					if(hit.transform.tag != tagToAvoid)
					{
						coverPosRay[Mathf.RoundToInt(currentId)] = hit.point;
					}
					
					
					//start the next raycast to check if we can see the avoiding object from the hit point
					RaycastHit hit2;
					if(debugActive == true)
					{
						Debug.DrawRay(hit.point, avoidGameobject.transform.position - hit.point, Color.red);
					}
					
					if (avoidGameobject != null && Physics.Raycast(hit.point, avoidGameobject.transform.position - hit.point, out hit2, (avoidGameobject.transform.position - hit.point).magnitude))
					{
						
						
						//check whether we hit the object to avoid or not, and record results
						if(hit2.transform.tag != tagToAvoid && SpotTrueCover(hit.point) == true)
						{
							//this is a potential cover
							rayFree[Mathf.RoundToInt(currentId)] = false;
							//add to selection array
							selectionArray.Add(hit.point);
							
						}
						else
						{
							//we try to send another raycast in a random direction, to test if we can find a better hiding spot there
							rayFree[Mathf.RoundToInt(currentId)] = true;
							float yrot2 = UnityEngine.Random.Range(0f, 359f);
							Vector3 rayToUse2 = SetVectorFromAngle(0, yrot2, 0, distanceToCheck);
							
							if(debugActive == true)
							{
								Debug.DrawRay(hit.point, rayToUse2, Color.blue);
							}
							
							RaycastHit hit3;
							if (Physics.Raycast(hit.point, rayToUse2, out hit3, rayToUse2.magnitude))
							{
								
								if( TagToBeAvoided(hit3.transform.tag) )
								{
									continue;
								}
								
								//start the next raycast to check if we can see the avoiding object from the hit point
								RaycastHit hit4;
								if(debugActive == true)
								{
									Debug.DrawRay(hit3.point, avoidGameobject.transform.position - hit3.point, Color.yellow);
								}
								
								if(Physics.Raycast(hit3.point, avoidGameobject.transform.position - hit3.point, out hit4, (avoidGameobject.transform.position - hit3.point).magnitude))
								{
									//check whether we hit the object to avoid or not, and record results
									if(hit4.transform.tag != tagToAvoid  && SpotTrueCover(hit3.point) == true)
									{
										//this is a potential cover
										//add to selection array
										selectionArray.Add(hit3.point);
									}
								}
							}
						}
						
					}
					
				}
				
				currentId += 1f;
			}
			
			
			
		CALCULATE_BEST_COVER:
			
			//now determine the best cover from the retrieved info
			//find the optimum cover based on whether we're using half cover or not
			if(halfCover == false)
			{
				
				if(avoidGameobject != null)
				{
					
					Vector3 coverPosition2 = FindOptimumVector3(transform.position, avoidGameobject.transform.position);
					if( coverPosition2 != Vector3.zero )
					{
						coverPostion = optimumVector3;
					}
					
				}
				
			}
			else
			{
				
				if(avoidGameobject != null)
				{
					Vector3 coverPosition2 = FindHalfCover(transform.position, avoidGameobject.transform.position);
					if( coverPosition2 != Vector3.zero )
					{
						halfCoverPosition = coverPosition2;
					}
				}
				
				
			}
			
			
			
			//return
			return coverPostion;
		}
		
		
		
		
		
		
		
		//find closest vector3 using the list selection array
		Vector3 FindClosestVector3(Vector3 positionA)
		{
			int testID = 0;
			float smallestDistance = Mathf.Infinity;
			Vector3 closestVector3 = Vector3.zero;
			
			while(testID < selectionArray.Count-1)
			{
				
				if(Vector3.Distance(positionA, selectionArray[testID]) < smallestDistance)
				{
					smallestDistance = Vector3.Distance(positionA, selectionArray[testID]);
					closestVector3 = selectionArray[testID];
				}
				
				testID += 1;
			}
			
			return closestVector3;
			
		}
		
		
		
		//find optimim (distance to player/distance to ai)
		Vector3 FindOptimumVector3(Vector3 self, Vector3 player)
		{
			StartCoroutine(CoverCycleOptimized(self, player));
			
			return optimumVector3;
		}	
		
		
		IEnumerator CoverCycleOptimized(Vector3 self, Vector3 player)
		{
			int testID = 0;
			float largestQuote = 0f;
			int optimizeID = 0;
			
			while(testID < selectionArray.Count-1)
			{
				
				RaycastHit[] hits;
				hits = Physics.RaycastAll(selectionArray[testID], player-selectionArray[testID], (player-selectionArray[testID]).magnitude);
				
				
				
				if((Vector3.Distance(player, selectionArray[testID])/Vector3.Distance(self, selectionArray[testID]))*hits.Length > largestQuote)
				{
					largestQuote = (Vector3.Distance(player, selectionArray[testID])/Vector3.Distance(self, selectionArray[testID]))*hits.Length;  
					
					optimumVector3 = selectionArray[testID];
				}
				
				testID += 1;
				
				//optimization by spreading of a number of frames
				optimizeID += 1;
				if(optimizeID > 10)
				{
					optimizeID = 0;
					yield return new WaitForEndOfFrame();
				}
			}
		}
		
		
		
		Vector3 FindHalfCover( Vector3 self, Vector3 player)
		{
			
			int testID = 0;
			//float largestQuote = 0f;
			//Vector3 optimumVector3 = Vector3.zero;
			
			
			//first cycle through the whole array to find which of the points are actually half-covers
			while(testID < selectionArray.Count-1)
			{
				Vector3 testVector =  avoidGameobject.transform.position - (selectionArray[testID] + eyePosition);
				
				//Debug.DrawRay( (selectionArray[testID] + eyePosition), testVector, Color.red);
				
				
				RaycastHit hit;
				if( Physics.Raycast( (selectionArray[testID] + eyePosition), testVector, out hit, testVector.magnitude) )
				{
					
					//if the object we see is the one we're seeking
					if(hit.transform.gameObject == avoidGameobject)
					{
						testID += 1;
						continue;
						
					}
					else
					{
						
						if(hit.transform.parent != null)
						{
							
							
							if(hit.transform.parent != null && hit.transform.parent.gameObject == avoidGameobject)
							{
								testID += 1;
								continue;
								
								//Debug.Log("This far 2");
								
							}
							else
							{
								
								
								
								if(hit.transform.parent.transform.parent != null && hit.transform.parent.transform.parent.gameObject == avoidGameobject)
								{
									testID += 1;
									continue;
									
									//Debug.Log("This far 3");
								}
								
							}
						}
					}
					
					//if this isn't our object to seek, delete this point from the selection list
					selectionArray[testID] = Vector3.zero;
					
					
				}
				
				testID += 1;
			}
			
			testID = 0;
			
			
			//<-----------------------------------------------NEW CODE (COPIED FROM ABOVE; SEE FUNCTION "FINDCLOSESTVECTOR3"; replace "positionA" with "self")-------------------------------->
			
			float smallestDistance = Mathf.Infinity;
			Vector3 closestVector3 = Vector3.zero;
			
			while(testID < selectionArray.Count-1)
			{
				
				if(selectionArray[testID] != Vector3.zero && Vector3.Distance( self, selectionArray[testID]) < smallestDistance &&
				   Vector3.Distance( self, selectionArray[testID]) < weaponsManager.currentWeaponMaxDistance )
				{
					smallestDistance = Vector3.Distance( self, selectionArray[testID]);
					closestVector3 = selectionArray[testID];
				}
				
				testID += 1;
			}
			
			return closestVector3;
			
		}
		
		
		//calculate vector
		Vector3 SetVectorFromAngle( float x, float y, float z, float distance)
		{
			var rotation = Quaternion.Euler(x, y, z);
			var forward = Vector3.forward * distance;
			return rotation * forward;
		}
		
		
		//find closest enemy
		GameObject FindClosestEnemy(string tagToUse) {
			
			GameObject[] gos;
			gos = GameObject.FindGameObjectsWithTag(tagToUse);
			float distance = Mathf.Infinity;
			Vector3 position = transform.position;
			foreach (GameObject go in gos) {
				Vector3 diff = go.transform.position - position;
				float curDistance = diff.sqrMagnitude;
				if (curDistance < distance) {
					closest = go;
					distance = curDistance;
				}
			}
			return closest;
		}	
		
		
		//check if this spot really is a cover, by placing a number of random  rays within a certain radius; return true if free, false if not
		public bool SpotTrueCover(Vector3 initPosition)
		{
			
			if(avoidGameobject == null)
			{
				return true;
			}
			
			
			float radius = 1f;
			if(GetComponent<NavMeshAgent>() != null)
			{
				radius = GetComponent<NavMeshAgent>().radius;
			}
			
			
			
			float checkInt = 0f;
			bool totalFree = true;
			
			while(checkInt < trueCoverTest)
			{		
				
				Vector3 checkPos = initPosition + UnityEngine.Random.insideUnitSphere * (radius+3.5f); 
				
				RaycastHit hitCheck;
				
				if (Physics.Raycast(checkPos, avoidGameobject.transform.position - checkPos, out hitCheck, (avoidGameobject.transform.position - checkPos).magnitude))
				{
					
					//check whether we hit the object to avoid or not, and record results
					if(Vector3.Distance(hitCheck.point, avoidGameobject.transform.position) < 0.5f)
					{
						totalFree = false;
						
						if(debugActive == true)
						{
							Debug.DrawRay(checkPos, avoidGameobject.transform.position - checkPos, Color.black);
						}
						
					}
					else
					{
						if(debugActive == true)
						{
							Debug.DrawRay(checkPos, avoidGameobject.transform.position - checkPos, Color.magenta);
						}
					}
				}
				
				checkInt += 1f;
			}
			
			return totalFree;
		}
		
		
		
		/// <summary>
		/// Check whether the provided tag should be avoided.
		/// </summary>
		/// <returns><c>true</c>, if to be avoided was taged, <c>false</c> otherwise.</returns>
		/// <param name="tagToCheck">Tag to check.</param>
		public bool TagToBeAvoided(string tagToCheck)
		{
			bool isTheAvoid = false;
			
			foreach(string tag in listOfTagsToAvoid)
			{
				
				if(tag == tagToCheck)
				{
					isTheAvoid = true;
				}
			}
			
			return isTheAvoid;
		}
		
		
		/// <summary>
		/// Sets new cover points.
		/// </summary>
		/// <param name="newPoints">New points.</param>
		public void NewPredefinedPoints(List<Vector3> newPoints)
		{
			predefinedPoints = newPoints;
		}
		
		
		
		//visuals
		void OnDrawGizmosSelected ()
		{
			
			if(debugActive == true)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(coverPostion, 0.5f);
			}
			
		}
		
	}
	
}