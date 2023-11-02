using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Photon.Pun.UtilityScripts;
public class LobbyManager : MonoBehaviourPunCallbacks, IPunObservable
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

    Photon.Realtime.Player player;

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
    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.AuthValues.UserId);
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
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
        
        playerItemListA.Clear();
        playerItemListB.Clear();

        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        GameObject _test;

        foreach (KeyValuePair<int, Photon.Realtime.Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            Transform _playerParent = playerItemParentA;

            if (player.Value.GetTeam() == PunTeams.Team.blue)
            {
                _playerParent = playerItemParentA;
            }
            else if(player.Value.GetTeam() == PunTeams.Team.red)
            {
                _playerParent = playerItemParentB;
            }
            else if(player.Value.GetTeam() == PunTeams.Team.none)
            {
                _playerParent = playerItemParentA;
            }
            
            if(PlayerItem.localPlayerItemInstance == null)
            {
                _test = PhotonNetwork.Instantiate("PlayerItem", new Vector3(0, 0, 0), Quaternion.identity, 0);
                PlayerItem _newPlayerItem = _test.transform.GetComponent<PlayerItem>();


                _newPlayerItem.SetPlayerInfo(player.Value);

                if (player.Value == PhotonNetwork.LocalPlayer)
                {
                    _newPlayerItem.ApplyLocalChanges();
                }

                // 플레이어의 태그를 확인하여 해당 리스트에 추가
                if (_newPlayerItem.tag == "TeamA")
                {
                    playerItemListA.Add(_newPlayerItem);
                }
                else if (_newPlayerItem.tag == "TeamB")
                {
                    playerItemListB.Add(_newPlayerItem);
                }
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
    }

    public void TeamChangedAToB(PlayerItem _playerItem)
    {
        PlayerItem _newPlayerItem;
        _newPlayerItem = _playerItem;
        playerItemListA.Remove(_playerItem);
        _playerItem.photonView.TransferOwnership(_newPlayerItem.player);
        _playerItem.photonView.transform.SetParent(_newPlayerItem.transform);
        playerItemListB.Add(_newPlayerItem);
        _playerItem.transform.SetParent(playerItemParentB);
    }

    public void TeamChangedBToA(PlayerItem _playerItem)
    {
        PlayerItem _newPlayerItem;
        _newPlayerItem = _playerItem;
        playerItemListA.Remove(_playerItem);
        _playerItem.photonView.TransferOwnership(_newPlayerItem.player);
        _playerItem.photonView.transform.SetParent(_newPlayerItem.transform);
        playerItemListB.Add(_newPlayerItem);
        _playerItem.transform.SetParent(playerItemParentA);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {

        }
        else
        {

        }
    }
}
