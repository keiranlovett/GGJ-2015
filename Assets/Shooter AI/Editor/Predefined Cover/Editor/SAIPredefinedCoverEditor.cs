using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using GatewayGames.ShooterAI;




[ CustomEditor( typeof( GatewayGamesSearchCover) ) ]
public class SAIPredefinedCoverEditor : Editor {
	
	
	
	public CoverSystem coverSystemToUse; //the cover system to use
	public List<Vector3> predefinedmPoints = new List<Vector3>(); //the list with predefined points
	
	
	private Vector2 scrollPos;
	private GatewayGamesSearchCover sc;
	private string fileSaveLoc = "Assets/PosSave.txt"; //the save location
	private string fileOpenLoc = "Assets/PosSave.txt"; //the open location
	
	void Awake()
	{
		sc = (GatewayGamesSearchCover)target;
	}
	

	public override void OnInspectorGUI()
	{
		
		//check for points changed
		if(GUI.changed == true)
		{
			sc.predefinedPoints = predefinedmPoints;
		}
		else
		{
			predefinedmPoints = sc.predefinedPoints;
		}
		
		
		EditorGUILayout.Space();
		
		//select the type of cover system
		coverSystemToUse = (CoverSystem)EditorGUILayout.EnumPopup( "Which cover system to use: ", coverSystemToUse );
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		
		//set the varaibles correctly
		if(Selection.gameObjects.Length == 0)
		{
			return;
		}
		
		
		if( GUILayout.Button("Create New Cover Position") )
		{
			predefinedmPoints.Add( Vector3.zero );
		}
		
		
		//make the scroll group
		scrollPos = EditorGUILayout.BeginScrollView( scrollPos, GUILayout.Height(100f) );
		
		//make each point visible and manipulatable
		for(int x = 0; x < predefinedmPoints.Count; x++)
		{	
			//in the tab
			EditorGUILayout.BeginHorizontal();
			{
				predefinedmPoints[x] = EditorGUILayout.Vector3Field( x.ToString(), predefinedmPoints[x] );	
				
				if( GUILayout.Button( "X" ) )
				{
					predefinedmPoints.Remove( predefinedmPoints[x] );
				}
			}
			EditorGUILayout.EndHorizontal();
			
		}
		
		//end scroll pos
		EditorGUILayout.EndScrollView();
		
		
		//save to file
		EditorGUILayout.BeginHorizontal();
		if( GUILayout.Button( "Save to File" ) )
		{

			//write to file
			string saveFile = "";
			for(int x = 0; x < predefinedmPoints.Count; x ++)
			{
				//add seperator
				saveFile += "/";
				
				//add the individual components
				saveFile += predefinedmPoints[x].x.ToString();
				saveFile += "~";
				saveFile += predefinedmPoints[x].y.ToString();
				saveFile += "~";
				saveFile += predefinedmPoints[x].z.ToString();
				
			}
			
			System.IO.File.WriteAllText( fileSaveLoc , saveFile);
			
		}
		
		fileSaveLoc = EditorGUILayout.TextField(fileSaveLoc);
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		//load from file
		if( GUILayout.Button( "Load from File" ) )
		{
			
			//open the file first
			string rawFile = System.IO.File.ReadAllText(fileOpenLoc);
			
			//parse first all the individual vectors
			string[] vectors = rawFile.Split( "/".ToCharArray()[0] );
			
			//now correctly fill up the array
			predefinedmPoints.Clear();
			
			//loop through al; start at 1 to skip the first which is a blank
			for(int x = 1; x < vectors.Length; x++)
			{
				//split up the vectors
				string[] component = vectors[x].Split( "~".ToCharArray()[0] );
				
				//put in the component
				Vector3 newCoverPos = Vector3.zero;
				
				newCoverPos.x = float.Parse( component[0] );
				newCoverPos.y = float.Parse( component[1] );
				newCoverPos.z = float.Parse( component[2] );
				
				//add it to the main array
				predefinedmPoints.Add( newCoverPos );
				
				
			}
			
			
		}
		fileOpenLoc = EditorGUILayout.TextField(fileOpenLoc);
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		
		//check for coversystem change
		if(GUI.changed == true)
		{
			sc.coverSystemToUse = coverSystemToUse;
		}
		else
		{
			coverSystemToUse = sc.coverSystemToUse;
		}
	}
	
	
	
	void OnSceneGUI()
	{

		//make each point visible and manipulatable
		for(int x = 0; x < predefinedmPoints.Count; x++)
		{
			//in the scene view
			predefinedmPoints[x] = Handles.PositionHandle( predefinedmPoints[x], Quaternion.identity );

			Handles.SphereCap(0, predefinedmPoints[x], Quaternion.identity, 0.7f);
		}

	}
	
	
}
