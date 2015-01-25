using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using SystemHashtable = System.Collections.Hashtable;

public class NetworkManager : MonoBehaviour {

	// TEMPORARY TESTING STUFF
	public string botResourceName;
	public Waypoint botSpawnWaypoint;
	// END OF TESTING

	public int time = 60;

    public Text instruction;

	public GameObject standbyCamera;
	SpawnSpot[] spawnSpots;

	public bool offlineMode = false;

	bool connecting = false;

	List<string> chatMessages;
	int maxChatMessages = 5;

	public float respawnTimer = 0;
	public int ScoreCounter = 0;
	public GameObject guiBipod;
	public GameObject guiGod;
	public Text guiScore;

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

	public void startCountdown() {
        StartCoroutine (countdown ());
        Debug.Log ("YES TIMER START!");
    }

    IEnumerator countdown() {
        while (time > 0) {
            Debug.Log ("TIMER countdown  time = " + time + " and instruction = " + instruction);
            yield return new WaitForSeconds(1);

            instruction.text = time.ToString();

            time -= 1;
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
		guiScore.text = ScoreCounter + "";

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

//		gameMode = (string)PhotonNetwork.room.customProperties["seed"];

		//If press ESCAPE
		if (GUILayout.Button("Return to Lobby"))
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
				SpawnPlayer(1);
											startCountdown();

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

		guiBipod.SetActive(true);

	}

	void SpawnMonster() {
		//GameObject botCount = GameObject.FindGameObjectWithTag("Bot");
		//if (botCount == null) {
			GameObject botGO = (GameObject)PhotonNetwork.Instantiate(botResourceName, botSpawnWaypoint.transform.position, botSpawnWaypoint.transform.rotation, 0);
			((MonoBehaviour)botGO.GetComponent("BotController")).enabled = true;
			botGO.GetComponent<BotController>().aggroRange = (int)PhotonNetwork.room.customProperties["ai"];
		//}

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

		guiBipod.SetActive(false);
		guiGod.SetActive(true);
	}

	void Update() {
		if(respawnTimer > 0) {
			respawnTimer -= Time.deltaTime;

			if(respawnTimer <= 0) {
				// Time to respawn the player!
				SpawnGod(teamID);

				ScoreCounter--;
				guiScore.text = ScoreCounter + "";
			}
		}

		if(ScoreCounter <= 0) {
			doGameOver(false);
		}
	}

public void doGameOver(bool win){

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
