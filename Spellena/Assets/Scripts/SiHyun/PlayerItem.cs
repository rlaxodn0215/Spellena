using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;

public class PlayerItem : MonoBehaviourPunCallbacks, IPunObservable
{
    public Text playerName;

    public static GameObject localPlayerItemInstance;

    /*public GameObject leftArrowButton;
    public GameObject rightArrowButton;*/
    public GameObject popUpButton;
    LobbyManager lobbyManagerScript;
    public GameObject playerItem;

    GameObject targetObject;
    Transform playerItemParent;

    public ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

    public Photon.Realtime.Player player;

    private string userId;
    private string userName;
    private string firebaseUserId;

    public async void Initialize(string _firebaseUserId)
    {
        firebaseUserId = _firebaseUserId;
        string _photonUserId = await FirebaseLoginManager.Instance.GetUserMapping(_firebaseUserId);

        if(!string.IsNullOrEmpty(_photonUserId))
        {
            Photon.Realtime.Player _targetPlayer = FindPhotonPlayerByUserId(_photonUserId);
            if(_targetPlayer != null)
            {
                playerName.text = _targetPlayer.NickName;
            }
        }
    }

    private Photon.Realtime.Player FindPhotonPlayerByUserId(string _userId)
    {
        Photon.Realtime.Player[] _players = PhotonNetwork.PlayerList;

        foreach(var _player in _players)
        {
            if (_player.UserId == _userId)
            {
                return _player;
            }
        }
        return null;
    }

    // Start is called before the first frame update
    private void Awake()
    {
        lobbyManagerScript = FindAnyObjectByType<LobbyManager>();
        if(photonView.IsMine)
        {
            localPlayerItemInstance = this.gameObject;
        }
        if (!PhotonNetwork.IsMasterClient)
        {
            popUpButton.SetActive(false);
        }
        SetPlayerNameFromFirebase();
        TeamChanged("TeamAList");
    }

    private void TeamChanged(string _teamList)
    {
        targetObject = GameObject.Find(_teamList);
        playerItemParent = targetObject.transform;
        this.transform.SetParent(playerItemParent);
    }

    private async void SetPlayerNameFromFirebase()
    {
        string _userId = FirebaseLoginManager.Instance.GetUser().UserId;
        string _userName = await FirebaseLoginManager.Instance.ReadUserInfo(_userId);
        playerName.text = _userName;
    }

    public void SetPlayerInfo(Photon.Realtime.Player _player, PunTeams.Team _team)
    {
        Debug.Log(PhotonNetwork.IsMasterClient);
        Debug.Log(PhotonNetwork.InRoom);
        Debug.Log(photonView.ViewID);

        string userId = FirebaseLoginManager.Instance.GetUser().UserId;
        //GetUserName(userId);

       FirebaseLoginManager.Instance.ReadUserInfo(userId).ContinueWith(task =>
       {
            if(task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                string userName = task.Result;
                playerName.text = userName;

                // Firebase에서 가져온 사용자 정보를 Photon.Player의 커스텀 프로퍼티에 저장
                ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
                customProperties["UserId"] = userId;
                customProperties["UserName"] = userName;
                _player.SetCustomProperties(customProperties);

                player = _player;
                player.SetTeam(_team);
            }
        });
    }

    public void SetPlayerInfo(string _userId, string _userName)
    {
        userId = _userId;
        userName = _userName;
        playerName.text = userName;
    }

    public void OnPhotonSerializeView(PhotonStream _stream, PhotonMessageInfo _info)
    {
        if(_stream.IsWriting)
        {
            _stream.SendNext(userId);
            _stream.SendNext(userName);
        }
        else
        {
            userId = (string)_stream.ReceiveNext();
            userName = (string)_stream.ReceiveNext();
            playerName.text = userName;
        }
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer != null && targetPlayer != player)
        {
            // 다른 플레이어의 커스텀 프로퍼티가 업데이트되면, 이름을 설정합니다.
            if (changedProps.TryGetValue("UserName", out object userName))
            {
                playerName.text = userName.ToString();
            }
        }
    }

    async void GetUserName(string _userId)
    {
        playerName.text = await FirebaseLoginManager.Instance.ReadUserInfo(_userId);
        Debug.Log(playerName.text);
    }

    public void OnClickLeftArrow()
    {
        TeamChanged("TeamAList");
        playerItem.tag = "TeamA";
        Debug.Log("팀 바뀜");
        photonView.RPC("TeamChangedRPC", RpcTarget.AllBuffered, "TeamAList");
    }

    public void OnClickRightArrow()
    {
        TeamChanged("TeamBList");
        playerItem.tag = "TeamB";
        Debug.Log("팀 바뀜");
        photonView.RPC("TeamChangedRPC", RpcTarget.AllBuffered, "TeamBList");
    }

    [PunRPC]
    public void TeamChangedRPC(string _teamList)
    {
        targetObject = GameObject.Find(_teamList);
        playerItemParent = targetObject.transform;
        this.transform.SetParent(playerItemParent);
    }

    [PunRPC]
    public void ChangedTeam(string _team)
    {
        this.tag = _team;
    }

    public PlayerItem GetPlayerItem()
    {
        return this;
    }

}
