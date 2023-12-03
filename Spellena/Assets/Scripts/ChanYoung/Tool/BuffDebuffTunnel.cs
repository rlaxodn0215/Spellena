using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.Rendering;

public class BuffDebuffTunnel : MonoBehaviourPunCallbacks
{
    public GameObject TentacleTrigger;

    List<string> tentacleHitObjects = new List<string>();
    List<float> tentacleHitCoolDownTime = new List<float>();

    private void Start()
    {
        Init();
    }

    private void FixedUpdate()
    {
        for(int i = 0; i < tentacleHitObjects.Count; i++)
        {
            if (tentacleHitCoolDownTime[i] > 0f)
                tentacleHitCoolDownTime[i] -= Time.deltaTime;
        }
    }


    void Init()
    {
        TentacleTrigger.SetActive(false);
        TentacleTrigger.GetComponent<TriggerEventer>().hitTriggerEventWithMe += TriggerEvent;
    }

    public void ActiveBuffDebuff(string buffDebuff)
    {
        if (buffDebuff == "TerribleTentacle")
            TentacleTrigger.SetActive(true);
    }

    public void InactiveBuffDebuff(string buffDebuff)
    {
        if (buffDebuff == "TerribleTentacle")
        {
            tentacleHitObjects.Clear();
            tentacleHitCoolDownTime.Clear();
            TentacleTrigger.SetActive(false);
        }
    }

    void CallRPCTunnel(string tunnelCommand, params object[] data)
    {
        object[] _tempData;

        if (tunnelCommand == "AddNewObjectTentacle"
            || tunnelCommand == "ResetCoolDownTimeTentacle")
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
            _tempData[1] = (string)data[0];
        }
        else
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
        }

        photonView.RPC("CallRPCTunnelOnBuffDebuffTunnel", RpcTarget.All, _tempData);
    }

    [PunRPC]
    public void CallRPCTunnelOnBuffDebuffTunnel(object[] data)
    {
        if ((string)data[0] == "AddNewObjectTentacle")
            AddNewObjectTentacle(data);
        else if ((string)data[0] == "ResetCoolDownTimeTentacle")
            ResetCoolDownTimeTentacle(data);
    }

    void ResetCoolDownTimeTentacle(object[] data)
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            for(int i = 0; i < tentacleHitObjects.Count; i++)
            {
                if (tentacleHitObjects[i] == (string)data[1])
                {
                    tentacleHitCoolDownTime[i] = 1f;
                    return;
                }
            }
        }
    }

    void AddNewObjectTentacle(object[] data)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            tentacleHitObjects.Add((string)data[0]);
            tentacleHitCoolDownTime.Add(1f);
        }
    }

    void TriggerEvent(GameObject hitObject, GameObject eventObject)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (eventObject.name == "TentacleTrigger")
            {
                if (hitObject.transform.root.GetComponent<Character>() != null)
                {
                    GameObject _rootObject = hitObject.transform.root.gameObject;
                    if (_rootObject.tag != transform.root.tag)
                    {
                        for (int i = 0; i < tentacleHitObjects.Count; i++)
                        {
                            if (_rootObject.name == tentacleHitObjects[i])
                            {
                                if (tentacleHitCoolDownTime[i] > 0f)
                                    return;
                                else
                                {
                                    //플레이어 데미지
                                    tentacleHitCoolDownTime[i] = 1f;
                                    CallRPCTunnel("ResetCoolDownTimeTentacle", _rootObject.name);
                                    _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.All,
                                        transform.root.GetComponent<Character>().playerName,
                                        50, hitObject.name, Vector3.zero, 0f);
                                    Debug.Log("데미지2");
                                    return;
                                }
                            }
                        }
                        //플레이어 데미지
                        Debug.Log("데미지");
                        _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.All,
                                        transform.root.GetComponent<Character>().playerName,
                                        50, hitObject.name, Vector3.zero, 0f);
                        tentacleHitObjects.Add(_rootObject.name);
                        tentacleHitCoolDownTime.Add(1f);
                        CallRPCTunnel("AddNewObjectTentacle", _rootObject.name);
                    }
                }
            }
        }
    }

    public void GetSphereArea(string buffDebuff, Vector3 point)
    {
        //마스터 클라이언트에서 실행됨
        if (buffDebuff == "Horror")
        {
            RaycastHit[] _hits = Physics.SphereCastAll(point, 2f, Vector3.up, 0f);
            List<GameObject> _hitObjects = new List<GameObject>();

            for (int i = 0; i < _hits.Length; i++)
            {
                if (_hits[i].transform.gameObject.GetComponent<Character>() != null)
                {
                    GameObject _enemy = _hits[i].transform.gameObject;
                    if (_enemy.tag != this.transform.root.tag)
                    {
                        int _check = 0;
                        for (int j = 0; j < _hitObjects.Count; j++)
                        {
                            if (_hitObjects[j] == _enemy)
                            {
                                _check = 1;
                                break;
                            }
                        }

                        if (_check == 1)
                            continue;
                        else
                            _hitObjects.Add(_enemy);
                    }
                }
            }

            for (int i = 0; i < _hitObjects.Count; i++)
            {
                _hitObjects[i].GetComponent<BuffDebuffChecker>().SetNewBuffDebuff("Horror", transform.root.GetComponent<PhotonView>().ViewID);
            }
        }
        else if(buffDebuff == "UniteAndOmen")
        {
            RaycastHit[] _hits = Physics.SphereCastAll(point, 5f, Vector3.up, 0f);
            List<GameObject> _deadPlayer = new List<GameObject>();

            for(int i = 0; i < _hits.Length; i++)
            {
                if (_hits[i].transform.root.GetComponent<Character>() != null)
                {
                    GameObject _rootObject = _hits[i].transform.root.gameObject;
                    //죽은 친구
                    if (_rootObject.GetComponent<Character>().Dead.active)
                    {
                        int _check = 0;
                        for(int j = 0; j < _deadPlayer.Count; j++)
                        {
                            if (_deadPlayer[j] == _rootObject)
                            {
                                _check = 1;
                                break;
                            }
                        }

                        if (_check == 1)
                            continue;
                        else
                            _deadPlayer.Add(_rootObject);
                    }
                }
            }

            transform.root.GetComponent<PhotonView>().RPC("AddRitualStack", RpcTarget.All, _deadPlayer.Count);
        }


    }
}
