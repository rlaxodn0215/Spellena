using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.InputSystem;
using System;
using System.Reflection;
using Unity.VisualScripting;

public class PlayerCommon : MonoBehaviourPunCallbacks, IPunObservable
{
    Photon.Realtime.Player player;

    public PlayerData playerData;
    //[HideInInspector]
    //public GameCenter0 gameCenter;

    protected int hp;
    private float speed = 5f;
    private float jumpForce = 7f;

    //speedRate�� ������ �÷��̾��� �̵��ӵ� ����
    protected float speedRate = 0f;
    protected float jumpForceRate = 1.0f;

    protected bool isGround = false;
    protected bool isAlive = true;
    protected bool isUniqueState = false;

    public Camera cameraMain;
    public Camera cameraOverlay;
    public Camera cameraMinimap;
    public GameObject UI;

    protected GameObject minimapMask;

    protected GameObject unique;

    protected Rigidbody rigidbodyMain;
    protected GameObject AvatarForMe;
    protected GameObject AvatarForOther;

    public Vector2 moveDirection;
    public bool isRunning = false;
    protected bool isClicked = false;

    //�������� �ܺ� ��
    protected Vector3 externalForce = Vector3.zero;
    protected LayerMask layerMaskMap;
    protected LayerMask layerMaskWall;

    public event Action<Vector2, bool> UpdateLowerAnimation;
    public event Action<int> PlayAnimation;

    protected List<SkillData> skillDatas = new List<SkillData>();

    protected bool isCameraLocked = false;
    protected Vector3 pointStrike;

    protected class SkillData
    {
        public float skillCoolDownTime;
        public float skillUniqueTime;
        public float skillCastingTime;
        public float skillChannelingTime;

        public SkillState skillState = SkillState.None;
        public enum SkillState
        {
            None, Unique, Casting, Channeling
        }

        //�����Ϳ��� ������ Ȯ�� ��ȣ
        public bool isReady = false;
        //���ÿ��� �غ� ���� Ȯ��
        public bool isLocalReady = false;
        //Ȧ��, ���� Ÿ�� �� Ư�� �غ� ���°� �ִ��� Ȯ��
        public bool isUnique = false;
    }

    public enum SkillTiming
    {
        Immediately, AfterCasting
    }

    //���� -> None : ����ִ� ����, Ready : �غ� ����
    //������ -> Casting : ĳ�������� �ٲ�� ���� �������� ��Ÿ�� üũ, Channeling : ä�θ��� ������ ��Ÿ�� ����
    //��� ���� �̺�Ʈ -> 
    

    virtual protected void Awake()
    {
        rigidbodyMain = GetComponent<Rigidbody>();

        InitCommonComponents();
        InitUniqueComponents();
    }

