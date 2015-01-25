// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkerMenu.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using SystemHashtable = System.Collections.Hashtable;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SessionSetup : MonoBehaviour
{
    public Vector2 WidthAndHeight = new Vector2(600,400);
    private string roomName = "myRoom";

    private Vector2 scrollPos = Vector2.zero;

    private bool connectFailed = false;

    public static readonly string SceneNameMenu = "lobby";

    public static readonly string SceneNameGame = "default-scene";

    public static string gameLevel;

    private static float maxPlayers = 3;
    private static float aiAggression = 50;
    private static bool godBool = false;
    private static bool hudBool = false;


    //UI SETTINGS
    public GameObject userCountLabel;
    public RectTransform userName;
    public RectTransform currentGamesList;
    public RectTransform playerCounter;
    public RectTransform aiCounter;
    public RectTransform HUDcounter;
    public RectTransform godCounter;

    private string errorDialog;
    private double timeToClearDialog;
    public string ErrorDialog
    {
        get
        {
            return errorDialog;
        }
        private set
        {
            errorDialog = value;
            if (!string.IsNullOrEmpty(value))
            {
                timeToClearDialog = Time.time + 4.0f;
            }
        }
    }

    public void Awake()
    {
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.automaticallySyncScene = true;

        // the following line checks if this client was just created (and not yet online). if so, we connect
        if (PhotonNetwork.connectionStateDetailed == PeerState.PeerCreated)
        {
            // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
            PhotonNetwork.ConnectUsingSettings("0.9");
        }

        // generate a name for this player, if none is assigned yet
        if (String.IsNullOrEmpty(PhotonNetwork.playerName))
        {
            PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
        }
            userName.transform.parent.GetChild(0).GetComponent<Text>().text = PhotonNetwork.playerName;


    }

    public void Update() {

        if (!PhotonNetwork.connected)
        {
            if (PhotonNetwork.connecting)
            {
                userCountLabel.GetComponent<Text>().text = "Connecting to: " + PhotonNetwork.ServerAddress;
            }
            else
            {
                userCountLabel.GetComponent<Text>().text = "Not connected. Check console output. Detailed connection state: " + PhotonNetwork.connectionStateDetailed + " Server: " + PhotonNetwork.ServerAddress;
            }

            if (this.connectFailed)
            {
                userCountLabel.GetComponent<Text>().text = String.Format("Server: {0}", new object[] {PhotonNetwork.ServerAddress});
            }

            return;
        }

        //Settings Update
        userCountLabel.GetComponent<Text>().text = PhotonNetwork.countOfPlayers + " users are online in " + PhotonNetwork.countOfRooms + " rooms.";
        if (String.IsNullOrEmpty(PhotonNetwork.playerName))
            PhotonNetwork.playerName = userName.GetComponent<Text>().text;
        maxPlayers = playerCounter.GetComponent<Slider>().value;
        aiAggression = aiCounter.GetComponent<Slider>().value;
        hudBool = HUDcounter.GetComponent<Toggle>().isOn;
        godBool = godCounter.GetComponent<Toggle>().isOn;

    }

    public void btnJoinMatch(){
        //PhotonNetwork.JoinRandomRoom();
        if (PhotonNetwork.GetRoomList().Length == 0)
        {
            currentGamesList.GetComponent<Text>().text = "Currently no games are available.";
        }
        else
        {
            currentGamesList.GetComponent<Text>().text = "";
            //GUILayout.Label(PhotonNetwork.GetRoomList().Length + " rooms available:");

                // Room listing: simply call GetRoomList: no need to fetch/poll whatever!
                foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList())
                {
                    if(GameObject.Find(roomInfo.name) == null) {
                        RectTransform t = (RectTransform)Instantiate(Resources.Load("SF Button", typeof(RectTransform)));
                        t.name = roomInfo.name;
                        t.SetParent(currentGamesList.GetChild(1), false);
                        t.GetChild(0).GetChild(0).GetComponent<Text>().text = roomInfo.name + " " + roomInfo.playerCount + "/" + roomInfo.maxPlayers;

                        Button b = t.gameObject.GetComponent<Button>();
                        b.onClick.AddListener(delegate
                        {
                            PhotonNetwork.JoinRoom(roomInfo.name);
                        });
                    }
                }
        }
    }

    // We have two options here: we either joined(by title, list or random) or created a room.
    public void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
    }

    public void btnStartMatch(){
        /*
        this.roomName = GUILayout.TextField(this.roomName);
        maxPlayers = GUILayout.TextField(maxPlayers);
        */
        string[] roomPropsInLobby = { "map", "ai" };
        PhotonHashtable customRoomProperties = new PhotonHashtable() { { "map", 1 } };
        customRoomProperties.Add("seed", "hello");
        customRoomProperties.Add("ai", (int)aiAggression);
        customRoomProperties.Add("hud", hudBool);
        customRoomProperties.Add("god", godBool);
        PhotonNetwork.CreateRoom(this.roomName + Random.Range(1, 9999), true, true, (int)maxPlayers, customRoomProperties, roomPropsInLobby);
    }

    public void OnPhotonCreateRoomFailed()
    {
        this.ErrorDialog = "Error: Can't create room (room name maybe already used).";
        Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
    }

    public void OnPhotonJoinRoomFailed()
    {
        this.ErrorDialog = "Error: Can't join room (full or unknown room name).";
        Debug.Log("OnPhotonJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
    }
    public void OnPhotonRandomJoinFailed()
    {
        this.ErrorDialog = "Error: Can't join random room (none found).";
        Debug.Log("OnPhotonRandomJoinFailed got called. Happens if no room is available (or all full or invisible or closed). JoinrRandom filter-options can limit available rooms.");
    }

    public void OnSetRoom(string name)
    {
        gameLevel = name;
        //customRoomProperties.Add("pc", name);
    }

    public void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
        PhotonNetwork.LoadLevel(gameLevel);
    }

    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from Photon.");
    }

    public void OnFailedToConnectToPhoton(object parameters)
    {
        this.connectFailed = true;
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.networkingPeer.ServerAddress);
    }
}
