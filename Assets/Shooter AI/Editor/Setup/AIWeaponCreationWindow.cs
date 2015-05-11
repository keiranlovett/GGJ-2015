using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using GatewayGames.ShooterAI; 


public class AIWeaponCreationWindow : EditorWindow 
{
	
	
	
	public Transform modelObject; //the object that contains the model and is a ragdoll
	private string newName; //the name for the new ai
	private Rigidbody bulletToUse; //the bullets to use
	private Rigidbody emptyMagazine; //the empty magazine object
	
	
	//this is for creating the instances
	private string nameOfObject = "Prefabs/Weapons/EmptyWeapon"; //the name of instance that we use as a prefab for the ai
	
	
	
	
	
	
	
	[MenuItem("Tools/Shooter AI/Create New Weapon")]
	private static void showEditor()
	{
		EditorWindow.GetWindow (typeof (AIWeaponCreationWindow), true, "Weapon Creation");
	}
	
	
	
	
	//all the gui stuff
	void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 200, 20), "Create New AI Weapons", EditorStyles.boldLabel);
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		
		
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField("Name of new weapon");
			newName = EditorGUILayout.TextField(newName);
		}
		EditorGUILayout.EndHorizontal();
		
		
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField("Please select the object that contains the model of the weapon");
			modelObject = EditorGUILayout.ObjectField(modelObject, typeof(Transform), true) as Transform;
		}
		EditorGUILayout.EndHorizontal();
		
		
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		if(GUILayout.Button("Create Weapon"))
		{
			CreateNewWeapon();
		}
		
	}
	
	
	public void CreateNewWeapon()
	{
		
		
		//create the ai from the template and disconnect it from the prefab
		var newAI = PrefabUtility.InstantiatePrefab(Resources.Load(nameOfObject) as GameObject) as GameObject;
		PrefabUtility.DisconnectPrefabInstance(newAI);
		newAI.name = newName;
		
		//create the model and set all correct components
		var aiModel = Instantiate(modelObject) as Transform;
		aiModel.transform.parent = newAI.transform;
		aiModel.transform.localPosition = Vector3.zero;
		aiModel.transform.localEulerAngles = Vector3.zero;
		aiModel.name = "Model";
		
		//create IK references
		newAI.GetComponent<GatewayGamesWeapon>().ikLeftHand.parent = aiModel;
		newAI.GetComponent<GatewayGamesWeapon>().ikRightHand.parent = aiModel;
		
		//this code is to destroy a unity bug that makes the object still connected to the prefab
		GameObject disconnectingObj = newAI.gameObject; 
		PrefabUtility.DisconnectPrefabInstance(disconnectingObj);
		Object prefab = PrefabUtility.CreateEmptyPrefab("Assets/dummy.prefab");
		PrefabUtility.ReplacePrefab(disconnectingObj, prefab, ReplacePrefabOptions.ConnectToPrefab);
		PrefabUtility.DisconnectPrefabInstance(disconnectingObj);
		AssetDatabase.DeleteAsset("Assets/dummy.prefab");
		
		
		
	}
	
	
	
	
}
