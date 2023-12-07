using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

namespace Player
{
    // 플레이어의 동작 상태
    public enum PlayerActionState
    {
        Move, Jump, Run, Sit, Interaction, ButtonCancel ,BasicAttack, Skill1, Skill2, Skill3, Skill4
    }

    // 플레이어 동작 데이터
    public class PlayerActionData
    {
        public PlayerActionState playerActionState;
        public InputAction inputAction;
        public bool isExecuting;
    }

    // 플레이어 캐릭터 종류 파악
    public enum PlayerCharactor
    {
        Aeterna,
        ElementalOrder,
        Dracoson,
        Cultist
    }

    public class Character : MonoBehaviourPunCallbacks, IPunObservable
    {
        PlayerInput playerInput;
        public List<PlayerActionData> playerActionDatas = new List<PlayerActionData>();
        public PlayerCharactor charactor;

        // 능력 넣는 Dictionary   
        [HideInInspector]
        public Dictionary<string, Ability> Skills;

        // 플레이어 하위 오브젝트
        public GameObject sight;
        public GameObject camera;
        public GameObject UI;

        public GameObject Alive;
        public GameObject Dead;
        public GameObject characterSoundManager;
        public BuffDebuffChecker buffDebuffChecker;

        //실시간 갱신 데이터
        public string playerName;
        public bool isOccupying = false;
        public bool isAlive = true;
        public int chargeCount = 0;
        public int ultimateCount = 0;

        public int hp;
        public float sitSpeed;
        public float sitSideSpeed;
        public float sitBackSpeed;

        public float moveSpeed;
        public float backSpeed;
        public float sideSpeed;
        public float runSpeedRatio;
        public float jumpHeight;

        public float headShotRatio;

        // 데이터 베이스에서 받는 데이터들
        [HideInInspector]
        public int dataHp;

        [HideInInspector]
        public float dataSitSpeed;
        [HideInInspector]
        public float dataSitSideSpeed;
        [HideInInspector]
        public float dataSitBackSpeed;

        [HideInInspector]
        public float dataMoveSpeed;
        [HideInInspector]
        public float dataBackSpeed;
        [HideInInspector]
        public float dataSideSpeed;
        [HideInInspector]
        public float dataRunSpeedRatio;

        [HideInInspector]
        public float dataJumpHeight;

        [HideInInspector]
        public float dataHeadShotRatio;

        // 컴포넌트
        [HideInInspector]
        public Animator animator;
        [HideInInspector]
        public Rigidbody rigidbody;
        [HideInInspector]
        public Ray lookRay;
        [HideInInspector]
        public RaycastHit lookHit;
        [HideInInspector]
        public SoundManager soundManager;
        [HideInInspector]
        public PhotonView soundManagerView;

        // 임시 사용 데이터
        public Vector3 moveVec;
        private bool isGrounded = false;
        private bool canInteraction = false;
        private Transform avatarForOther;
        private Transform avatarForMe;
        private RaycastHit slopeHit;
        private Vector3 cameraPos;
        private Quaternion cameraRot;

        private Rigidbody[] ragdollRigid;
        private Vector3[] ragdollPos;
        private Quaternion[] ragdollRot;

        private Vector3 networkSight;
        private Vector3 currentSight;

        // 체력 이나, 데미지, 죽음 같은 데이터는 마스터 클라인트만 처리하기. PhotonNetwork.isMasterClient
        protected virtual void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            SetPlayerKeys(PlayerActionState.Move, "Move");
            SetPlayerKeys(PlayerActionState.Jump, "Jump");
            SetPlayerKeys(PlayerActionState.Run, "Run");
            SetPlayerKeys(PlayerActionState.Sit, "Sit");
            SetPlayerKeys(PlayerActionState.Interaction, "Interaction");
            SetPlayerKeys(PlayerActionState.ButtonCancel, "ButtonCancel");
            SetPlayerKeys(PlayerActionState.BasicAttack, "BasicAttack");
            SetPlayerKeys(PlayerActionState.Skill1, "Skill1");
            SetPlayerKeys(PlayerActionState.Skill2, "Skill2");
            SetPlayerKeys(PlayerActionState.Skill3, "Skill3");
            SetPlayerKeys(PlayerActionState.Skill4, "Skill4");
            currentSight = sight.transform.position;
            soundManager = characterSoundManager.GetComponent<SoundManager>();
            soundManagerView = characterSoundManager.GetComponent<PhotonView>();
        }

