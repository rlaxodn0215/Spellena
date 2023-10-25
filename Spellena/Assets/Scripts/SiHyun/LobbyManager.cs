using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks

{
    public InputField roomInputField;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public Text roomName;

    List<RoomItem> roomItemList = new List<RoomItem>();
    public RoomItem roomItemPrefab;
    public Transform contentObjects;

    public float timeBetweenUpdates = 1.5f;
    float nextUpdateTime;

    List<PlayerItem> playerItemListA = new List<PlayerItem>();
    List<PlayerItem> playerItemListB = new List<PlayerItem>();
    public PlayerItem playerItemPrefab;
    public Transform playerItemParentA;
    public Transform playerItemParentB;

    private void Start()
    {
        PhotonNetwork.JoinLobby();
    }

    public void OnClickCreate()
    {
        if(roomInputField.text.Length >= 1)
        {
            PhotonNetwork.CreateRoom(roomInputField.text, new RoomOptions()
            {
                MaxPlayers = 10
            });
        }
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if(Time.time >=nextUpdateTime)
        {            
            UpdateRoomList(roomList);
            nextUpdateTime = Time.time + timeBetweenUpdates;
        }
    }

    void UpdateRoomList(List<RoomInfo> list)
    {
        foreach(RoomItem item in roomItemList)
        {
            Destroy(item.gameObject);
        }
        roomItemList.Clear();

        foreach(RoomInfo room in list)
        {
            RoomItem newRoom = Instantiate(roomItemPrefab, contentObjects);
            newRoom.SetRoomName(room.Name);
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

    public override void OnLeftRoom()
    {
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    void UpdatePlayerList()
    {
        // 기존의 플레이어 아이템을 삭제하고 리스트를 초기화
        foreach (PlayerItem item in playerItemListA)
        {
            Destroy(item.gameObject);
        }
        playerItemListA.Clear();

        foreach (PlayerItem item in playerItemListB)
        {
            Destroy(item.gameObject);
        }
        playerItemListB.Clear();

        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        foreach (KeyValuePair<int, Photon.Realtime.Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayerItem = Instantiate(playerItemPrefab, playerItemParentA);
            newPlayerItem.SetPlayerInfo(player.Value);

            if (player.Value == PhotonNetwork.LocalPlayer)
            {
                newPlayerItem.ApplyLocalChanges();
            }

            // 플레이어의 태그를 확인하여 해당 리스트에 추가
            if (newPlayerItem.tag == "TeamA")
            {
                playerItemListA.Add(newPlayerItem);
            }
            else if (newPlayerItem.tag == "TeamB")
            {
                playerItemListB.Add(newPlayerItem);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdatePlayerList();
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
        if (Time.time >= nextUpdateTime)
        {
            UpdatePlayerList();
            nextUpdateTime = Time.time + timeBetweenUpdates;
        }
    }

    public void TeamChangedAToB(PlayerItem _playerItem)
    {
        playerItemListA.Remove(_playerItem);
        playerItemListB.Add(_playerItem);
        _playerItem.transform.SetParent(playerItemParentB);
    }

    public void TeamChangedBToA(PlayerItem _playerItem)
    {
        playerItemListB.Remove(_playerItem);
        playerItemListA.Add(_playerItem);
        _playerItem.transform.SetParent(playerItemParentA);
    }
}
