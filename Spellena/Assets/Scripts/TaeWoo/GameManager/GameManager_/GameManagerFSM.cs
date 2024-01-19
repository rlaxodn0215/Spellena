using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using FSM;

public class GameManagerFSM : StateMachine
{
    public GameObject angleStatue;
    public GameObject playerSpawnPoints;
    public GameObject bgmManagerObj;

    public GameObject characterSelectObj;
    public GameObject inGameUIObj;
    public GameObject deathUIObj;
    public GameObject gameResultObj;
    public GameObject playerStatObj;

    public GameManagerData gameManagerData;

    [HideInInspector]
    public GameManagerStat gameManagerStat;

    void Awake()
    {
        gameManagerStat = new GameManagerStat(this);
    }

    protected override void Update()
    {
        if(PhotonNetwork.IsMasterClient)
            base.Update();
    }

    protected override BaseState GetInitalState()
    {
        return gameManagerStat.GameStates[GameManagerStat.GameState.InTheLobby];
    }

    [PunRPC]
    public void ActiveObject(string name, bool isActive)
    {
        switch (name)
        {
            case "characterSelectObj":
                characterSelectObj.SetActive(isActive);
                break;
            case "inGameUIObj":
                inGameUIObj.transform.GetChild(0).gameObject.SetActive(isActive);
                break;
            case "gameResultObj":
                gameResultObj.SetActive(isActive);
                break;
            case "playerStatObj":
                playerStatObj.SetActive(isActive);
                break;
            case "deathUIObj":
                deathUIObj.SetActive(isActive);
                break;
            default:
                Debug.LogWarning("잘못된 게임 오브젝트 이름 사용");
                break;
        }
    }

}