        void SetPlayerKeys(PlayerActionState playerActionState, string action)
        {
            PlayerActionData _data = new PlayerActionData();
            _data.playerActionState = playerActionState;
            _data.inputAction = playerInput.actions.FindAction(action);
            _data.isExecuting = false;
            playerActionDatas.Add(_data);
        }

        protected virtual void Start()
        {
            Initialize();
        }

        void Initialize()
        {
            animator = GetComponent<Animator>();
            rigidbody = GetComponent<Rigidbody>();
            Skills = new Dictionary<string, Ability>();

            cameraPos = camera.transform.localPosition;
            cameraRot = camera.transform.localRotation;

            ragdollRigid = Dead.GetComponentsInChildren<Rigidbody>(true);
            ragdollPos = new Vector3[ragdollRigid.Length];
            ragdollRot = new Quaternion[ragdollRigid.Length];

            for (int i = 0; i < ragdollRigid.Length; i++)
            {
                ragdollPos[i] = ragdollRigid[i].transform.localPosition;
                ragdollRot[i] = ragdollRigid[i].transform.localRotation;
            }
        }


        protected virtual void Update()
        {

            if (photonView.IsMine)
            {
                if (buffDebuffChecker.CheckBuffDebuff("Horror") == true)
                {
                    PhotonView _photonView = PhotonNetwork.GetPhotonView(buffDebuffChecker.horrorViewID);
                    if (_photonView != null)
                    {
                        MouseControl _mouseControl = camera.GetComponent<MouseControl>();
                        Vector3 _directionVector = (_photonView.transform.position - camera.transform.position + new Vector3(0, 1f, 0)).normalized;
                        Quaternion _tempQ = Quaternion.LookRotation(_directionVector);
                        Vector3 _tempEuler = _tempQ.eulerAngles;
                        _mouseControl.ApplyPos(_tempEuler.y, _tempEuler.x);
                    }
                }

                lookRay = camera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

                if(Physics.Raycast(lookRay,out lookHit,1.5f))
                {
                    if (lookHit.collider.name == "AngelStatue" && CompareTag(lookHit.collider.tag))
                    {
                        canInteraction = true;
                        // 상태 UI 보이기
                    }

                    else
                    {
                        canInteraction = false;
                    }
                }

                else
                {
                    canInteraction = false;
                }


                animator.SetBool("Grounded", isGrounded);

                if(isGrounded == true)
                {
                    if (playerActionDatas[(int)PlayerActionState.Sit].isExecuting == true)//땅에 있고 앉아있으면
                    {
                        if (animator.GetLayerWeight(2) < 0)
                        {
                            animator.SetLayerWeight(2, 0);
                        }

                        animator.SetLayerWeight(2, animator.GetLayerWeight(2) + Time.deltaTime * 8);
                        animator.SetLayerWeight(1, animator.GetLayerWeight(1) - Time.deltaTime * 8);
                        animator.SetLayerWeight(3, animator.GetLayerWeight(3) - Time.deltaTime * 8);

                        if (animator.GetLayerWeight(2) > 1)
                        {
                            animator.SetLayerWeight(2, 1);
                        }

                    }
                    else//땅에 있고 서있을 때
                    {
                        if (animator.GetLayerWeight(1) < 0)
                        {
                            animator.SetLayerWeight(1, 0);
                        }

                        animator.SetLayerWeight(1, animator.GetLayerWeight(1) + Time.deltaTime * 8);
                        animator.SetLayerWeight(2, animator.GetLayerWeight(2) - Time.deltaTime * 8);
                        animator.SetLayerWeight(3, animator.GetLayerWeight(3) - Time.deltaTime * 8);

                        if (animator.GetLayerWeight(1) > 1)
                        {
                            animator.SetLayerWeight(1, 1);
                        }

                        
                    }

                }
                else//공중에 있을 때
                {
                    if(animator.GetLayerWeight(3) < 0)
                    {
                        animator.SetLayerWeight(3, 0);
                    }

                    animator.SetLayerWeight(3, animator.GetLayerWeight(3) + Time.deltaTime * 8);
                    animator.SetLayerWeight(2, animator.GetLayerWeight(2) - Time.deltaTime * 8);
                    animator.SetLayerWeight(1, animator.GetLayerWeight(1) - Time.deltaTime * 8);

                    if(animator.GetLayerWeight(3) > 1)
                    {
                        animator.SetLayerWeight(3, 1);
                    }

                   // MakeSound();
                }
            }
        }

