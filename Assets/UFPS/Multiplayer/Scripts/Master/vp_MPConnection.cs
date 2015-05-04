/////////////////////////////////////////////////////////////////////////////////
//
//	vp_MPConnection.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	initiates and manages the connection to Photon Cloud, regulates
//					room creation, max player count per room and logon timeout.
//					also keeps the 'isMultiplayer' and 'isMaster' flags up-to-date.
//					(these are quite often relied upon by Base UFPS classes)
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class vp_MPConnection : Photon.MonoBehaviour
{

	// connection state
	public int MaxPlayersPerRoom = 16;			// if all available rooms have exactly this many players, the next player who joins will automatically create a new room
	public float LogOnTimeOut = 5.0f;			// if a stage in the initial connection process stalls for more than this many seconds, the connection will be restarted
	public static bool StayConnected = false;	// as long as this is true, this component will relentlessly try to reconnect to the photon cloud
	// public int MaxConnectionAttempts = 10;	// TODO

	protected int m_ConnectionAttempts = 0;
	protected PeerState m_LastPeerState = PeerState.Uninitialized;
	protected vp_Timer.Handle m_ConnectionTimer = new vp_Timer.Handle();

	protected static Dictionary<Transform, int> m_ViewIDsByTransform = new Dictionary<Transform, int>();
	protected static Dictionary<int, Transform> m_TransformsByViewID = new Dictionary<int, Transform>();


	/// <summary>
	/// 
	/// </summary>
	protected virtual void OnEnable()
	{

		vp_Gameplay.isMultiplayer = true;

	}


	/// <summary>
	/// 
	/// </summary>
	protected virtual void OnDisable()
	{

		vp_Gameplay.isMultiplayer = false;

	}


	/// <summary>
	/// 
	/// </summary>
	protected virtual void Start()
	{

		if(StayConnected)
			Connect();

	}


	/// <summary>
	/// 
	/// </summary>
	protected virtual void Update()
	{

		UpdateConnectionState();

	}
	

	/// <summary>
	/// 
	/// </summary>
	protected virtual void CreateRoom()
	{
		//vp_MPDebug.Log("trying to create room: " + "Room" + (PhotonNetwork.countOfRooms + 1).ToString());
		PhotonNetwork.CreateRoom("Room" + (PhotonNetwork.countOfRooms + 1).ToString());
	}


	/// <summary>
	/// 
	/// </summary>
	protected virtual bool JoinRoom()
	{
		//vp_MPDebug.Log("trying to join room: " + "Room" + (PhotonNetwork.countOfRooms).ToString());
		return PhotonNetwork.JoinRoom("Room" + (PhotonNetwork.countOfRooms).ToString());
	}



	/// <summary>
	///	detects cases where the connection process has stalled,
	///	disconnects and tries to connect again
	/// </summary>
	protected virtual void UpdateConnectionState()
	{

		if (!StayConnected)
			return;

		if (PhotonNetwork.connectionStateDetailed != m_LastPeerState)
		{
			string s = PhotonNetwork.connectionStateDetailed.ToString();
			s = ((PhotonNetwork.connectionStateDetailed == PeerState.Joined) ? "--- " + s + " ---" : s);
			vp_MPDebug.Log(s);
		}

		if (PhotonNetwork.connectionStateDetailed == PeerState.Joined)
		{
			if (m_ConnectionTimer.Active)
			{
				m_ConnectionTimer.Cancel();
				m_ConnectionAttempts = 0;
			}
		}
		else if ((PhotonNetwork.connectionStateDetailed != m_LastPeerState) && !m_ConnectionTimer.Active)
		{
			vp_Timer.In(LogOnTimeOut, delegate()
			{
				m_ConnectionAttempts++;
				vp_MPDebug.Log("Retrying (" + m_ConnectionAttempts + ") ...");
				//UnityEngine.Debug.Log("Retrying (" + m_ConnectionAttempts + ") ...");
				Disconnect();
				Connect();
				m_LastPeerState = PeerState.Uninitialized;
			}, m_ConnectionTimer);
		}

		m_LastPeerState = PhotonNetwork.connectionStateDetailed;

	}


	/// <summary>
	/// 
	/// </summary>
	protected virtual void Connect()
	{

		PhotonNetwork.ConnectUsingSettings("0.1");

	}


	/// <summary>
	/// 
	/// </summary>
	protected virtual void Disconnect()
	{

		if (PhotonNetwork.connectionStateDetailed == PeerState.Disconnected)
			return;

		if (PhotonNetwork.connectionStateDetailed == PeerState.PeerCreated)
			return;

		PhotonNetwork.Disconnect();

	}

	
	/// <summary>
	/// caches and returns the photonview id of the given transform.
	/// ids are stored in a dictionary that resets on level load
	/// </summary>
	public static int GetViewIDOfTransform(Transform t)
	{

		if (t == null)
			return 0;

		int id = 0;

		if (!m_ViewIDsByTransform.TryGetValue(t, out id))
		{
			PhotonView p = t.GetComponent<PhotonView>();
			if (p != null)
				id = p.viewID;
			m_ViewIDsByTransform.Add(t, id);	// add (even if '0' to prevent searching again)
		}

		return id;

	}


	/// <summary>
	/// caches and returns the transform of the given photonview id.
	/// transforms are stored in a dictionary that resets on level load
	/// </summary>
	public static Transform GetTransformOfViewID(int id)
	{

		Transform t = null;

		if (!m_TransformsByViewID.TryGetValue(id, out t))
		{
			PhotonView p = PhotonView.Find(id);
			if (p != null)
			{
				t = p.transform;
				m_TransformsByViewID.Add(id, p.transform);
				return p.transform;
			}
			m_TransformsByViewID.Add(id, t);	// add (even if not found, to avoid searching again)
		}

		return t;

	}

	
	/// <summary>
	/// 
	/// </summary>
	void OnPhotonRandomJoinFailed()
	{
		//PhotonNetwork.CreateRoom(null);	// TODO
	}


	/// <summary>
	/// 
	/// </summary>
	void OnJoinedLobby()
	{

		// get player name from the main menu
		// TODO: currently fetched using globalevent. move to vp_Gameplay in upcoming UFPS
		PhotonNetwork.player.name = vp_GlobalEventReturn<string>.Send("PlayerName", vp_GlobalEventMode.REQUIRE_LISTENER);

		//vp_MPDebug.Log("Total players using app: " + PhotonNetwork.countOfPlayers);

		if ((PhotonNetwork.countOfPlayersInRooms % MaxPlayersPerRoom) == 0)
			CreateRoom();
		else
			JoinRoom();

	}

	
	/// <summary>
	/// 
	/// </summary>
	void OnJoinedRoom()
	{
		
		if (PhotonNetwork.isMasterClient)
			PhotonNetwork.room.maxPlayers = MaxPlayersPerRoom;

		// TODO: use PhotonNetwork.LoadLevel to load level while automatically pausing network queue
		// ("call this in OnJoinedRoom to make sure no cached RPCs are fired in the wrong scene")
		// also, get level from room properties / master

		// send spawn request to master client
		string name = "Unnamed";

		// sent as RPC instead of in 'OnPhotonPlayerConnected' because the
		// MasterClient does not run the latter for itself + we don't want
		// to do the request on all clients

		if(FindObjectOfType<vp_MPMaster>())	// in rare cases there might not be a vp_MPMaster, for example: a chat lobby
			photonView.RPC("RequestInitialSpawnInfo", PhotonTargets.MasterClient, PhotonNetwork.player, 0, name);

		vp_Gameplay.isMaster = PhotonNetwork.isMasterClient;

	}


	/// <summary>
	/// 
	/// </summary>
	void OnPhotonPlayerConnected(PhotonPlayer player)
	{

		//Debug.Log("Player joined: " + player.ID);

	}

	
	/// <summary>
	/// updates the 'isMaster' flag which gets read by Base UFPS classes
	/// </summary>
	void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{

		vp_Gameplay.isMaster = PhotonNetwork.isMasterClient;

	}

		
	/// <summary>
	/// 
	/// </summary>
	protected virtual void OnLevelWasLoaded()
	{
		m_ViewIDsByTransform.Clear();
		m_TransformsByViewID.Clear();
	}



}

