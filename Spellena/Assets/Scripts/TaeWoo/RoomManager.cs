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
        GameObject _gameCenter;
        if (PhotonNetwork.IsMasterClient)
        {
            _gameCenter = PhotonNetwork.Instantiate("ChanYoung/Prefabs/GameCenter", spawnPoint.position, Quaternion.identity);
        }

        player = PhotonNetwork.Instantiate("TaeWoo/Prefabs/Aeterna", spawnPoint.position, Quaternion.identity);
        player.GetComponent<Player.Character>().IsLocalPlayer();
<<<<<<< HEAD
=======

/*<<<<<<< HEAD
=======
    void Update()
    {
        if(isFirstSetting == true)
        {
            GameObject _temp = GameObject.Find("GameCenter(Clone)");
            //Debug.Log(_temp);
            if(_temp != null)
            {
                isFirstSetting = false;
                _temp.GetComponent<GameCenter>().AddPlayer(player);
            }
        }
>>>>>>> a4b25233a3d05e17e5fb4a6706677b7156d515e6*/
>>>>>>> 20c334131e01d851d79ba9cace2c7440dea74943
    }
}

