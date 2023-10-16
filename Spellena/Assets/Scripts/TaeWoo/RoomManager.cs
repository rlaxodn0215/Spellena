using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.PunBasics;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public GameObject player;
    public GameObject gameCenter;

    public Transform spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(message: "Connecting...");

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log(message: "Connected to Server");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        Debug.Log(message: "We're in the Lobby");

        RoomOptions roomOption = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 2
        };

        PhotonNetwork.JoinOrCreateRoom(roomName: "test", roomOptions: roomOption, typedLobby: null);


    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        if(PhotonNetwork.IsMasterClient)
        {
            GameObject _gameCenter = PhotonNetwork.Instantiate("ChanYoung/Prefabs/GameCenter", spawnPoint.position, Quaternion.identity);
        }

        GameObject _player = PhotonNetwork.Instantiate("TaeWoo/Prefabs/Aeterna", spawnPoint.position, Quaternion.identity);
        _player.GetComponent<Player.Charactor>().IsLocalPlayer();
        PhotonNetwork.PlayerList[PhotonNetwork.PlayerList.Length - 1].TagObject = _player;

    }


}

