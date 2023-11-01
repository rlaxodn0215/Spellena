using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.PunBasics;

public class RoomEnter: MonoBehaviourPunCallbacks
{
    [HideInInspector]
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
            MaxPlayers = 10
        };

        PhotonNetwork.JoinOrCreateRoom(roomName: "test", roomOptions: roomOption, typedLobby: null);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log(message: "We're in the Room");

        GameObject _gameCenter;

        if (PhotonNetwork.IsMasterClient)
        {
            _gameCenter = PhotonNetwork.Instantiate("TaeWoo/Prefabs/GameManager", spawnPoint.position, Quaternion.identity);
        }

        player = PhotonNetwork.Instantiate("TaeWoo/Prefabs/Aeterna", spawnPoint.position, Quaternion.identity);
        player.GetComponent<Player.Character>().IsLocalPlayer();
    }
}

