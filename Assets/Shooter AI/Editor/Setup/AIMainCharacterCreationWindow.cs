using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using GatewayGames.ShooterAI;


public class AIMainCharacterCreationWindow : EditorWindow 
{
	
	
	public Transform modelObject; //the object that contains the model and is a ragdoll
	private string newName; //the name for the new ai
	private GameObject weaponObjet; //the weapon
	
	private enum NavSystems { AStar, UnityNavmesh};
	private NavSystems navsysToUse = NavSystems.UnityNavmesh;
	
	//this is for creating the instances
	private string nameOfObject = "Prefabs/AICharacters/EmptyAI"; //the name of instance that we use as a prefab for the ai
	private string nameOfAnimator = "Prefabs/Animator/EnemyAnimator"; //the name of the ai animator
	private Transform modelObjectPrefab;
	bool useMultiplayer = true;
	
	//this is for IK setup
	private Vector3 defaultEngageLocalPos = new Vector3( 0.2327956f, 0.06857096f, 0.00272f );
	private Vector3 defaultNormalEngagePos = new Vector3( 0.3008149f, 0.08790074f, 0.1776404f );
	private Vector3 defaultNormalRot = new Vector3( 42.42332f, 280.452f, 356.3545f );
	
	//cover settings
	private enum CoverSystemsSetup { Bacover, RayTracing };
	private CoverSystemsSetup coversystemToUse;
	
	
	[MenuItem("Tools/Shooter AI/Create New Character")]
	private static void showEditor()
	{
		EditorWindow.GetWindow (typeof (AIMainCharacterCreationWindow), true, "Shooter AI Character Creation");
	}
	
	
	//all the gui stuff
	void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 200, 20), "Create New AI Characters", EditorStyles.boldLabel);
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField("Name of new AI");
			newName = EditorGUILayout.TextField(newName);
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField("Please select with navigation system the AI should use");
			navsysToUse = (NavSystems)EditorGUILayout.EnumPopup( "", navsysToUse);
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField("Please select the object that contains the model with the ragdoll");
			modelObjectPrefab = EditorGUILayout.ObjectField(modelObjectPrefab, typeof(Transform), true) as Transform;
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField("Please select FINISHED WEAPON object (optional at this stage)");
			weaponObjet = EditorGUILayout.ObjectField(weaponObjet, typeof(Object), true) as GameObject;
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField("Please select the cover system to use");
			coversystemToUse = (CoverSystemsSetup)EditorGUILayout.EnumPopup( "", coversystemToUse);
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField("Should it be multiplayer enabled?");
			useMultiplayer = EditorGUILayout.Toggle( useMultiplayer );
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		if(GUILayout.Button("Create AI") && modelObjectPrefab != null)
		{
			CreateNewAI();
		}
		
		if(modelObjectPrefab != null && modelObjectPrefab.GetComponent<Animator>() == null)
		{
			EditorGUILayout.LabelField("Please add an animator to your model object!");
		}
		
		if(modelObjectPrefab != null && modelObjectPrefab.GetComponent<Animator>() != null && modelObjectPrefab.GetComponent<Animator>().avatar == null)
		{
			EditorGUILayout.LabelField("Please add an animator with an avatar to your model object!");
		}
		
	}
	
	
	
	
	
	//create new ai
	void CreateNewAI()
	{
		
			//create the ai from the template and disconnect it from the prefab
		GameObject newAI = null;
		
		newAI = PrefabUtility.InstantiatePrefab(Resources.Load(nameOfObject) as GameObject) as GameObject;
	
		
		PrefabUtility.DisconnectPrefabInstance(newAI);
		newAI.name = newName;
		
		//create the model
		modelObject = Instantiate( modelObjectPrefab ) as Transform;
		modelObject.transform.parent = newAI.transform;
		modelObject.name = "Model";
		if(modelObject.GetComponent<Animator>() == null)
		{
			modelObject.gameObject.AddComponent<Animator>();
		}
		modelObject.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(nameOfAnimator) as RuntimeAnimatorController;
		modelObject.gameObject.AddComponent<GatewayGamesModelManager>();
		modelObject.GetComponent<GatewayGamesModelManager>().movement = newAI.GetComponent<GatewayGamesMovementContoller>();
		modelObject.GetComponent<GatewayGamesModelManager>().brain = newAI.GetComponent<GatewayGamesBrain>();
		modelObject.gameObject.AddComponent<GatewayGamesRagdollHelper>();
		modelObject.gameObject.AddComponent<ShooterAIIK>();
		modelObject.gameObject.AddComponent<UpperBodyLookAt>();
		modelObject.transform.localPosition = Vector3.zero;
		modelObject.transform.localRotation = Quaternion.identity;
		
		
		//set A* nav system
		if(navsysToUse == NavSystems.AStar)
		{
			//first destroy the old systems
			DestroyImmediate( newAI.GetComponent<GatewayGamesMovementContoller>());
			DestroyImmediate( newAI.GetComponent<NavMeshAgent>() );
			
			//then add A* stuff
			newAI.AddComponent<GatewayGamesMovementControllerASTAR>();
			newAI.AddComponent<Seeker>();
			newAI.AddComponent<GatewayGamesAgent>();
			
			//set references
			modelObject.GetComponent<GatewayGamesModelManager>().movement = newAI.GetComponent<GatewayGamesMovementControllerASTAR>();
		}
		
		//this creates the gun
		if(weaponObjet != null)
		{
			var gun = Instantiate(weaponObjet) as GameObject;
			gun.transform.parent = newAI.transform;
			gun.transform.localPosition = Vector3.zero;
			gun.transform.localEulerAngles = Vector3.zero;
			
			newAI.GetComponent<GatewayGamesWeaponManager>().currentHoldingWeapon = gun.transform;
		}
		
		//do the cover system
		if(coversystemToUse == CoverSystemsSetup.Bacover)
		{
			DestroyImmediate(newAI.GetComponent<GatewayGamesSearchCover>());
			newAI.AddComponent<GatewayGamesSearchCoverBacover>();
		}
		
		
		//add multiplayer
		if(useMultiplayer == true)
		{
			newAI.AddComponent<GatewayGamesMultiplayerController>();
			
		}
		
		//set references
		newAI.GetComponent<GatewayGamesBrain>().modelManager = modelObject.GetComponent<GatewayGamesModelManager>();
		
		//set IK stuff		
		newAI.GetComponent<GatewayGamesWeaponManager>().engageWeaponHoldingLocation.parent = modelObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Neck).parent;
		newAI.GetComponent<GatewayGamesWeaponManager>().engageWeaponHoldingLocation.localPosition = defaultEngageLocalPos;
		newAI.GetComponent<GatewayGamesWeaponManager>().defaultWeaponHoldingLocation.parent = modelObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Neck).parent;
		newAI.GetComponent<GatewayGamesWeaponManager>().defaultWeaponHoldingLocation.localPosition = defaultNormalEngagePos;
		newAI.GetComponent<GatewayGamesWeaponManager>().defaultWeaponHoldingLocation.localEulerAngles = defaultNormalRot;
		
		//this code is to destroy a unity bug that makes the object still connected to the prefab
		GameObject disconnectingObj = newAI.gameObject; 
		PrefabUtility.DisconnectPrefabInstance(disconnectingObj);
		Object prefab = PrefabUtility.CreateEmptyPrefab("Assets/dummy.prefab");
		PrefabUtility.ReplacePrefab(disconnectingObj, prefab, ReplacePrefabOptions.ConnectToPrefab);
		PrefabUtility.DisconnectPrefabInstance(disconnectingObj);
		AssetDatabase.DeleteAsset("Assets/dummy.prefab");
		
		
	}
	
	
	
}
