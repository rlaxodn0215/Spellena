using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Photon.Realtime;

public class BuffDebuffChecker : MonoBehaviourPunCallbacks
{
    public GameObject tentacleObject;
    public BuffDebuffTunnel buffDebuffTunnel;
    NavMeshAgent agent;
    GameObject target;

    //컬티스트만 사용 -> 의식 스택
    //라운드 종료시에만 스택이 쌓임
    public int ritualStacks = 0;

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

    float horrorDamageTime = 0.1f;
    float currentHorrorDamageTime = 0f;
    string horrorPlayer;

    //가지고 있는 버프 디버프
    List<string> buffsAndDebuffs = new List<string>();
    //버프 디버프의 유지 시간
    List<float> leftTime = new List<float>();

    //이시스의 축복 연산할 플레이어들
    List<GameObject> blessingTargets = new List<GameObject>();

    public int horrorViewID = -1;

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

    private void Update()
    {
        if(agent.enabled == true)
            agent.baseOffset = Mathf.Lerp(agent.baseOffset, -0.5f, Time.deltaTime * 3f);
        else
            agent.baseOffset = Mathf.Lerp(agent.baseOffset, 0f, Time.deltaTime * 3f);

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
                    if (buffsAndDebuffs[i] == "Horror")
                    {
                        currentHorrorDamageTime = 0f;
                        CallRPCTunnel("SetHorrorViewID", -1);
                    }
                    RemoveBuffDebuffData(buffsAndDebuffs[i], i);
                    buffsAndDebuffs.RemoveAt(i);
                    leftTime.RemoveAt(i);
                    i = -1;
                }
            }

            for (int i = 0; i < buffsAndDebuffs.Count; i++)
            {
                if (buffsAndDebuffs[i] == "Horror")
                {
                    if (currentHorrorDamageTime > 0f)
                    {
                        currentHorrorDamageTime -= Time.deltaTime;
                        if (currentHorrorDamageTime < 0f)
                        {
                            currentHorrorDamageTime = horrorDamageTime;
                            GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.All, horrorPlayer, 10, "",
                               Vector3.zero, 0f);

                            //공포 디버프 데미지
                        }
                    }
                }
                else if (buffsAndDebuffs[i] == "BlessingCast")
                {
                    Debug.Log(blessingTargets.Count);
                    for(int j = 0; j < blessingTargets.Count; j++)
                    {
                        if (blessingTargets[j] == gameObject)
                            continue;
                        Vector3 _viewPort = blessingTargets[j].GetComponent<Character>().camera.GetComponent<Camera>().WorldToViewportPoint(transform.position);

                        //카메라안에 들어오는가 확인
                        if(_viewPort.x >= 0 && _viewPort.x <= 1 && _viewPort.y >= 0 && _viewPort.y <=1)
                        {
                            Vector3 _playerCameraPos = blessingTargets[j].transform.position + new Vector3(0, 1, 0);
                            Vector3 _cameraPos = transform.position + new Vector3(0, 1, 0);
                            Vector3 _direction = _playerCameraPos - _cameraPos;
                            float _distance = _direction.magnitude;
                            Vector3 _normalizedDirection = _direction.normalized;
                            Ray _tempRay = new Ray();
                            _tempRay.direction = _normalizedDirection;
                            _tempRay.origin = _cameraPos;
                            //맵이 있는지 확인 후 없으면 카메라를 밑으로 내려버린다
                            LayerMask _layerMask = LayerMask.GetMask("Map");
                            RaycastHit _tempHit;
                            if ((Physics.Raycast(_tempRay, out _tempHit, _distance, _layerMask)))
                                Debug.Log("벽에 막힘");
                            else
                                blessingTargets[j].GetComponent<BuffDebuffChecker>().CallRPCTunnel("DownCamera");

                        }

                    }
                }
            }

            //데이터 갱신
            CallRPCTunnel("UpdateData");
        }

        for (int i = 0; i < tentacles.Length; i++)
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
        else if (tunnelCommand == "AddTentacle" || tunnelCommand == "SetHorrorViewID")
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
        else if ((string)data[0] == "SetHorrorViewID")
            SetHorrorViewID(data);
        else if ((string)data[0] == "ActiveBlessingCollider")
            ActiveBlessingCollider(data);
        else if ((string)data[0] == "DownCamera")
            DownCamera();
    }

    void DownCamera()
    {
        if(photonView.IsMine)
        {
            MouseControl _mouseControl = GetComponent<Character>().camera.GetComponent<MouseControl>();
            Vector3 _tempEulerCamera = GetComponent<Character>().camera.transform.localEulerAngles;
            Vector3 _tempEuler = transform.localEulerAngles;
            _tempEulerCamera.x += 1f;
            _mouseControl.ApplyPos(_tempEuler.y ,_tempEulerCamera.x);
        }
    }

    void ActiveBlessingCollider(object[]data)
    {
        if ((bool)data[1])
            buffDebuffTunnel.ActiveBuffDebuff("BlessingTrigger");
        else
            buffDebuffTunnel.InactiveBuffDebuff("BlessingTrigger");
    }

    void SetHorrorViewID(object[] data)
    {
        horrorViewID = (int)data[1];
        horrorPlayer = PhotonNetwork.GetPhotonView(horrorViewID).gameObject.name;
        Debug.Log(horrorViewID);
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
                buffDebuffTunnel.InactiveBuffDebuff("TerribleTentacle");
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

    public void SpreadBuffDebuff(string buffDebuff, params object[] data)
    {
        buffDebuffTunnel.GetSphereArea(buffDebuff, (Vector3)data[0]);
    }

    void AddBuffDebuff(string buffDebuff, object[] data)
    {
        if (buffDebuff == "TerribleTentacles")
        {
            buffsAndDebuffs.Add(buffDebuff);
            leftTime.Add(10f);
            CallRPCTunnel("AddTentacle", (int)data[0]);
        }
        else if(buffDebuff == "Horror")
        {
            buffsAndDebuffs.Add(buffDebuff);
            leftTime.Add(0.8f);
            CallRPCTunnel("SetHorrorViewID", (int)data[0]);
        }
        else if(buffDebuff == "BlessingCast")
        {
            buffsAndDebuffs.Add(buffDebuff);
            leftTime.Add((float)data[0]);
            //CallRPCTunnel("ActiveBlessingCollider", true);
            Photon.Realtime.Player[] _players = PhotonNetwork.PlayerList;
            //모든 플레이어를 가져와서 이시스의 축복 스킬을 체크한다
            blessingTargets.Clear();
            for(int i = 0; i < _players.Length; i++)
            {
                int _viewID = (int)_players[i].CustomProperties["CharacterViewID"];
                PhotonView _photonView = PhotonNetwork.GetPhotonView(_viewID);
                if(_photonView.GetComponent<Character>() != null)
                    blessingTargets.Add(_photonView.gameObject);
            }
        }
    }


    void UpdateBuffDebuff(string buffDebuff, int i, object[] data)
    {
        if(buffDebuff == "TerribleTentacles")
        {
            leftTime[i] = 10f;
            CallRPCTunnel("AddTentacle", (int)data[0]);
        }
        else if(buffDebuff == "Horror")
        {
            leftTime[i] = 0.8f;
            CallRPCTunnel("SetHorrorViewID", (int)data[0]);
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

                if (i == 0)
                    buffDebuffTunnel.ActiveBuffDebuff("TerribleTentacle");

                if (_count <= 0)
                    break;
            }
        }

        //몸에 붙어 있는 촉수 갱신
        UpdateTentacle();

        if (tentacles[3].isActive == true)
        {
            if(photonView.IsMine)
                ChaseNearEnemy();
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
            else if (buffDebuff == "Horror")
                return true;
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

    [PunRPC]
    public void AddRitualStack(int stacks)
    {
        ritualStacks += stacks;
    }

}
