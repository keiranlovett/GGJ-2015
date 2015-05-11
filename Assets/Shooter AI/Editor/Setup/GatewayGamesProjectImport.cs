using UnityEditor;
using UnityEngine;
using System.Collections;

namespace GatewayGames.ShooterAI
{
	
	public class GatewayGamesProjectImport : AssetPostprocessor 
	{
		
		static void OnPostprocessAllAssets (
			string[] importedAssets, string[] deletedAssets,
			string[] movedAssets, string[] movedFromAssetPaths) 
		{
			
			if (IsMyFrameWorkTagFile(importedAssets))	
			{
				
				
				
				SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
				
				SerializedProperty it = tagManager.GetIterator();
				bool showChildren = true;
				while (it.NextVisible(showChildren))
				{	
					//set your tags here
					if (it.name == "data")
					{
						//check if bullet already exists
						if(it.stringValue == "Bullet")
						{
							return;
						}
						
						if(it.stringValue == "")
						{
							it.stringValue = "Bullet";
							break;
						}
					}
				}
				tagManager.ApplyModifiedProperties();
			}
		}
		
		static bool IsMyFrameWorkTagFile(string[] asstNames)
		{
			foreach(string s in asstNames)
			{
				//replaced by your path
				if (s.Equals("Assets/Shooter AI/Editor/Setup/GatewayGamesProjectImport.cs"))
					return true;
			}   
			return false;
		}
	}
	
}