        protected virtual void FixedUpdate()
        {
            if(photonView.IsMine)
            {
                PlayerMove();
                MakeMoveSound();
            }

            if(PhotonNetwork.IsMasterClient)
            {
                if (!((bool)PhotonNetwork.LocalPlayer.CustomProperties["IsAlive"]))
                {
                    isOccupying = false;
                    Debug.Log("죽어있음");
                }
            }
        }

        protected void MakeMoveSound()
        {
            if (playerActionDatas[(int)PlayerActionState.Jump].isExecuting)
            {
                soundManagerView.RPC("PlayAudio", RpcTarget.All, "JumpSound", 1.0f, false, "ActSounds");
            }

            else
            {
                // 하나만 소라가 나야 한다면 한 조건문 안에 한 사운드를 넣어라

                if (playerActionDatas[(int)PlayerActionState.Move].isExecuting)
                {
                    if (playerActionDatas[(int)PlayerActionState.Run].isExecuting)
                    {
                        soundManagerView.RPC("PlayAudio", RpcTarget.All, "RunSound", 1.0f, true, "ActSounds");
                    }

                    else
                    {
                        soundManagerView.RPC("PlayAudio", RpcTarget.All, "WalkSound", 1.0f, true, "ActSounds");
                    }
                }

                else
                {
                    soundManagerView.RPC("PlayAudio", RpcTarget.All, "NoSound", 1.0f, true, "ActSounds");
                }
            }
        }

        protected void PlayerMove()
        {
            if (animator == null || rigidbody == null) return;

            Vector3 _temp = new Vector3(0, 0, 0);

            if(IsOnSlope())
            {
                rigidbody.useGravity = false;
                if(isGrounded)
                    rigidbody.velocity = new Vector3(0,0,0);
            }

            else
            {
                rigidbody.useGravity = true;
            }

            if (playerActionDatas[(int)PlayerActionState.Move].isExecuting)
            {
                if (moveVec.z > 0)
                {
                    _temp += transform.forward;
                }
                else if (moveVec.z < 0)
                {
                    _temp -= transform.forward;
                }
                if (moveVec.x > 0)
                {
                    _temp += transform.right;
                }
                else if (moveVec.x < 0)
                {
                    _temp -= transform.right;
                }

                _temp.Normalize();
                SelectMoveSpeed(_temp);
                _temp = transform.InverseTransformVector(_temp);
            }

            animator.SetInteger("VerticalSpeed", (int)moveVec.z);
            animator.SetInteger("HorizontalSpeed", (int)moveVec.x);

        }

        bool IsOnSlope()
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            if(Physics.Raycast(ray, out slopeHit,0.2f))
            {
                var angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                return angle != 0.0f;
            }

