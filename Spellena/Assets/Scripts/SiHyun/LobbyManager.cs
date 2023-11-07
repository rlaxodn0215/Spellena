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

    public ObjectPool playerItemPool;

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
                MaxPlayers = 14
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
        //playerItemPool.Init();
        //UpdatePlayerList();
        SetUpPlayerInfo(PhotonNetwork.LocalPlayer, PunTeams.Team.red);
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

    void SetUpPlayerInfo(Photon.Realtime.Player _player, PunTeams.Team _team)
    {
        GameObject _playerItem = PhotonNetwork.Instantiate("PlayerItem", Vector3.zero, Quaternion.identity);
        PlayerItem _item = _playerItem.GetComponent<PlayerItem>();
        // PlayerItem에 플레이어 정보를 설정하고 RPC로 동기화
        //_item.SetPlayerInfo(_player, _team);
        SetPlayerInfoRPC(_player, _team); // RPC를 통해 다른 클라이언트에게 정보 전송
        _playerItem.transform.SetParent(playerItemParentA);
    }

    void RemovePlayerItem(Photon.Realtime.Player _player)
    {
        // 플레이어가 방을 떠날 때 해당 플레이어의 PlayerItem을 삭제
        PlayerItem[] playerItems = FindObjectsOfType<PlayerItem>();

        foreach (PlayerItem item in playerItems)
        {
            if (item.photonView.OwnerActorNr == _player.ActorNumber)
            {
                PhotonNetwork.Destroy(item.gameObject);
            }
        }
    }

    [PunRPC]
    void SetPlayerInfoRPC(Photon.Realtime.Player _player, PunTeams.Team _team)
    {
        // RPC로 호출되는 함수로, 플레이어 정보를 설정
        PlayerItem _item = GetComponent<PlayerItem>();

        if (_player.IsLocal)
        {
            // 이 플레이어가 로컬 플레이어일 경우
            _item.SetPlayerInfo(_player, _team);
        }
        else
        {
            // 다른 플레이어일 경우
            _item.SetPlayerInfo(_player, _team);
        }
    }


    void UpdatePlayerList()
    {
        /*Photon.Realtime.Player[] _players = PhotonNetwork.PlayerList;

        int index = 0;
        List<GameObject> _playerItems = playerItemPool.GetPlayerItemPool();

        if(_playerItems != null)
        {
            foreach (GameObject _oldItem in _playerItems)
            {
                PlayerItem _item = _oldItem.GetComponent<PlayerItem>();
                playerItemPool.ReturnPlayerItemPool(_item);
            }
        }

        foreach (Photon.Realtime.Player _player in _players)
        {
            // playerItemPool에서 사용 가능한 플레이어 GameObject를 가져옴
            if (index < _playerItems.Count)
            {
                GameObject _playerItemGO = _playerItems[index];
                PlayerItem _playerItem = _playerItemGO.GetComponent<PlayerItem>();
                _playerItem.SetPlayerInfo(_player);
                index++;
            }
            else
            {
                Debug.LogWarning("플레이어 GameObject가 부족합니다.");
            }
        }*/


        /*Photon.Realtime.Player[] _players = PhotonNetwork.PlayerList;

        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        foreach(Photon.Realtime.Player _player in _players)
        {
            GameObject _playerItemGO = playerItemPool.GetPlayerItemPool();
            PlayerItem _playertItem = _playerItemGO.GetComponent<PlayerItem>();
            _playertItem.SetPlayerInfo(_player);
        }*/

        /*GameObject _test;

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
        }*/
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player _newPlayer)
    {
        SetUpPlayerInfo(_newPlayer, PunTeams.Team.red);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        RemovePlayerItem(otherPlayer);
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
