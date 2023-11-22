using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class MatchMaking : MonoBehaviourPunCallbacks
{
    public GameObject loadingPanel;
    public Text currentPlayerCount;
    public Text playerNameText;
    private LoadBalancingClient loadBalancingClient;
    EnterRoomParams enterRoomParams = new EnterRoomParams();

    public GameObject matchStartButton;

    static bool ismaster = true;

    private void Awake()
    { 
        PhotonNetwork.AutomaticallySyncScene = true;

        loadingPanel.SetActive(false);

        // OnConnectedToMaster 이벤트에 대한 리스너 등록
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        GetUserName();
    }

    async void GetUserName()
    {
        var _user = FirebaseLoginManager.Instance.GetUser();
        string _userName = await FirebaseLoginManager.Instance.ReadUserInfo(_user.UserId);
        if(!string.IsNullOrEmpty(_userName))
        {
            PhotonNetwork.LocalPlayer.NickName = _userName;
            playerNameText.text = PhotonNetwork.LocalPlayer.NickName;
        }
    }

    public void JoinRandomOrCreateRoom()
    {
        PhotonNetwork.JoinRandomOrCreateRoom(
            expectedCustomRoomProperties: new ExitGames.Client.Photon.Hashtable(),
            expectedMaxPlayers: 10,
            matchingType: MatchmakingMode.FillRoom,
            typedLobby: null,
            sqlLobbyFilter: "",
            expectedUsers: null);
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

    public void OnClickLoad()
    {
        SceneManager.LoadScene("SiHyun RoomLobby Test");
    }

    public void OnClickGameOff()
    {
        FirebaseLoginManager.Instance.SignOut();
        Application.Quit();
    }

    //포톤 콜백 함수
    #region 

    // PhotonNetwork 연결이 완료되면 호출되는 콜백
    public override void OnConnectedToMaster()
    {
        print("서버 접속 완료.");
        FirebaseLoginManager.Instance.SetPhotonId(FirebaseLoginManager.Instance.GetUser().UserId, PhotonNetwork.LocalPlayer.UserId);
        Debug.Log(PhotonNetwork.LocalPlayer.UserId);
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
