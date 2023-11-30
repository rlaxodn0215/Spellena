using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BuffDebuffChecker : MonoBehaviourPunCallbacks
{

    public GameObject tentacleObject;
    NavMeshAgent agent;
    GameObject target;

    public class Tentacle
    {
        public bool isActive = false;
        public float coolDownTime = 2f;
        public float currentCoolDownTime = 0f;
    }

    //촉수 부모 오브젝트
    GameObject[] tentaclesParent = new GameObject[4];
    GameObject[] tentacleOnBody = new GameObject[4];
    //촉수가 가지고 있는 정보
    Tentacle[] tentacles = new Tentacle[4];

    //가지고 있는 버프 디버프
    List<string> buffsAndDebuffs = new List<string>();
    //버프 디버프의 유지 시간
    List<float> leftTime = new List<float>();

    float chasingTimer = 0f;

    private void Start()
    {
        Init();
    }

    void Init()
    {
        //각각 스킬 사용 시 촉수가 나타날 위치
        tentaclesParent[0] = gameObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips).gameObject;
        tentaclesParent[1] = gameObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightUpperArm).gameObject;
        tentaclesParent[2] = gameObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftLowerLeg).gameObject;
        tentaclesParent[3] = gameObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).gameObject;

        agent = GetComponent<NavMeshAgent>();

        for(int i = 0; i < tentacles.Length; i++)
        {
            tentacles[i] = new Tentacle();
        }
    }

    private void FixedUpdate()
    {
        //버프 디버프 시간 처리
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < buffsAndDebuffs.Count; i++)
            {
                if (leftTime[i] > 0f)
                    leftTime[i] -= Time.deltaTime;
            }

            for (int i = 0; i < buffsAndDebuffs.Count; i++)
            {
                if (leftTime[i] <= 0f)
                {
                    RemoveBuffDebuffData(buffsAndDebuffs[i], i);
                    buffsAndDebuffs.RemoveAt(i);
                    leftTime.RemoveAt(i);
                    i = -1;
                }
            }

            //데이터 갱신
            CallRPCTunnel("UpdateData");
        }

        for(int i = 0; i < tentacles.Length; i++)
        {
            if (tentacles[i].isActive == true)
            {
                if (tentacles[i].currentCoolDownTime > 0f)
                    tentacles[i].currentCoolDownTime -= Time.deltaTime;
            }
        }

        if(photonView.IsMine)
        {
            if (agent.enabled == true)
            {
                agent.SetDestination(target.transform.position);
                if (chasingTimer > 0f)
                {
                    chasingTimer -= Time.deltaTime;
                    if (chasingTimer <= 0f)
                    {
                        agent.enabled = false;
                        GetComponent<Character>().camera.transform.localPosition -= new Vector3(0, 0.5f);
                        photonView.RPC("ResetTentacleRequest", RpcTarget.MasterClient);
                    }
                }
            }
        }
    }
    [PunRPC]
    public void ResetTentacleRequest()
    {
        CallRPCTunnel("ResetAllTentacles");
    }

    void CallRPCTunnel(string tunnelCommand, params object[] data)
    {
        object[] _tempData;
        if (tunnelCommand == "UpdateData")
        {
            _tempData = new object[3];
            _tempData[0] = tunnelCommand;
            _tempData[1] = buffsAndDebuffs.ToArray();
            _tempData[2] = leftTime.ToArray();
        }
        else if(tunnelCommand == "AddTentacle")
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
            _tempData[1] = data[0];
        }
        else
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
        }

        photonView.RPC("CallRPCTunnelBuffDebuff", RpcTarget.AllBuffered, _tempData);
    }

    [PunRPC]
    public void CallRPCTunnelBuffDebuff(object[] data)
    {
        if ((string)data[0] == "UpdateData")
            UpdateData(data);
        else if ((string)data[0] == "AddTentacle")
            AddTentacle(data);
        else if ((string)data[0] == "ResetAllTentacles")
            ResetAllTentacles();
        else if ((string)data[0] == "AddTentacleForce")
            AddTentacleForce();
    }

    void ResetAllTentacles()
    {
        for (int i = 0; i < tentacles.Length; i++)
        {
            tentacles[i].isActive = false;
            if (tentacleOnBody[i] != null)
            {
                Debug.Log("쿨타임 끝");
                Destroy(tentacleOnBody[i]);
            }
        }
    }

    void RemoveBuffDebuffData(string buffDebuff, int i)
    {
        if (buffDebuff == "TerribleTentacles")
            CallRPCTunnel("ResetAllTentacles");
    }

    void UpdateData(object[] data)
    {
        buffsAndDebuffs = ((string[])data[1]).ToList();
        leftTime = ((float[])data[2]).ToList();
    }
    public void SetNewBuffDebuff(string buffDebuff, params object[] data)
    {
        for (int i = 0; i < buffsAndDebuffs.Count; i++)
        {
            if (buffsAndDebuffs[i] == buffDebuff)
            {
                UpdateBuffDebuff(buffDebuff, i, data);
                return;
            }
        }
        AddBuffDebuff(buffDebuff, data);
    }

    void AddBuffDebuff(string buffDebuff, object[] data)
    {
        if (buffDebuff == "TerribleTentacles")
        {
            buffsAndDebuffs.Add(buffDebuff);
            leftTime.Add(10f);
            CallRPCTunnel("AddTentacle", (int)data[0]);
        }
    }

    void UpdateBuffDebuff(string buffDebuff, int i, object[] data)
    {
        if(buffDebuff == "TerribleTentacles")
        {
            leftTime[i] = 10f;
            CallRPCTunnel("AddTentacle", (int)data[0]);
        }
    }

    void AddTentacle(object[] data)
    {
        int _count = (int)data[1];
        for (int i = 0; i < tentacles.Length; i++)
        {
            if (tentacles[i].isActive == false)
            {
                _count--;
                tentacles[i].isActive = true;
                tentacles[i].currentCoolDownTime = 0f;
                if (_count <= 0)
                    break;
            }
        }

        //몸에 붙어 있는 촉수 갱신
        UpdateTentacle();

        if (tentacles[0].isActive == true)
        {
            if(photonView.IsMine)
            {
                ChaseNearEnemy();
            }
        }
    }

    void ChaseNearEnemy()
    {
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f);
        List<Collider> _tempList = colliders.ToList();

        _tempList.Sort((a, b) =>
        {
            float distanceToA = Vector3.Distance(transform.position, a.transform.position);
            float distanceToB = Vector3.Distance(transform.position, b.transform.position);
            return distanceToA.CompareTo(distanceToB);
        });

        for(int i = 0; i < _tempList.Count; i++)
        {
            if (_tempList[i].transform.GetComponent<Character>() != null)
            {
                if (tag != _tempList[i].gameObject.tag)
                {
                    agent.enabled = true;
                    target = _tempList[i].gameObject;
                    chasingTimer = 4f;
                    GetComponent<Character>().camera.transform.localPosition += new Vector3(0, 0.5f);
                    break;
                }
            }
        }

        Debug.Log(colliders.Length);
        
    }

    void UpdateTentacle()
    {
        for(int i = 0; i < tentacles.Length; i++)
        {
            if (tentacles[i].isActive == false)
            {
                if (tentacleOnBody[i] != null)
                    Destroy(tentacleOnBody[i]);
            }
            else
            {
                if (tentacleOnBody[i] == null)
                    tentacleOnBody[i] = Instantiate(tentacleObject, tentaclesParent[i].transform);
            }
        }
    }


    
    public bool CheckBuffDebuff(string buffDebuff, params object[] data)
    {
        int _index = -1;
        for(int i = 0; i < buffsAndDebuffs.Count; i++)
        {
            if (buffsAndDebuffs[i] == buffDebuff)
            {
                if (leftTime[i] > 0f)
                {
                    _index = i;
                    break;
                }
            }
        }

        if (_index >= 0)
        {
            if (buffDebuff == "TerribleTentacles")
                return tentacles[(int)data[0]].isActive;
        }

        return false;
    }

    void AddTentacleForce()
    {
        if (photonView.IsMine)
        {
            GetComponent<Rigidbody>().AddForce(transform.forward * 7f, ForceMode.Impulse);
        }
    }
    

    public void UseTerribleTentacles(int index)
    {
        if(tentacles[index].currentCoolDownTime <= 0f)
        {
            tentacles[index].currentCoolDownTime = tentacles[index].coolDownTime;
            CallRPCTunnel("AddTentacleForce");
        }
    }


}