            return false;
        }

        void SelectMoveSpeed(Vector3 _temp)
        {
            if (playerActionDatas[(int)PlayerActionState.Sit].isExecuting)
            {
                if (Mathf.Abs(moveVec.x) > 0.01f)
                {
                    rigidbody.MovePosition(rigidbody.transform.position + _temp * sitSideSpeed * Time.deltaTime);
                }

                if (moveVec.z > 0.01f)
                {
                    rigidbody.MovePosition(rigidbody.transform.position + _temp * sitSpeed * Time.deltaTime);
                }

                if (moveVec.z < -0.01f)
                {
                    rigidbody.MovePosition(rigidbody.transform.position + _temp * sitBackSpeed * Time.deltaTime);
                }

            }

            else if (playerActionDatas[(int)PlayerActionState.Run].isExecuting)
            {
                if (Mathf.Abs(moveVec.x) > 0.01f)
                {
                    rigidbody.MovePosition(rigidbody.transform.position + _temp * sideSpeed * runSpeedRatio * Time.deltaTime);
                }

                if (moveVec.z > 0.01f)
                {
                    rigidbody.MovePosition(rigidbody.transform.position + _temp * moveSpeed * runSpeedRatio * Time.deltaTime);
                }

                if (moveVec.z < -0.01f)
                {
                    rigidbody.MovePosition(rigidbody.transform.position + _temp * backSpeed * runSpeedRatio * Time.deltaTime);
                }
            }

            else
            {
                if (Mathf.Abs(moveVec.x) > 0.01f)
                {
                    rigidbody.MovePosition(rigidbody.transform.position + _temp * sideSpeed * Time.deltaTime);
                }

                if (moveVec.z > 0.01f)
                {
                    rigidbody.MovePosition(rigidbody.transform.position + _temp * moveSpeed * Time.deltaTime);
                }

                if (moveVec.z < -0.01f)
                {
                    rigidbody.MovePosition(rigidbody.transform.position + _temp * backSpeed * Time.deltaTime);
                }

            }

            
        }

        void OnMove(InputValue value)
        {
            if (photonView.IsMine && (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsAlive"])
            {
                moveVec = new Vector3(value.Get<Vector2>().x, 0, value.Get<Vector2>().y);

                if (moveVec.magnitude <= 0)
                {
                    playerActionDatas[(int)PlayerActionState.Move].isExecuting = false;
                }
                else
                {
                    playerActionDatas[(int)PlayerActionState.Move].isExecuting = true;
                }
            }
        }
        void OnJump()
        {
            if (playerActionDatas[(int)PlayerActionState.Jump].isExecuting) return;

            if (photonView.IsMine && (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsAlive"])
            {
                if (isGrounded)
                {
                    rigidbody.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
                    playerActionDatas[(int)PlayerActionState.Jump].isExecuting = true;
                }
            }
        }
        void OnRun()
        {
            if (photonView.IsMine && (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsAlive"])
            {

                if (!playerActionDatas[(int)PlayerActionState.Run].isExecuting)
                {
                    animator.SetBool("Run", true);
                    playerActionDatas[(int)PlayerActionState.Run].isExecuting = true;
                }

                else
                {
                    animator.SetBool("Run", false);
                    playerActionDatas[(int)PlayerActionState.Run].isExecuting = false;
                }
            }
        }

        void OnSit()
        {
            if(photonView.IsMine && (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsAlive"])
            {
                photonView.RPC("Sitting", RpcTarget.AllBuffered);

                if (!playerActionDatas[(int)PlayerActionState.Sit].isExecuting)
                {
                    playerActionDatas[(int)PlayerActionState.Sit].isExecuting = true;
                }
                else
                {
                    playerActionDatas[(int)PlayerActionState.Sit].isExecuting = false;
                }

            }
        }

        [PunRPC]
        public void Sitting()
        {
            if (!playerActionDatas[(int)PlayerActionState.Sit].isExecuting)
            {
                transform.GetChild(0).localPosition = new Vector3(0, -0.25f, 0);
                transform.GetChild(1).localPosition += new Vector3(0, -0.25f, 0);
            }
            else
            {
                transform.GetChild(0).localPosition = new Vector3(0, 0, 0);
                transform.GetChild(1).localPosition -= new Vector3(0, -0.25f, 0);

            }
        }

        void OnInteraction()
        {
            if (photonView.IsMine && (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsAlive"])
            {
                // 자기 팀 거점인 것 확인 / 거점 점령시 석상 태그 팀으로 설정
                // 캐릭터 각자 관리하면 프레임 차이로 인한 시간 차 발생
                if(canInteraction)
                {
                    float temp = (float)PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["AngelStatueCoolTime"];
                    PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["ParameterName"] = "AngelStatueCoolTime";
                    GameCenterTest.ChangePlayerCustomProperties
                            (PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr], "AngelStatueCoolTime", temp);
                    Debug.Log("Interaction!!");
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            /*if (photonView.IsMine)
            {
                if (collision.gameObject.layer == LayerMask.NameToLayer("Map"))
                {
                    soundManagerView.RPC("PlayAudio", RpcTarget.All, "LandSound", 1.0f, false);
                }
            }*/
        }

        private void OnCollisionStay(Collision collision)
        {
            if(photonView.IsMine)
            {
                if (collision.gameObject.layer == LayerMask.NameToLayer("Map"))
                {
                    isGrounded = true;
                    playerActionDatas[(int)PlayerActionState.Jump].isExecuting = false;
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if(photonView.IsMine)
            {
                if (collision.gameObject.layer == LayerMask.NameToLayer("Map"))
                {
                    isGrounded = false;
                    playerActionDatas[(int)PlayerActionState.Jump].isExecuting = true;
                }
            }
        }

        [PunRPC]
        public void ChangeName(string name)
        {
            gameObject.name  = name;
            playerName = name;
        }

        [PunRPC]
        public virtual void IsLocalPlayer()
        {
            if (photonView.IsMine)
            {
                GetComponent<PlayerInput>().enabled = true;
                camera.SetActive(true);
                avatarForOther = transform.GetChild(0).GetChild(0).GetChild(0);//다른 사람들이 보는 자신의 아바타
                avatarForMe = transform.GetChild(0).GetChild(1).GetChild(0);//자신이 보는 자신의 아바타
                for (int i = 0; i < avatarForOther.childCount; i++)
                {
                    avatarForOther.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Me");
                    avatarForOther.GetChild(i).gameObject.GetComponent<SkinnedMeshRenderer>().enabled = false;
                }
                //avatarForMe.gameObject.SetActive(true);

                for (int i = 0; i < avatarForMe.childCount; i++)
                {
                    avatarForMe.GetChild(i).gameObject.layer = LayerMask.NameToLayer("OverlayCameraForMe");
                }
                UI.SetActive(true);
            }
        }

        [PunRPC]
        public void SetEnemyLayer()
        {
            Character[] characters = FindObjectsOfType<Character>();
            if (characters == null) return;

            foreach (Character character in characters)
            {
                if (!character.gameObject.CompareTag(tag))
                {
                    OutlineDrawer[] outlineDrawers = character.gameObject.GetComponentsInChildren<OutlineDrawer>();
                    if (outlineDrawers == null) return;

                    foreach (OutlineDrawer outline in outlineDrawers)
                    {
                        outline.enabled = true;
                    }
                }
            }
        }

        public void SetTagServer(string team)
        {
            photonView.RPC("SetTag", RpcTarget.All, team);
        }

        [PunRPC]
        protected virtual void SetTag(string team)
        {
            this.tag = team;
            
            Transform[] allChildren = GetComponentsInChildren<Transform>();
            if (allChildren == null) return;

            foreach (Transform child in allChildren)
            {
                child.gameObject.tag = team;
            }       
        }

        
        [PunRPC]
        public void PlayerTeleport(Vector3 pos)
        {
            transform.position = pos;
        }

        [PunRPC]
        public void PlayerDamaged(string enemy ,int damage, string damgePart, Vector3 direction, float force)
        {
            if (isAlive == false) return;

            if (damage > 0)
            {
                if (hp <= dataHp)
                {
                    hp -= damage;
                    //UI.GetComponent<ScreenEffectManager>().PlayDamageEffect(damage);
                    Debug.Log("Player Damaged !! : " + damage + " EnemyName: " + enemy);
                }

                // 마스터 클라이언트이기 때문에 동기화 안되도 게임센터의 값과 같다. 
                if (PhotonNetwork.IsMasterClient)
                {
                    var killer = GameCenterTest.FindPlayerWithCustomProperty("Name", enemy);

                    // 사망시
                    if (hp <= 0)
                    {
                        isAlive = false;
                        int temp1 = (int)PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DeadCount"];
                        PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["ParameterName"] = "DeadCount";

                        PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DamagePart"] = damgePart;
                        PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DamageDirection"] = direction;
                        PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DamageForce"] = force;

                        if (killer == null)
                        {
                            if (!(bool)PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["IsAlive"]) return;

                            PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["KillerName"] = enemy;

                            GameCenterTest.ChangePlayerCustomProperties
                                (PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr], "DeadCount", temp1 + 1);
                            return;
                        }

                        SetUltimatePoint(killer.ActorNumber);
                        PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["KillerName"] = killer.CustomProperties["Name"];

                        Debug.Log("Player Character Dead");
                        GameCenterTest.ChangePlayerCustomProperties
                            (PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr], "DeadCount", temp1 + 1);
                        Debug.Log("DeadCount");

                        int temp2 = (int)killer.CustomProperties["KillCount"];
                        killer.CustomProperties["ParameterName"] = "KillCount";

                        GameCenterTest.ChangePlayerCustomProperties(killer, "KillCount", temp2 + 1);

                    }

                    int temp = (int)killer.CustomProperties["TotalDamage"];
                    killer.CustomProperties["ParameterName"] = "TotalDamage";
                    killer.CustomProperties["PlayerAssistViewID"] = photonView.ViewID.ToString();
                    GameCenterTest.ChangePlayerCustomProperties(killer, "TotalDamage", temp + damage);
                }
            }

            else
            {
                if (hp < dataHp)
                {
                    hp -= damage;
                    if (hp > dataHp) hp = dataHp;

                    var healer = GameCenterTest.FindPlayerWithCustomProperty("CharacterViewID", enemy);
                    int temp = (int)healer.CustomProperties["TotalHeal"];
                    healer.CustomProperties["ParameterName"] = "TotalHeal";
                    healer.CustomProperties["PlayerAssistViewID"] = photonView.ViewID.ToString();
                    GameCenterTest.ChangePlayerCustomProperties(healer, "TotalHeal", temp + (-damage));
                }

                
            }

            
        }

        [PunRPC]
        public virtual void PlayerDeadForAll(string damgePart, Vector3 direction, float force)
        {
            // Ragdoll로 처리
            Dead.SetActive(true);
            animator.enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;
            Alive.SetActive(false);
            moveVec = Vector3.zero;

            Rigidbody[] bodyParts = Dead.GetComponentsInChildren<Rigidbody>();
            
            foreach(Rigidbody rb in bodyParts)
            {
                if(rb.gameObject.name == damgePart)
                {
                    rb.AddForce(direction.normalized * force, ForceMode.Impulse);
                    return;
                }
            }

        }


        [PunRPC]
        public void PlayerDeadPersonal()
        {
            camera.transform.SetParent(Dead.transform);

            camera.GetComponent<MouseControl>().enabled = false;
            camera.GetComponent<DeadCamera>().enabled = true;

        }

        [PunRPC]
        public virtual void PlayerReBornForAll(Vector3 pos)
        {
            gameObject.transform.position = pos;
            gameObject.transform.rotation = Quaternion.identity;
            moveVec = Vector3.zero;
            isAlive = true;

            for (int i = 0; i < ragdollRigid.Length; i++)
            {
                ragdollRigid[i].transform.localPosition = ragdollPos[i];
                ragdollRigid[i].transform.localRotation = ragdollRot[i];
                ragdollRigid[i].velocity = new Vector3(0, 0, 0);
            }
            
            Alive.SetActive(true);
            animator.enabled = true;
            GetComponent<CapsuleCollider>().enabled = true;
            Dead.SetActive(false);

            hp = dataHp;
        }

        [PunRPC]
        public void PlayerReBornPersonal()
        {
            camera.transform.SetParent(Alive.transform);

            camera.transform.localPosition = cameraPos;
            camera.transform.localRotation = cameraRot;

            camera.GetComponent<MouseControl>().enabled = true;
            camera.GetComponent<DeadCamera>().enabled = false;
        }

        [PunRPC]
        public void SetChargePoint(int actorNumber)
        {
            chargeCount++;
            if(chargeCount>=4)
            {
                chargeCount = 0;
                SetUltimatePoint(actorNumber);
            }
        }

        public void SetUltimatePoint(int actorNumber)
        {
            int temp = (int)PhotonNetwork.CurrentRoom.Players[actorNumber].CustomProperties["UltimateCount"];

            if (temp < 10)
            {             
                GameCenterTest.ChangePlayerCustomProperties(PhotonNetwork.CurrentRoom.Players[actorNumber], "UltimateCount", temp + 1);
                PhotonView view = PhotonView.Find((int)PhotonNetwork.CurrentRoom.Players[actorNumber].CustomProperties["CharacterViewID"]);
                if (view == null) return;
                view.RPC("AddUltimatePoint", PhotonNetwork.CurrentRoom.Players[actorNumber], temp + 1);
            }
        }

        [PunRPC]
        public void AddUltimatePoint(int num)
        {
            ultimateCount = num;
        }

        [PunRPC]
        public void AngelStatueHP(int addHp)
        {
            if(hp < dataHp)
            {
                hp += addHp;
            }
        }

        [PunRPC]
        public void DisActiveMe()
        {
            gameObject.SetActive(false);
        }

        protected virtual void OnAnimatorIK()
        {
            SetLookAtObj();
        }

        void SetLookAtObj()
        {
            if (animator == null) return;
            if (photonView.IsMine)
            {
                animator.SetLookAtWeight(1f, 0.9f);
                animator.SetLookAtPosition(sight.transform.position);
            }
            else
            {
                Vector3 newVec = Vector3.Lerp(currentSight, networkSight, Time.deltaTime * 10);
                animator.SetLookAtWeight(1f, 0.9f);
                animator.SetLookAtPosition(newVec);
                currentSight = newVec;
            }
        }
        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // 데이터를 보내는 부분
                stream.SendNext(hp);
                stream.SendNext(isOccupying);
                //stream.SendNext(chargeCount);
                //stream.SendNext(ultimateCount);
                for (int i = 0; i < playerActionDatas.Count; i++)
                {
                    stream.SendNext(playerActionDatas[i].isExecuting);
                }
                stream.SendNext(moveVec);
                stream.SendNext(isGrounded);
                stream.SendNext(sight.transform.position);
            }
            else
            {
                // 데이터를 받는 부분
                hp = (int)stream.ReceiveNext();
                isOccupying = (bool)stream.ReceiveNext();
                //chargeCount = (int)stream.ReceiveNext();
                //ultimateCount = (int)stream.ReceiveNext();
                for (int i = 0; i < playerActionDatas.Count; i++)
                {
                    playerActionDatas[i].isExecuting = (bool)stream.ReceiveNext();
                }
                moveVec = (Vector3)stream.ReceiveNext();
                isGrounded = (bool)stream.ReceiveNext();
                networkSight = (Vector3)stream.ReceiveNext();
            }
        }
    }
}