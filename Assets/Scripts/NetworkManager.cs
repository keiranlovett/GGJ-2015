using UnityEngine;
using System.Collections.Generic;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using SystemHashtable = System.Collections.Hashtable;

public class NetworkManager : MonoBehaviour {

	// TEMPORARY TESTING STUFF
	public string botResourceName;
	public Waypoint botSpawnWaypoint;
	// END OF TESTING


	public GameObject standbyCamera;
	SpawnSpot[] spawnSpots;

	public bool offlineMode = false;

	bool connecting = false;

	List<string> chatMessages;
	int maxChatMessages = 5;

	public float respawnTimer = 0;
	public int ScoreCounter = 0;


	public string gameMode;

	bool hasPickedTeam = false;
	int teamID=0;


	public void Awake()
    {
        // in case we started this demo with the wrong scene being active, simply load the menu scene
        if (!PhotonNetwork.connected)
        {
            Application.LoadLevel(SessionSetup.SceneNameMenu);
            return;
        }
    }


	// Use this for initialization
	void Start () {
		spawnSpots = GameObject.FindObjectsOfType<SpawnSpot>();
		PhotonNetwork.player.name = PlayerPrefs.GetString("Username", "Awesome Dude");
		chatMessages = new List<string>();
		if(PhotonNetwork.playerList.Length < 2) {
		SpawnMonster();
		}
		ScoreCounter = PhotonNetwork.playerList.Length;
	}

	void OnDestroy() {
		PlayerPrefs.SetString("Username", PhotonNetwork.player.name);
	}

	public void AddChatMessage(string m) {
		GetComponent<PhotonView>().RPC ("AddChatMessage_RPC", PhotonTargets.AllBuffered, m);
	}

	[RPC]
	void AddChatMessage_RPC(string m) {
		while(chatMessages.Count >= maxChatMessages) {
			chatMessages.RemoveAt(0);
		}
		chatMessages.Add(m);
	}

	void Connect() {
		//PhotonNetwork.ConnectUsingSettings( "MultiFPS v004" );
		//

	}

	void OnGUI() {

		gameMode = (string)PhotonNetwork.room.customProperties["seed"];

		//Setup all player properties
		PhotonHashtable setPlayerTeam = new PhotonHashtable() {{"TeamName", "Spectators"}};
		PhotonNetwork.player.SetCustomProperties(setPlayerTeam);

		PhotonHashtable setPlayerKills = new PhotonHashtable() {{"Kills", 0}};
		PhotonNetwork.player.SetCustomProperties(setPlayerKills);

		PhotonHashtable setPlayerDeaths = new PhotonHashtable() {{"Deaths", 0}};
		PhotonNetwork.player.SetCustomProperties(setPlayerDeaths);
		//If press ESCAPE
		if (GUILayout.Button("Return to Lobby" + gameMode))
        {
            PhotonNetwork.LeaveRoom();  // we will load the menu level when we successfully left the room
        }


		GUILayout.Label( PhotonNetwork.connectionStateDetailed.ToString() );



		if(PhotonNetwork.connected == true && connecting == false) {
			if(hasPickedTeam) {
				// We are fully connected, make sure to display the chat box.
				GUILayout.BeginArea( new Rect(0, 0, Screen.width, Screen.height) );
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();

				foreach(string msg in chatMessages) {
					GUILayout.Label(msg);
				}

				GUILayout.EndVertical();
				GUILayout.EndArea();
			}
			else {
				// Player has not yet selected a team.
SpawnPlayer(1);

			/*	GUILayout.BeginArea( new Rect(0, 0, Screen.width, Screen.height) );
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();

				if( GUILayout.Button("Red Team") ) {
					SpawnPlayer(1);
				}

				if( GUILayout.Button("Green Team") ) {
					SpawnPlayer(2);
				}

				if( GUILayout.Button("God") ) {
					SpawnPlayer();	// 1 or 2
					SpawnGod(0);
				}

				if( GUILayout.Button("Renegade!") ) {
					SpawnPlayer(0);
				}

				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.EndArea();*/


			}

		}

	}

	void OnJoinedLobby() {
		Debug.Log ("OnJoinedLobby");
		PhotonNetwork.JoinRandomRoom();
	}

	void OnPhotonRandomJoinFailed() {
		Debug.Log ("OnPhotonRandomJoinFailed");
		PhotonNetwork.CreateRoom( null );
	}

