using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MatchMaking : MonoBehaviourPunCallbacks
{
    public GameObject loadingPanel;
    public Text currentPlayerCount;

    private LoadBalancingClient loadBalancingClient;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        loadingPanel.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("서버 연결 시도");
        PhotonNetwork.ConnectUsingSettings();
    }

    public void JoinRandomOrCreateRoom()
    {
        Debug.Log("랜덤 매칭 시작.");
        PhotonNetwork.LocalPlayer.NickName = FirebaseLoginManager.Instance.GetUser().DisplayName;

        byte _maxPlayers = 10;

        RoomOptions _roomOptions = new RoomOptions();
        _roomOptions.MaxPlayers = _maxPlayers;

        PhotonNetwork.JoinRandomOrCreateRoom(
            expectedCustomRoomProperties: new ExitGames.Client.Photon.Hashtable(),
            expectedMaxPlayers: _maxPlayers, // 참가할 때의 기준.
            roomOptions: _roomOptions);      // 생성할 때의 기준.
    }

    public void CancelMatching()
    {
        print("매칭 취소.");
        loadingPanel.SetActive(false);

        print("방 떠남.");
        PhotonNetwork.LeaveRoom();
    }

    private void UpdatePlayerCounts()
    {
        currentPlayerCount.text = $"{PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers}";
    }

    //포톤 콜백 함수
    #region 

    public override void OnConnectedToMaster()
    {
        print("서버 접속 완료.");
    }

    public override void OnJoinedRoom()
    {
        print("방 참가 완료.");
        Debug.Log($"{PhotonNetwork.LocalPlayer.NickName}은 인원수 {PhotonNetwork.CurrentRoom.MaxPlayers} 매칭 기다리는 중.");
        UpdatePlayerCounts();

        loadingPanel.SetActive(true);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"플레이어 {newPlayer.NickName} 방 참가.");
        UpdatePlayerCounts();

        if(PhotonNetwork.IsMasterClient)
        {
            if(PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                PhotonNetwork.LoadLevel("Game");
            }
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"플레이어 {otherPlayer.NickName} 방 나감.");
        UpdatePlayerCounts();
    }

    #endregion
}
