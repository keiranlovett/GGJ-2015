using UnityEngine;
using System.Collections;
using System.Collections.Generic;





namespace GatewayGames.ShooterAI
{

	[AddComponentMenu("Shooter AI/LOD Manager") ]
public class GatewayGamesLODManager : MonoBehaviour {
	
	public SkinnedMeshRenderer mainMeshObject = null; //the gameobject with the main mesh that should be LOD
	public List<Mesh> meshes = new List<Mesh>();
	public List<float> distances = new List<float>();
	
	
	private Mesh tempMesh = null;
	private SkinnedMeshRenderer meshRender;
	
	
	
	void Awake()
	{
		//set caches
		meshRender = mainMeshObject;
	}
	
	
	
	
	void Update()
	{
		
		//find and mesh
		tempMesh = meshes[ FindCorrectLODid( Vector3.Distance(transform.position, Camera.main.transform.position) ) ];
		
		//set correct mesh
		if(tempMesh != null && meshRender.sharedMesh != tempMesh)
		{
			meshRender.sharedMesh = tempMesh;
		}
		
	}
	
	
	
	
	
	
	/// <summary>
	/// Finds the correct LOD id.
	/// </summary>
	/// <returns>The correct LOD id.</returns>
	/// <param name="distance">Distance.</param>
	int FindCorrectLODid( float distance)
	{
		
		for(int x = 0; x < meshes.Count; x ++)
		{
			
			if(distance > distances[x])
			{
				
				if(x+1 > distances.Count-1)
				{
					return x;
				}
				
				if(distance < distances[x + 1])
				{
					return x;
				}
				
				
			}
			
			
		}
		
		return -1;	
	}
	
	

}

}