	void OnJoinedRoom() {
		Debug.Log ("OnJoinedRoom");
		connecting = false;
	}

	void SpawnPlayer(int teamID) {
		this.teamID = teamID;
		hasPickedTeam = true;
		AddChatMessage("Spawning player: " + PhotonNetwork.playerName);

		if(spawnSpots == null) {
			Debug.LogError ("WTF?!?!?");
			return;
		}

		SpawnSpot mySpawnSpot = spawnSpots[ Random.Range (0, spawnSpots.Length) ];
		GameObject myPlayerGO = (GameObject)PhotonNetwork.Instantiate("PlayerController", mySpawnSpot.transform.position, mySpawnSpot.transform.rotation, 0);
		standbyCamera.SetActive(false);

		//((MonoBehaviour)myPlayerGO.GetComponent("FPSInputController")).enabled = true;
		((MonoBehaviour)myPlayerGO.GetComponent("MouseLook")).enabled = true;
		((MonoBehaviour)myPlayerGO.GetComponent("PlayerController")).enabled = true;

		myPlayerGO.GetComponent<PhotonView>().RPC ("SetTeamID", PhotonTargets.AllBuffered, teamID);

		myPlayerGO.transform.FindChild("Main Camera").gameObject.SetActive(true);
	}

	void SpawnMonster() {
		GameObject botCount = GameObject.FindGameObjectWithTag("Bot");
		if (botCount == null) {
			GameObject botGO = (GameObject)PhotonNetwork.Instantiate(botResourceName, botSpawnWaypoint.transform.position, botSpawnWaypoint.transform.rotation, 0);
			((MonoBehaviour)botGO.GetComponent("BotController")).enabled = true;
			botGO.GetComponent<BotController>().aggroRange = (int)PhotonNetwork.room.customProperties["ai"];
		}

	}

	void SpawnGod(int teamID) {
		this.teamID = teamID;
		hasPickedTeam = true;
		AddChatMessage("Spawning god: " + PhotonNetwork.player.name);

		if(spawnSpots == null) {
			Debug.LogError ("WTF?!?!?");
			return;
		}

		SpawnSpot mySpawnSpot = spawnSpots[ Random.Range (0, spawnSpots.Length) ];
		GameObject myGodGO = (GameObject)PhotonNetwork.Instantiate("GodController", mySpawnSpot.transform.position, mySpawnSpot.transform.rotation, 0);
		//myGodGO.name = "Test";
		standbyCamera.SetActive(false);

		myGodGO.GetComponent<PhotonView>().RPC ("SetTeamID", PhotonTargets.AllBuffered, teamID);

	}

	void Update() {
		if(respawnTimer > 0) {
			respawnTimer -= Time.deltaTime;

			if(respawnTimer <= 0) {
				// Time to respawn the player!
				SpawnGod(teamID);

				ScoreCounter--;
			}
		}

		if(ScoreCounter <= 0) {
			doGameOver();
		}
	}

public void doGameOver(){

}

public void OnMasterClientSwitched(PhotonPlayer player)
    {
        Debug.Log("OnMasterClientSwitched: " + player);

        string message;
        InRoomChat chatComponent = GetComponent<InRoomChat>();  // if we find a InRoomChat component, we print out a short message

        if (chatComponent != null)
        {
            // to check if this client is the new master...
            if (player.isLocal)
            {
                message = "You are Master Client now.";
            }
            else
            {
                message = player.name + " is Master Client now.";
            }


            chatComponent.AddLine(message); // the Chat method is a RPC. as we don't want to send an RPC and neither create a PhotonMessageInfo, lets call AddLine()
        }
    }

    public void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom (local)");

        // back to main menu
        Application.LoadLevel(SessionSetup.SceneNameMenu);
    }

    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("OnDisconnectedFromPhoton");

        // back to main menu
        Application.LoadLevel(SessionSetup.SceneNameMenu);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("OnPhotonInstantiate " + info.sender);    // you could use this info to store this or react
    }

    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        Debug.Log("OnPhotonPlayerConnected: " + player);
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Debug.Log("OnPlayerDisconneced: " + player);
    }

    public void OnFailedToConnectToPhoton()
    {
        Debug.Log("OnFailedToConnectToPhoton");

        // back to main menu
        Application.LoadLevel(SessionSetup.SceneNameMenu);
    }





}
