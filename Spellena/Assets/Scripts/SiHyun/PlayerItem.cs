using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;

public class PlayerItem : MonoBehaviourPunCallbacks
{
    public Text playerName;

    public static GameObject localPlayerItemInstance;
    public GameObject popUpButton;
    public GameObject playerItem;

    GameObject targetObject;
    Transform playerItemParent;

    public ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

    public Photon.Realtime.Player player;

    private string userId;
    private string userName;
    private string firebaseUserId;
    private bool wasSetSibiling = false;

    // Start is called before the first frame update
    private void Awake()
    {
        if(photonView.IsMine)
        {
            localPlayerItemInstance = this.gameObject;
        }
        if(PhotonNetwork.CurrentRoom.PlayerCount < 5)
        {
            TeamChanged("TeamAList");
        }
        else
        {
            TeamChanged("TeamBList");
        }
    }

    private void OnEnable()
    {
        if(photonView.IsMine && photonView.Owner != null)
        {
            if(photonView.Owner.CustomProperties.ContainsKey("UserName"))
            {
                string _playerName = (string)photonView.Owner.CustomProperties["UserName"];
                playerName.text = _playerName;
            }
        }
    }

    private void Update()
    {
        GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        playerName.text = photonView.Owner.NickName;
        if (photonView.IsMine && !photonView.Owner.IsMasterClient && !wasSetSibiling)
        {
            wasSetSibiling = true;
            this.transform.SetAsLastSibling();
        }
        if (PhotonNetwork.IsMasterClient)
        {
            popUpButton.SetActive(true);
        }
        else if(!PhotonNetwork.IsMasterClient)
        {
            popUpButton.SetActive(false);
        }
    }

    private void TeamChanged(string _teamList)
    {
        targetObject = GameObject.Find(_teamList);
        playerItemParent = targetObject.transform;
        this.transform.SetParent(playerItemParent);
    }

    public void SetPlayerInfo(Photon.Realtime.Player _player, PunTeams.Team _team)
    {
        string userId = FirebaseLoginManager.Instance.GetUser().UserId;
        userName = photonView.Owner.NickName;
        playerName.text = userName;

        // Firebase에서 가져온 사용자 정보를 Photon.Player의 커스텀 프로퍼티에 저장
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
        customProperties["UserId"] = userId;
        customProperties["UserName"] = userName;
        _player.SetCustomProperties(customProperties);

        player = _player;
        player.SetTeam(_team);
    }

    public void SetPlayerInfo(string _userId, string _userName)
    {
        userId = _userId;
        userName = _userName;
        playerName.text = photonView.Owner.NickName;

        if (photonView.IsMine && photonView.Owner != null)
        {
            ExitGames.Client.Photon.Hashtable _customProperties = new ExitGames.Client.Photon.Hashtable();
            _customProperties["UserId"] = _userId;
            _customProperties["UserName"] = _userName;
            photonView.Owner.SetCustomProperties(_customProperties);
        }
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

    public void OnClickKickPlayer()
    {
        ExitGames.Client.Photon.Hashtable _hastable
            = new ExitGames.Client.Photon.Hashtable();
        _hastable.Add("isKicked", true);
        player.SetCustomProperties(_hastable);
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if(targetPlayer == PhotonNetwork.LocalPlayer)
        {
            // isKicked property가 존재할경우
            if (changedProps["isKicked"] !=null)
            {
                // isKicked property가 true일 경우에만 진행
                if ((bool)changedProps["isKicked"])
                {
                    string[] _removeProperties = new string[1];
                    _removeProperties[0] = "isKicked";
                    PhotonNetwork.RemovePlayerCustomProperties(_removeProperties);
                    PhotonNetwork.LeaveRoom();
                }
            }
        }
        else
        {
            Debug.Log(targetPlayer.NickName + " : 로컬 플레이어 아님");
        }
    }

    [PunRPC]
    public void TeamChangedRPC(string _teamList)
    {
        targetObject = GameObject.Find(_teamList);
        playerItemParent = targetObject.transform;
        this.transform.SetParent(playerItemParent);
    }

    public PlayerItem GetPlayerItem()
    {
        return this;
    }

}
