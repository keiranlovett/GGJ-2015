using UnityEngine;
using System.Collections;


namespace GatewayGames.ShooterAI
{
	
	
	public class GatewayGamesSearchCoverBacover : GatewayGamesSearchCover 
	{
		
		
		

		public LayerMask lm;
		
		private Transform frontsensor; //set it to an object
		private Transform primarytarget;

		private GameObject lht;
		private Transform MoveAround;
		
		private bool findHalfCover;
		private Transform eyes;
		
		bool findcover;
		int index;
		bool iseven1;
		
		float sine;
		//bool coveravailable;
		
		//Vector3 laststoredcover;
		float chosensine;
		
		
		
		// MOVEAROUND should be a simple cube with no collider ( for visual aid ) and then have LHT on the side that faces outwards from the cover
		// you will find that side by simple testing it. 1 movearound is usually enough for 60 or more AIs.
		// Use this for initialization
		
		
		
		void Awake()
		{
						
			//set caches
			eyes = GetComponent<GatewayGamesBrain>().eyes.transform;
			
			//create neccearry objects
			MoveAround = new GameObject().transform;
			frontsensor = new GameObject().transform;
			
			frontsensor.parent = eyes.parent;
			
			MoveAround.name = "CoverPosition";
			frontsensor.name = "Front Sensor";
			
			//do lht
			lht = new GameObject();
			lht.transform.parent = MoveAround;
			lht.transform.localPosition = new Vector3( 0f, 0f, -0.5f ); 
			lht.name = "LHT";
		}
		
		
		
		public override Vector3 FindCoverPostion (Vector3 eyeLevel, bool halfCover)
		{
			//set vars
			findcover = !halfCover;
			findHalfCover = halfCover;
			chosensine = 0f;
			
			return transform.position;
		}
		
		
		
		
		
		// Update is called once per frame
		void Update () {
		
			
			//don't do the function if we don't need to search
			if (findcover == false && findHalfCover == false) 
			{
				return;
			}
			
			if(avoidGameobject != null)
			{
				primarytarget = avoidGameobject.transform;
			}
			else
			{
				return;
			}
			
			
			if (findcover || findHalfCover) 
			{
				
				for (int i = 0; i < 60; i++) 
				{
					
					index = i - 30; // Making sure the 30th raycast faces forward
					RaycastHit ithit;
					
					
					
					iseven1 = (i % 2 == 0); // checking if it can be divided by 2 ( so we use half the rays for opt.)
					
					if (iseven1) {																
						Debug.DrawRay (frontsensor.position, Quaternion.AngleAxis (i, transform.up) * frontsensor.forward * 20, Color.green); // For visual aid in scene view, shows where the rays are pointing
					}
					
					if (iseven1) { // currently optimized, only every second ray gets casted ( i hope lol )
						

						
						if (Physics.Raycast (frontsensor.position, Quaternion.AngleAxis ((float)index, transform.up) * frontsensor.forward, out ithit, 20f, lm)) 
						{ //raycast towards the current i value making sure we cast in all directions.
							
							
							
							if (Vector3.Distance (frontsensor.position, ithit.point) > 0) 
							{ // making sure cover is ok
								
								
								
								Ray r = new Ray (frontsensor.position, Quaternion.AngleAxis ((float)index, transform.up) * frontsensor.forward);
								
								Debug.DrawRay (frontsensor.position, ithit.point, Color.yellow); // just useless debug rays
								sine = Vector3.Angle (r.direction, ithit.normal); // getting the value of the degree
								
								
								// making sure the cover is suitable angle wise and that we are looking for cover
								RaycastHit whathit;
								if (sine > 134 && sine > chosensine) 
								{ 
								
									//move cover to possible cover location, for linecast.																				
									MoveAround.position = ithit.point;													
									MoveAround.rotation = Quaternion.LookRotation (-ithit.normal);
									
									// linecast from the LHT empty gameobject that is child of the MoveAround, see top of script for LHT descr. and setup
									//Debug.Log( primarytarget);
									if (Physics.Linecast (lht.transform.position, primarytarget.position, out whathit)) 
									{
									
										//laststoredcover = transform.position; // store the cover pos
										if (whathit.transform != primarytarget ) { // check if we have a better pos or not, if not then move to this cover
											if (findcover == true && findHalfCover == false) 
											{	
												// this is probably useless, was just making sure the ai needs cover
												chosensine = sine;		//make this the best pos value ( angle )	
												coverPostion = MoveAround.position;																																																			
											}
											else
											{
												if(findHalfCover == true && HalfCover(MoveAround.position) == true )
												{
													chosensine = sine;	//make this the best pos value ( angle )
													halfCoverPosition = MoveAround.position;
												}
											}
										}
									}
								} 																	
							}
						}					
					}	
				}
				
				//once we're finished with the cycle, turn off
				findcover = false;
				findHalfCover = false;
				
			}
		}
		
		
		
		bool HalfCover(Vector3 cover)
		{
			RaycastHit hittest;
			if (Physics.Linecast (cover + new Vector3 (0, 0.5f, 0), primarytarget.position, out hittest)) 
			{
				
				if (hittest.transform.IsChildOf( primarytarget)) 
				{
					return true;
				} 
				else 
				{
					return false;
				}
			}
			
			return false;
		}
		
		
	}
}
