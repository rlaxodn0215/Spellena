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

    enum Team
    {
        TeamA,
        TeamB
    }

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
        foreach (PlayerItem item in playerItemListA)
        {
            Destroy(item.gameObject);
        }
        playerItemListA.Clear();

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

            playerItemListA.Add(newPlayerItem);
        }

    }

    List<PlayerItem> GetPlayerList(Team team)
    {
        return team == Team.TeamA ? playerItemListA : playerItemListB;
    }
    void UpdatePlayerListTest()
    {
        List<PlayerItem> playerItemList;
        Transform playerItemParent;
        
        if (teamToUpdate == Team.TeamA)
        {
            playerItemList = playerItemListA;
            playerItemParent = playerItemParentA;
        }
        else if (teamToUpdate == Team.TeamB)
        {
            playerItemList = playerItemListB;
            playerItemParent = playerItemParentB;
        }
        else
        {
            Debug.LogError("유효하지 않은 팀입니다.");
            return;
        }

        // 이동할 팀의 기존 아이템 삭제
        foreach (PlayerItem item in playerItemList)
        {
            Destroy(item.gameObject);
        }
        playerItemList.Clear();

        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        // PhotonNetwork.CurrentRoom.Players를 기반으로 팀 A 또는 팀 B의 아이템 생성
        foreach (KeyValuePair<int, Photon.Realtime.Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayerItem = Instantiate(playerItemPrefab, playerItemParent);
            newPlayerItem.SetPlayerInfo(player.Value);

            if (player.Value == PhotonNetwork.LocalPlayer)
            {
                newPlayerItem.ApplyLocalChanges();
            }

            playerItemList.Add(newPlayerItem);
        }
    }
    Transform GetPlayerItemParent(Team team)
    {
        return team == Team.TeamA ? playerItemParentA : playerItemParentB;
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
    }

    public void TeamChangedAToB(PlayerItem _playerItem)
    {
        playerItemListA.Remove(_playerItem);
        playerItemListB.Add(_playerItem);
        _playerItem.transform.SetParent(playerItemParentB);
        SyncTeamChange(_playerItem.playerName.ToString(), (int)Team.TeamA, (int)Team.TeamB);
    }

    public void TeamChangedBToA(PlayerItem _playerItem)
    {
        playerItemListB.Remove(_playerItem);
        playerItemListA.Add(_playerItem);
        _playerItem.transform.SetParent(playerItemParentA);
        SyncTeamChange(_playerItem.playerName.ToString(), (int)Team.TeamB, (int)Team.TeamA);
    }

    private void SyncTeamChange(string userID, int oldTeam, int newTeam)
    {
        ExitGames.Client.Photon.Hashtable _customProperties = new ExitGames.Client.Photon.Hashtable();
        _customProperties["playerTeam"] = newTeam;
        PhotonNetwork.CurrentRoom.SetCustomProperties(_customProperties);

        RaiseEventOptions _raiseEventOptions = new RaiseEventOptions { CachingOption = EventCaching.DoNotCache };
        /*PhotonNetwork.RaiseEvent(1, new object[] { userID, oldTeam, newTeam },
                                 _raiseEventOptions, SendOptions.SendReliable);*/
    }

    public List<PlayerItem> GetListA()
    {
        return playerItemListA;
    }

    public List<PlayerItem> GetListB()
    {
        return playerItemListB;
    }
}
