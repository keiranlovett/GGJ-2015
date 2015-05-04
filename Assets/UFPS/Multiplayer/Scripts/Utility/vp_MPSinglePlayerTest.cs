/////////////////////////////////////////////////////////////////////////////////
//
//	vp_MPSinglePlayerTest.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a development utility for quick tests where you don't want
//					to spend time connecting to the Photon Cloud. just put this
//					script on a gameobject in the scene and activate it.
//					NOTE: not intended for use in an actual game
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class vp_MPSinglePlayerTest : MonoBehaviour
{

	public GameObject LocalPlayerPrefab = null;
	public SpawnMode m_SpawnMode = SpawnMode.Prefab;

	public enum SpawnMode
	{
		Scene,
		Prefab
	}


	/// <summary>
	/// 
	/// </summary>
	protected virtual void Start()
	{

		PhotonNetwork.offlineMode = true;
		vp_Gameplay.isMultiplayer = false;
		vp_Gameplay.isMaster = true;

		vp_PlayerEventHandler[] players = FindObjectsOfType<vp_PlayerEventHandler>();
		foreach (vp_PlayerEventHandler player in players)
		{
			vp_Utility.Activate(player.gameObject, false);
		}

		vp_MPMaster[] masters = Component.FindObjectsOfType<vp_MPMaster>() as vp_MPMaster[];
		foreach (vp_MPMaster g in masters)
		{
			if (g.gameObject != gameObject)
				vp_Utility.Activate(g.gameObject, false);
		}

		// disable demo gui via globalevent since we don't want hard references
		// to code in the demo folder
		vp_GlobalEvent.Send("DisableMultiplayerGUI", vp_GlobalEventMode.DONT_REQUIRE_LISTENER);

		vp_SpawnPoint p = vp_SpawnPoint.GetRandomSpawnPoint();

		switch (m_SpawnMode)
		{
			case SpawnMode.Prefab:
				GameObject l = (GameObject)GameObject.Instantiate(LocalPlayerPrefab, p.transform.position, p.transform.rotation);
				l.GetComponent<vp_PlayerEventHandler>().Rotation.Set(p.transform.eulerAngles);
				break;
			case SpawnMode.Scene:
				vp_Utility.Activate(LocalPlayerPrefab, true);
				if (p != null)
				{
					LocalPlayerPrefab.transform.position = p.transform.position;
					LocalPlayerPrefab.GetComponent<vp_PlayerEventHandler>().Rotation.Set(p.transform.eulerAngles);
				}
				break;
		}

	}


}