using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Firebase.Auth;
using System.Threading.Tasks;

public class MatchMaking : MonoBehaviourPunCallbacks
{
    public Text currentPlayerCount;
    public Text playerNameText;

    private FirebaseUser user;

    public GameObject matchButton;

    private void Awake()
    { 
        PhotonNetwork.AutomaticallySyncScene = true;


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
        user = FirebaseLoginManager.Instance.GetUser();
        GetUserName();

        DatabaseReference lobbyMasterRef =
            FirebaseLoginManager.Instance.GetReference().Child("users").Child(user.UserId).Child("isLobbyMaster?");
        lobbyMasterRef.ValueChanged += (sender, args) =>
        {
            Debug.Log("이벤트 핸들러 호출");
            if (args.DatabaseError != null)
            {
                Debug.Log("이벤트 핸들러 호출 실패");
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            try
            {
                Debug.Log("이벤트 핸들러 호출 성공");

                if (args.Snapshot != null)
                {
                    Debug.Log("스냅샷 있음");

                    if ((bool)args.Snapshot.Value)
                    {
                        Debug.Log("방장");
                        matchButton.SetActive(true);
                    }
                    else
                    {
                        Debug.Log("방장 아님");
                        matchButton.SetActive(false);
                    }
                }
                else
                {
                    Debug.Log("스냅샷 없음");
                }

            }
            catch (Exception ex)
            {
                Debug.LogError("Exception in ValueChanged event handler: " + ex.Message);
            }
        };
        Button _matchButton = matchButton.GetComponent<Button>();
        _matchButton.onClick.AddListener(StartMatchMaking);
    }

    async void GetUserName()
    {
        string _userName = await FirebaseLoginManager.Instance.ReadUserInfo(user.UserId);
        if(!string.IsNullOrEmpty(_userName))
        {
            PhotonNetwork.LocalPlayer.NickName = _userName;
            playerNameText.text = PhotonNetwork.LocalPlayer.NickName;
        }
    }

    private void StartMatchMaking()
    {
        PartyMatchMaking();
    }

    private async void PartyMatchMaking()
    {
        await JoinRandomOrCreateRoomAsync();

    }

    private async Task JoinRandomOrCreateRoomAsync()
    {
        List<string> _partyMembers = await FirebaseLoginManager.Instance.GetFriendsList(user.UserId);
        _partyMembers.Insert(0, user.UserId);
        string[] _partys = _partyMembers.ToArray();

        TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

        PhotonNetwork.JoinRandomOrCreateRoom(
            expectedCustomRoomProperties: new ExitGames.Client.Photon.Hashtable(),
            expectedMaxPlayers: 10,
            matchingType: MatchmakingMode.FillRoom,
            typedLobby: null,
            sqlLobbyFilter: "",
            expectedUsers: _partys);
    }

    public void CancelMatching()
    {
        print("매칭 취소.");

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
