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

    public Dropdown maxPlayers;

    public float timeBetweenUpdates = 1.5f;
    float nextUpdateTime;

    public PlayerItem playerItemPrefab;
    public Transform playerItemParentA;
    public Transform playerItemParentB;

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
            ExitGames.Client.Photon.Hashtable customProperties =
                new ExitGames.Client.Photon.Hashtable() 
                { 
                    { "GameState", "Waiting" },
                    { "MasterName", PhotonNetwork.LocalPlayer.NickName }
                };
            roomOptions.CustomRoomProperties = customProperties;
            roomOptions.CustomRoomPropertiesForLobby = new string[] { "GameState", "MasterName" };

            PhotonNetwork.CreateRoom(roomInputField.text, roomOptions);
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
        foreach(RoomItem item in roomItemList)
        {
            Destroy(item.gameObject);
        }
        roomItemList.Clear();

        foreach(RoomInfo room in list)
        {
            string masterClientName = room.CustomProperties.ContainsKey("MasterName")
                ? (string)room.CustomProperties["MasterName"] : "N/A";
            string gameState = room.CustomProperties.ContainsKey("GameState")
                ? (string)room.CustomProperties["GameState"] : "Waiting";

            RoomItem newRoom = Instantiate(roomItemPrefab, contentObjects);
            newRoom.SetRoomInfo(room.Name, room.PlayerCount, room.MaxPlayers, masterClientName, gameState);
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
                PhotonNetwork.Instantiate("SiHyun/Prefabs/PlayerItem", Vector3.zero, Quaternion.identity);
            _playerItemObj.transform.SetParent(playerItemParentA);

            /*
            if (_playerItem.photonView.IsMine)
            {
                _playerItem.SetPlayerInfo(_localPlayer, PunTeams.Team.red);
            }
            */

            /*
            CanvasScaler canvasScaler = playerItemParentA.GetComponentInParent<CanvasScaler>();
            if (canvasScaler != null)
            {
                float referenceWidth = canvasScaler.referenceResolution.x;
                float referenceHeight = canvasScaler.referenceResolution.y;

                // 기준 해상도에서의 cellSize 및 spacing
                Vector2 baseCellSize = new Vector2(427f, 93f);
                Vector2 baseSpacing = new Vector2(0f, 3f);

                // 화면 크기에 따라 조절된 cellSize 및 spacing 계산
                float scaleFactorX = canvasScaler.referenceResolution.x / Screen.width;
                float scaleFactorY = canvasScaler.referenceResolution.y / Screen.height;

                Vector2 adjustedCellSize = 
                    new Vector2(baseCellSize.x / scaleFactorX, baseCellSize.y / scaleFactorY);
                Vector2 adjustedSpacing = 
                    new Vector2(baseSpacing.x / scaleFactorX, baseSpacing.y / scaleFactorY);

                GridLayoutGroup _gridLayout = playerItemParentA.GetComponent<GridLayoutGroup>();

                if (_gridLayout != null)
                {
                    _gridLayout.cellSize = adjustedCellSize;
                    _gridLayout.spacing = adjustedSpacing;

                    float verticalSpacing = _gridLayout.spacing.y;
                    float horizontalSpacing = _gridLayout.spacing.x;

                    AdjustChildObjectSizesAndPositions(playerItemParentA.gameObject, adjustedCellSize,
                        _gridLayout.constraintCount, verticalSpacing, horizontalSpacing);
                }
            }
            */
        }
    }

    void CreatePlayerItem(Photon.Realtime.Player _newPlayer)
    {
        if (PlayerItem.localPlayerItemInstance == null)
        {
            GameObject _playerItemObj = 
                PhotonNetwork.Instantiate("SiHyun/Prefabs/PlayerItem", Vector3.zero, Quaternion.identity);
            PlayerItem _playerItem = _playerItemObj.GetComponent<PlayerItem>();
            _playerItem.transform.SetParent(playerItemParentA);

            if(_playerItem.photonView.IsMine)
            {
                //_playerItem.SetPlayerInfo(_newPlayer.UserId, _newPlayer.NickName);
                _playerItem.SetPlayerInfo(_newPlayer, PunTeams.Team.red);
            }


            /*
            CanvasScaler canvasScaler = playerItemParentA.GetComponentInParent<CanvasScaler>();
            if (canvasScaler != null)
            {
                float referenceWidth = canvasScaler.referenceResolution.x;
                float referenceHeight = canvasScaler.referenceResolution.y;

                // 기준 해상도에서의 cellSize 및 spacing
                Vector2 baseCellSize = new Vector2(427f, 93f);
                Vector2 baseSpacing = new Vector2(0f, 3f);

                // 화면 크기에 따라 조절된 cellSize 및 spacing 계산
                float scaleFactorX = canvasScaler.referenceResolution.x / Screen.width;
                float scaleFactorY = canvasScaler.referenceResolution.y / Screen.height;

                Vector2 adjustedCellSize = 
                    new Vector2(baseCellSize.x / scaleFactorX, baseCellSize.y / scaleFactorY);
                Vector2 adjustedSpacing =
                    new Vector2(baseSpacing.x / scaleFactorX, baseSpacing.y / scaleFactorY);

                GridLayoutGroup _gridLayout = playerItemParentA.GetComponent<GridLayoutGroup>();

                if (_gridLayout != null)
                {
                    _gridLayout.cellSize = adjustedCellSize;
                    _gridLayout.spacing = adjustedSpacing;
                    float verticalSpacing = _gridLayout.spacing.y;
                    float horizontalSpacing = _gridLayout.spacing.x;

                    AdjustChildObjectSizesAndPositions(playerItemParentA.gameObject, adjustedCellSize,
                        _gridLayout.constraintCount,verticalSpacing , horizontalSpacing);
                }
            }
            */
        }
    }

    void AdjustChildObjectSizesAndPositions(GameObject parentObject, Vector2 cellSize,
        int constraintCount, float verticalSpacing, float horizontalSpacing)
    {
        // 자식 오브젝트들의 크기 조절
        foreach (Transform child in parentObject.transform)
        {
            RectTransform childRectTransform = child.GetComponent<RectTransform>();

            // 자식 오브젝트의 크기를 부모의 셀 크기에 맞게 설정
            if (childRectTransform != null)
            {
                childRectTransform.sizeDelta = new Vector2(cellSize.x, cellSize.y);

                // 자식 오브젝트의 위치 계산
                int index = child.GetSiblingIndex();
                int row = index / constraintCount;
                int col = index % constraintCount;

                float posX = col * (cellSize.x + verticalSpacing);
                float posY = -row * (cellSize.y + horizontalSpacing);

                childRectTransform.anchoredPosition = new Vector2(posX, posY);
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
