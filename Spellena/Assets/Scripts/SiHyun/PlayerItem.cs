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

    Image backgroundImage;
    public Color highlightColor;
    public GameObject leftArrowButton;
    public GameObject rightArrowButton;
    LobbyManager lobbyManagerScript;
    public GameObject playerItem;

    public ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

    public Photon.Realtime.Player player;

    enum Team
    {
        TeamA = 1,
        TeamB
    }

    // Start is called before the first frame update
    private void Awake()
    {
        backgroundImage = GetComponent<Image>();
        lobbyManagerScript = FindAnyObjectByType<LobbyManager>();
    }

    private void Update()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            leftArrowButton.SetActive(false);
            rightArrowButton.SetActive(false);
        }
    }

    public void SetPlayerInfo(Photon.Realtime.Player _player)
    {
        string userId = FirebaseLoginManager.Instance.GetUser().UserId;
        GetUserName(userId);

        // Firebase에서 가져온 사용자 정보를 Photon.Player의 커스텀 프로퍼티에 저장
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
        customProperties["UserId"] = userId;
        customProperties["UserName"] = playerName.text;

        _player.SetCustomProperties(customProperties);

        player = _player;
        UpdatePlayerItem(player);
    }

    async void GetUserName(string _userId)
    {
        playerName.text = await FirebaseLoginManager.Instance.ReadUserInfo(_userId);
    }

    public void ApplyLocalChanges()
    {
        backgroundImage.color = highlightColor;
    }

    public void OnClickLeftArrow()
    {
        playerItem.tag = "TeamA";
        leftArrowButton.SetActive(false);
        rightArrowButton.SetActive(true);
        playerProperties["playerTeam"] = (int)Team.TeamA;
        player.SetCustomProperties(playerProperties);
        lobbyManagerScript.TeamChangedBToA(this);
        Debug.Log("팀 바뀜");
    }

    public void OnClickRightArrow()
    {
        string _team = "TeamB";
        photonView.RPC("ChangedTeam", RpcTarget.AllBuffered, _team);
        playerItem.tag = "TeamB";
        leftArrowButton.SetActive(true);
        rightArrowButton.SetActive(false);
        playerProperties["playerTeam"] = (int)Team.TeamB;
        player.SetCustomProperties(playerProperties);
        lobbyManagerScript.TeamChangedAToB(this);
        Debug.Log("팀 바뀜");
    }

    [PunRPC]
    public void ChangedTeam(string _team)
    {
        this.tag = _team;
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if(player == targetPlayer)
        {
            UpdatePlayerItem(targetPlayer);
        }
    }

    public void UpdatePlayerItem(Photon.Realtime.Player player)
    {
        if (player != null && player.CustomProperties.ContainsKey("playerTeam"))
        {
            int team = (int)player.CustomProperties["playerTeam"];
            leftArrowButton.SetActive(team == (int)Team.TeamB);
            rightArrowButton.SetActive(team == (int)Team.TeamA);
        }
    }

    public PlayerItem GetPlayerItem()
    {
        return this;
    }

}
