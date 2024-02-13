using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Photon.Pun.UtilityScripts;

public class LobbyManager : MonoBehaviourPunCallbacks
{ 
    public InputField roomInputField;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public Text roomName;

    public Button startButton;

    List<RoomItem> roomItemList = new List<RoomItem>();
    public RoomItem roomItemPrefab;
    public Transform contentObjects;

    public Dropdown gameMode;
    public Dropdown maxPlayers;

    public float timeBetweenUpdates = 1.5f;
    float nextUpdateTime;

    public PlayerItem playerItemPrefab;
    public Transform playerItemParentA;
    public Transform playerItemParentB;
    public Transform AIPanel;

    private const string playerItemPrefabPath = "SiHyun/Prefabs/PlayerItem";
    public static bool isFightAI = false;

    private void Start()
    {
        PhotonNetwork.JoinLobby();
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public void OnClickCreate()
    {
        if(roomInputField.text.Length >= 1)
        {
            RoomOptions roomOptions = new RoomOptions()
            {
                MaxPlayers = -(maxPlayers.value - 10),
                IsOpen = true,
                IsVisible = true,
            };

            PhotonNetwork.CreateRoom(roomInputField.text, roomOptions);
        }

        if(gameMode.value == 1)
        {
            playerItemParentB.parent.gameObject.SetActive(false);
            AIPanel.gameObject.SetActive(true);
            isFightAI = true;
        }
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if(Time.time >=nextUpdateTime)
        {
            Debug.Log("방 목록 업데이트");
            UpdateRoomList(roomList);
            nextUpdateTime = Time.time + timeBetweenUpdates;
        }
    }

    void UpdateRoomList(List<RoomInfo> list)
    {
        foreach (RoomItem item in roomItemList)
        {
            Destroy(item.gameObject);
        }
        roomItemList.Clear();

        foreach (RoomInfo room in list)
        {
            RoomItem newRoom = Instantiate(roomItemPrefab, contentObjects);
            //Photon.Realtime.Player masterClient = PhotonNetwork.CurrentRoom.GetPlayer(room.masterClientId);
            newRoom.SetRoomInfo(room.Name, room.PlayerCount, room.MaxPlayers, ""/*masterClient.NickName*/);
            roomItemList.Add(newRoom);
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.AuthValues.UserId);
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        UpdateRoomUI();
    }

    void UpdateRoomUI()
    {
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        CreateLocalPlayerItem(PhotonNetwork.LocalPlayer);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player _newPlayer)
    {
        CreatePlayerItem(_newPlayer);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {

    }

    public override void OnLeftRoom()
    {
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    void CreateLocalPlayerItem(Photon.Realtime.Player _localPlayer)
    {
        if (PlayerItem.localPlayerItemInstance == null)
        {
            GameObject _playerItemObj = 
                PhotonNetwork.Instantiate(playerItemPrefabPath, Vector3.zero, Quaternion.identity);
            _playerItemObj.transform.SetParent(playerItemParentA);
        }
    }

    void CreatePlayerItem(Photon.Realtime.Player _newPlayer)
    {
        if (PlayerItem.localPlayerItemInstance == null)
        {
            GameObject _playerItemObj = 
                PhotonNetwork.Instantiate(playerItemPrefabPath, Vector3.zero, Quaternion.identity);
            PlayerItem _playerItem = _playerItemObj.GetComponent<PlayerItem>();
            _playerItem.transform.SetParent(playerItemParentA);

            if(_playerItem.photonView.IsMine)
            {
                _playerItem.SetPlayerInfo(_newPlayer, PunTeams.Team.red);
            }
        }
    }

    private void Update()
    {
        if (lobbyPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("SiHyun MainLobby Test");
            }
        }

        if(!PhotonNetwork.IsMasterClient)
        {
            startButton.gameObject.SetActive(false);
        }
        else
        {
            startButton.gameObject.SetActive(true);
        }
    }
}