    virtual protected void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            MovePlayer();
            CheckPlayerFunction();
        }

        ProgressTime();

    }

    virtual protected void ProgressTime()
    {
        for(int i = 0; i < skillDatas.Count; i++)
        {
            //��Ÿ�� ����
            if (skillDatas[i].skillCoolDownTime > 0f)
                skillDatas[i].skillCoolDownTime -= Time.fixedDeltaTime;

            if (PhotonNetwork.IsMasterClient
                && skillDatas[i].skillCoolDownTime <= 0f && skillDatas[i].isReady == false
                && skillDatas[i].skillCastingTime <= 0f && skillDatas[i].skillChannelingTime <= 0f)
            {
                skillDatas[i].isReady = true;
                photonView.RPC("NotifySkillIsReady", player, i);
            }

            //ĳ���� �ð� ����
            if (skillDatas[i].skillCastingTime > 0f)
            {
                skillDatas[i].skillCastingTime -= Time.fixedDeltaTime;
                if (skillDatas[i].skillCastingTime <= 0f)
                {
                    if (photonView.IsMine)
                        skillDatas[i].skillState = SkillData.SkillState.Channeling;
                    //��ų ä�θ� �ð� ���� Ÿ�ֿ̹� ���ÿ� ����ǹǷ� fixedDeltaTime �� ������ �߰�
                    skillDatas[i].skillChannelingTime = playerData.skillChannelingTime[i] + Time.fixedDeltaTime;
                    //ĳ���� �ð��� �ִ� ��ų�� �� ������ ������ �����
                    PlaySkillLogic(i, SkillTiming.AfterCasting);
                }
            }

            //ä�θ� �ð� ����
            if (skillDatas[i].skillChannelingTime > 0f)
            {
                skillDatas[i].skillChannelingTime -= Time.fixedDeltaTime;
                if (skillDatas[i].skillChannelingTime <= 0 && PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC("NotifySetSkillCoolDownTime", RpcTarget.All, i);
                }
            }
        }
    }

    [PunRPC]
    public void NotifySkillIsReady(int index)
    {
        skillDatas[index].isReady = true;
        Debug.Log("��ų " + index + " �غ��");
    }

    [PunRPC]
    public void NotifySetSkillCoolDownTime(int index)
    {
        //��Ÿ�� ����
        skillDatas[index].skillCoolDownTime = 1f;
        if (photonView.IsMine)
            skillDatas[index].skillState = SkillData.SkillState.None;
    }

    virtual protected void CheckPlayerFunction()
    {
        Ray _jumpRay = new Ray();
        _jumpRay.direction = Vector3.down;
        _jumpRay.origin = transform.position + new Vector3(0, 0.01f, 0);
        RaycastHit _groundHit;

        if (Physics.Raycast(_jumpRay, out _groundHit, 0.02f, layerMaskMap))
            isGround = true;
        else
            isGround = false;
    }

    virtual protected void MovePlayer()
    {
        Vector3 _direction = Vector3.zero;

        if (moveDirection.x > 0)
            _direction += transform.right;
        else if (moveDirection.x < 0)
            _direction -= transform.right;

        if (moveDirection.y > 0)
            _direction += transform.forward;
        else if (moveDirection.y < 0)
            _direction -= transform.forward;

        _direction.Normalize();

        externalForce = Vector3.Lerp(externalForce, Vector3.zero, Time.fixedDeltaTime * 5);

        if (Mathf.Abs(externalForce.x) <= 0.01f)
            externalForce.x = 0;
        if (Mathf.Abs(externalForce.z) <= 0.01f)
            externalForce.z = 0;
        externalForce.y = 0;

        Vector3 _velocity = _direction * speed * speedRate;

        rigidbodyMain.velocity = new Vector3(_velocity.x, rigidbodyMain.velocity.y, _velocity.z) + externalForce;
    }

    protected void InitCommonComponents()
    {
        layerMaskMap = LayerMask.GetMask("Map");
        layerMaskWall = LayerMask.GetMask("Wall");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        AddSkill(4);

        player = photonView.Owner;
        //gameCenter = GameObject.Find("GameCenter").GetComponent<GameCenter0>();
        //if (gameObject == null) Debug.LogError("Can't find gameCenter");

        AvatarForOther = transform.GetChild(1).gameObject;
        AvatarForMe = transform.GetChild(2).gameObject;

        unique = transform.GetChild(0).GetChild(1).gameObject;

        minimapMask = UI.transform.GetChild(0).gameObject;

        //if(photonView.IsMine)
        //    SetLocalPlayer();
    }

    protected void AddSkill(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SkillData _skillData = new SkillData();
            skillDatas.Add(_skillData);
        }
    }

    virtual protected void InitUniqueComponents()
    {

    }

    [PunRPC]
    virtual public void SetLocalPlayer(string team)
    {
        cameraOverlay.gameObject.SetActive(true);
        cameraMain.gameObject.SetActive(true);
        cameraMinimap.gameObject.SetActive(true);

        UI.SetActive(true);

        SkinnedMeshRenderer[] _skinMeshForOther = AvatarForOther.GetComponentsInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer[] _skinMeshForMe = AvatarForMe.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < _skinMeshForOther.Length; i++)
            _skinMeshForOther[i].gameObject.layer = 6;
        for (int i = 0; i < _skinMeshForMe.Length; i++)
            _skinMeshForMe[i].gameObject.layer = 8;

        photonView.RPC("SetTag", RpcTarget.All,team);
    }

    [PunRPC]
    public void SetLocalAI(string team)
    {
        cameraMain.gameObject.SetActive(true);
        photonView.RPC("SetTag", RpcTarget.All, team);
    }

    [PunRPC]
    public void SetTag(string team)
    {
        gameObject.tag = team;
    }

    [PunRPC]
    public void PlayerDamaged(string enemy, int damage, string damagePart, Vector3 direction, float force)
    {
        if (isAlive == false) return;

        if (damage > 0)
        {
            if (damagePart == "head")
                hp -= (int)(damage * playerData.dataHeadShotRatio);
            else
                hp -= damage;

            UI.GetComponent<ScreenEffectManager>().PlayDamageEffect(damage);
            externalForce = direction;
            
            if (hp <= 0)
                isAlive = false;
        }

        else
        {
            hp -= damage;
            if (hp > playerData.dataHp) hp = playerData.dataHp;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            //gameCenter.GetComponent<PhotonView>().RPC("UpdateTotalDamage", RpcTarget.All, player.NickName, enemy, damage);

            if(hp<=0)
            {
                //gameCenter.GetComponent<PhotonView>().RPC("UpdatePlayerDead", RpcTarget.All, player.NickName, enemy);
            }
        }

    }

    //[PunRPC]
    //public virtual void PlayerReBornForAll(Vector3 pos)
    //{
    //    gameObject.transform.position = pos;
    //    gameObject.transform.rotation = Quaternion.identity;
    //    moveVec = Vector3.zero;
    //    isAlive = true;

    //    for (int i = 0; i < ragdollRigid.Length; i++)
    //    {
    //        ragdollRigid[i].transform.localPosition = ragdollPos[i];
    //        ragdollRigid[i].transform.localRotation = ragdollRot[i];
    //        ragdollRigid[i].velocity = new Vector3(0, 0, 0);
    //    }

    //    Alive.SetActive(true);
    //    animator.enabled = true;
    //    GetComponent<CapsuleCollider>().enabled = true;
    //    Dead.SetActive(false);

    //    hp = dataHp;
    //}

    //[PunRPC]
    //public void PlayerReBornPersonal()
    //{
    //    GetComponent<Camera>().transform.SetParent(Alive.transform);

    //    GetComponent<Camera>().transform.localPosition = cameraPos;
    //    GetComponent<Camera>().transform.localRotation = cameraRot;

    //    GetComponent<Camera>().GetComponent<MouseControl>().enabled = true;
    //    GetComponent<Camera>().GetComponent<DeadCamera>().enabled = false;
    //}

    //�Է� �̺�Ʈ -> Ű���� ���� ���� �� ���� ȣ��

    virtual protected void OnMouseMove(InputValue inputValue)
    {
        if (!isCameraLocked)
        {
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + inputValue.Get<Vector2>().x / 5f, 0);

            float _nextAngle = cameraMain.transform.eulerAngles.x - inputValue.Get<Vector2>().y / 5f;
            float _normalizedAngle = GlobalOperation.Instance.NormalizeAngle(_nextAngle);
            if (_normalizedAngle > 60)
                _normalizedAngle = 60;
            else if (_normalizedAngle < -60)
                _normalizedAngle = -60;
            cameraMain.transform.localRotation = Quaternion.Euler(_normalizedAngle, 0, 0);
        }
    }
    virtual protected void OnMove(InputValue inputValue)
    {
        moveDirection = new Vector2(inputValue.Get<Vector2>().x, inputValue.Get<Vector2>().y);
        UpdateLowerAnimation?.Invoke(moveDirection, isRunning);
    }

    virtual protected void OnJump()
    {
        if(isGround && isAlive && photonView.IsMine)
            rigidbodyMain.AddForce(Vector3.up * jumpForce * jumpForceRate, ForceMode.Impulse);
    }

    virtual protected void OnRun()
    {
        isRunning = !isRunning;
        UpdateLowerAnimation?.Invoke(moveDirection, isRunning);
    }

    virtual protected void OnSkill1()
    {
        if (photonView.IsMine)
            if (skillDatas[0].isReady)
                SetSkillReady(0);
    }

    virtual protected void OnSkill2()
    {
        if (photonView.IsMine)
            if (skillDatas[1].isReady)
                SetSkillReady(1);
    }

    virtual protected void OnSkill3()
    {
        if (photonView.IsMine)
            if (skillDatas[2].isReady)
                SetSkillReady(2);
    }

    virtual protected void OnSkill4()
    {
        if (photonView.IsMine)
            if (skillDatas[3].isReady)
                SetSkillReady(3);
    }

    virtual protected void OnButtonCancel()
    {
        if (photonView.IsMine)
            if(!IsSkillProgressing())
                CancelSkill();
    }


    virtual protected void OnMouseButton()
    {
        isClicked = !isClicked;
        if (isClicked)
        {
            int _index = GetIndexofSkillReady();
            bool _isUnique = false;

            if (_index >= 0)
            {
                if (skillDatas[_index].skillState == SkillData.SkillState.Unique)
                    _isUnique = true;
            }

            photonView.RPC("ClickMouse", RpcTarget.MasterClient, _index, _isUnique);
        }
    }
    virtual protected void OnMouseButton2()
    {

    }

    virtual protected void OnSit()
    {

    }

    virtual protected void OnInteraction()
    {

    }

    virtual protected void OnFly()
    {

    }

    virtual protected void SetSkillReady(int index)
    {
        if (!IsSkillProgressing())
        {
            for (int i = 0; i < skillDatas.Count; i++)
                skillDatas[i].isLocalReady = false;
            skillDatas[index].isLocalReady = true;
            Debug.Log("��ų " + (index + 1) + " : �غ�");
        }
    }

    virtual protected void CancelSkill()
    {
        for(int i = 0; i < skillDatas.Count; i++)
        {
            if (skillDatas[i].skillState <= SkillData.SkillState.Unique)
            {
                if (skillDatas[i].skillState == SkillData.SkillState.Unique)
                    PlayUniqueState(i, false);
                skillDatas[i].skillState = SkillData.SkillState.None;
                skillDatas[i].isLocalReady = false;
            }
        }
    }

    protected int GetIndexofSkillReady()
    {
        for (int i = 0; i < skillDatas.Count; i++)
            if (skillDatas[i].isLocalReady)
                return i;
        return -1;
    }

    protected bool IsSkillProgressing()
    {
        for (int i = 0; i < skillDatas.Count; i++)
            if (skillDatas[i].skillState > SkillData.SkillState.Unique)
                return true;
        return false;
    }



    [PunRPC]
    virtual public void ClickMouse(int index, bool isUnique)
    {
        if(index >= 0)
        {
            //������ Ŭ���̾�Ʈ���� �� �� �� Ȯ��
            if (skillDatas[index].isReady)
            {
                if (skillDatas[index].isUnique && !isUnique)
                    photonView.RPC("SetSkillPlayer", player, index, (int)SkillData.SkillState.Unique);
                else
                    photonView.RPC("SetSkillPlayer", RpcTarget.All, index, (int)SkillData.SkillState.Casting);
            }
        }
        else
        {
            //��Ÿ
        }
    }

    [PunRPC]
    virtual public void SetSkillPlayer(int index, int nextSkillState)
    {
        //��ų ��� Ÿ�̹�
        SkillData.SkillState _nextSkillState = (SkillData.SkillState)nextSkillState;
        if (photonView.IsMine)
        {
            if (isUniqueState)
            {
                isUniqueState = false;
                PlayUniqueState(index, isUniqueState);
            }

            skillDatas[index].skillState = _nextSkillState;
            if (_nextSkillState == SkillData.SkillState.Casting)
            {
                if (playerData.skillCastingTime[index] <= 0f)
                {
                    _nextSkillState = SkillData.SkillState.Channeling;
                    skillDatas[index].skillState = _nextSkillState;
                    skillDatas[index].skillChannelingTime = playerData.skillChannelingTime[index];
                }
                else
                    skillDatas[index].skillCastingTime = playerData.skillCastingTime[index];

                for (int i = 0; i < skillDatas.Count; i++)
                    skillDatas[i].isLocalReady = false;
                skillDatas[index].isReady = false;
                InvokeAnimation(index, true);
                //��ų Ÿ�̹� : ���
                PlaySkillLogic(index, SkillTiming.Immediately);
            }
            //Unique�϶�
            else
            {
                isUniqueState = true;
                PlayUniqueState(index, isUniqueState);
            }
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
                skillDatas[index].isReady = false;
            if (playerData.skillCastingTime[index] <= 0f)
            {
                _nextSkillState = SkillData.SkillState.Channeling;
                skillDatas[index].skillChannelingTime = playerData.skillChannelingTime[index];
            }
            else
                skillDatas[index].skillCastingTime = playerData.skillCastingTime[index];
            if (_nextSkillState == SkillData.SkillState.Casting
                || _nextSkillState == SkillData.SkillState.Channeling)
                InvokeAnimation(index, true);
        }
    }

    [PunRPC]
    public void AddPower(Vector3 power)
    {
        externalForce += power;
    }

    virtual protected void PlayUniqueState(int index, bool IsOn)
    {

    }

    //��ų ���� ������ ���⿡�� �������̵�� ����
    virtual protected void PlaySkillLogic(int index, SkillTiming timing)
    {

    }

    virtual protected void InvokeAnimation(int index, bool isPlay)
    {
        if (isPlay)
            PlayAnimation.Invoke(index);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        throw new NotImplementedException();
    }
}
