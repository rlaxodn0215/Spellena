using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using UnityEngine.AI;
using GameCenterTest0;

namespace Player
{
    // 플레이어의 동작 상태
    public enum PlayerActionState
    {
        Move, Jump, Run, Sit, Interaction, ButtonCancel, BasicAttack, Skill1, Skill2, Skill3, Skill4
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
        public SoundManager soundManager;
        [HideInInspector]
        public PhotonView soundManagerView;

        // 임시 사용 데이터
        public Vector3 moveVec;
        public bool canInteraction = false;

        protected bool isGrounded = false;
        protected Transform avatarForOther;
        protected Transform avatarForMe;
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

                animator.SetBool("Grounded", isGrounded);

                if (isGrounded == true)
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
                    if (animator.GetLayerWeight(3) < 0)
                    {
                        animator.SetLayerWeight(3, 0);
                    }

                    animator.SetLayerWeight(3, animator.GetLayerWeight(3) + Time.deltaTime * 8);
                    animator.SetLayerWeight(2, animator.GetLayerWeight(2) - Time.deltaTime * 8);
                    animator.SetLayerWeight(1, animator.GetLayerWeight(1) - Time.deltaTime * 8);

                    if (animator.GetLayerWeight(3) > 1)
                    {
                        animator.SetLayerWeight(3, 1);
                    }
                }

                CheckPlayerLowHP(0.15f);
            }


        }

        protected virtual void FixedUpdate()
        {
            if (photonView.IsMine)
            {
                PlayerMove();
            }

            if (PhotonNetwork.IsMasterClient)
            {
                isOccupying = false;
                //Debug.Log(hp);
            }
        }

        protected void PlayerMove()
        {
            if (animator == null || rigidbody == null) return;

            Vector3 _temp = new Vector3(0, 0, 0);

            if (IsOnSlope())
            {
                rigidbody.useGravity = false;
                if (isGrounded)
                    rigidbody.velocity = new Vector3(0, 0, 0);
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
            if (Physics.Raycast(ray, out slopeHit, 0.2f))
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
                    soundManagerView.RPC("PlayAudio", RpcTarget.All, "NoSound", 1.0f, true, "ActSounds", "EffectSound");
                    playerActionDatas[(int)PlayerActionState.Move].isExecuting = false;
                }
                else
                {
                    soundManagerView.RPC("PlayAudio", RpcTarget.All, "WalkSound", 1.0f, true, "ActSounds", "EffectSound");
                    playerActionDatas[(int)PlayerActionState.Move].isExecuting = true;
                }
            }
        }
        protected virtual void OnJump()
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
                    soundManagerView.RPC("PlayAudio", RpcTarget.All, "RunSound", 1.0f, true, "ActSounds", "EffectSound");
                    playerActionDatas[(int)PlayerActionState.Run].isExecuting = true;
                }

                else
                {
                    if (playerActionDatas[(int)PlayerActionState.Jump].isExecuting)
                    {
                        soundManagerView.RPC("PlayAudio", RpcTarget.All, "NoSound", 1.0f, true, "ActSounds", "EffectSound");
                    }

                    else
                    {
                        if (playerActionDatas[(int)PlayerActionState.Move].isExecuting)
                        {
                            soundManagerView.RPC("PlayAudio", RpcTarget.All, "WalkSound", 1.0f, true, "ActSounds", "EffectSound");
                        }

                        else
                        {
                            soundManagerView.RPC("PlayAudio", RpcTarget.All, "NoSound", 1.0f, true, "ActSounds", "EffectSound");
                        }
                    }

                    animator.SetBool("Run", false);
                    playerActionDatas[(int)PlayerActionState.Run].isExecuting = false;
                }
            }
        }

        void OnSit()
        {
            if (photonView.IsMine && (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsAlive"])
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
                if (canInteraction)
                {
                    float temp = (float)PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["AngelStatueCoolTime"];
                    PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["ParameterName"] = "AngelStatueCoolTime";
                    GameCenterTest.ChangePlayerCustomProperties
                            (PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr], "AngelStatueCoolTime", temp);
                    Debug.Log("Interaction!!");
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (photonView.IsMine)
            {
                if (collision.gameObject.layer == LayerMask.NameToLayer("Map"))
                {
                    playerActionDatas[(int)PlayerActionState.Jump].isExecuting = false;

                    if (!isGrounded)
                    {
                        if (playerActionDatas[(int)PlayerActionState.Move].isExecuting)
                        {
                            if (playerActionDatas[(int)PlayerActionState.Run].isExecuting)
                            {
                                soundManagerView.RPC("PlayAudio", RpcTarget.All, "RunSound", 1.0f, true, "ActSounds", "EffectSound");
                            }

                            else
                            {
                                soundManagerView.RPC("PlayAudio", RpcTarget.All, "WalkSound", 1.0f, true, "ActSounds", "EffectSound");
                            }

                        }

                        else
                        {
                            soundManagerView.RPC("PlayAudio", RpcTarget.All, "NoSound", 1.0f, true, "ActSounds", "EffectSound");
                        }
                    }

                    isGrounded = true;
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (photonView.IsMine)
            {
                if (collision.gameObject.layer == LayerMask.NameToLayer("Map"))
                {
                    playerActionDatas[(int)PlayerActionState.Jump].isExecuting = true;
                    if (isGrounded)
                    {
                        soundManagerView.RPC("PlayAudio", RpcTarget.All, "JumpSound", 1.0f, false, "ActSounds", "EffectSound");
                    }
                    isGrounded = false;
                }
            }
        }

        [PunRPC]
        public void ChangeName(string name)
        {
            gameObject.name = name;
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

                //Debug.LogError("IsLocalPlayer!!");
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

                    GameObject miniMapSign = Helper.FindObject(character.gameObject, "Cylinder");
                    if (miniMapSign == null) continue;
                    miniMapSign.SetActive(false);
                }

                if(character.gameObject.GetComponent<Aeterna>())
                {
                    if (character.gameObject.GetComponent<PhotonView>().IsMine)
                    {
                        character.gameObject.GetComponent<Aeterna>().DimensionSword.SetActive(false);
                    }

                    else
                    {
                        character.gameObject.GetComponent<Aeterna>().DimensionSwordForMe.SetActive(false);
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
        public void MovePlayerWithDuration(Vector3 direction, float distance, float speed)
        {
            float elapsedTime = 0f;

            while (elapsedTime < distance / speed)
            {
                transform.position += direction * speed * Time.deltaTime;
                elapsedTime += Time.deltaTime;
            }
        }

        [PunRPC]
        public void PlayerKnockBack(Vector3 direction, float knockbackForce)
        {
            transform.GetComponent<Rigidbody>().AddForce(direction * knockbackForce, ForceMode.Impulse);
        }

        [PunRPC]
        public void PlayerDamaged(string enemy, int damage, string damagePart, Vector3 direction, float force)
        {
            if (isAlive == false) return;

            if (damage > 0)
            {
                if (hp <= dataHp)
                {
                    if(damagePart == "head")
                    {
                        hp -= (int)(damage * headShotRatio);
                        //Debug.Log("Player HEADSHOT Damaged !!");
                    }

                    else
                    {
                        hp -= damage;
                        Debug.Log("Player Damaged !! : " + damage + " EnemyName: " + enemy);
                    }

                    if(UI !=null)
                        UI.GetComponent<ScreenEffectManager>().PlayDamageEffect(damage);
                }

                 var killer = GameCenterTest.FindPlayerWithCustomProperty("Name", enemy);

                 // 사망시
                 if (hp <= 0)
                 {
                    if(UI == null)
                    {
                        StartCoroutine(AILifeDead());
                        return;
                    }


                    isAlive = false;
                    if (killer == null)
                    {
                        int temp = (int)PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DeadCount"];
                        PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["ParameterName"] = "DeadCount";

                        PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DamagePart"] = damagePart;
                        PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DamageDirection"] = direction;
                        PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DamageForce"] = force;

                        if (!(bool)PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["IsAlive"]) return;

                        PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["KillerName"] = enemy;

                        if (PhotonNetwork.IsMasterClient)
                            GameCenterTest.ChangePlayerCustomProperties(PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr], "DeadCount", temp + 1);

                        return;
                    }

                     StartCoroutine(PlayerDead(0.2f, killer,damagePart,direction,force));
                 }

                 if (killer == null) return;
                 int temp0 = (int)killer.CustomProperties["TotalDamage"];
                 killer.CustomProperties["ParameterName"] = "TotalDamage";
                  
                 killer.CustomProperties["DamagePart"] = damagePart;
                 killer.CustomProperties["DamageDirection"] = direction;
                 killer.CustomProperties["DamageForce"] = force;

                 killer.CustomProperties["PlayerAssistViewID"] = photonView.ViewID.ToString();

                //Debug.Log("<color=purple>" + "DamagePart : " + damagePart +"</color>");

                 if (PhotonNetwork.IsMasterClient)
                        GameCenterTest.ChangePlayerCustomProperties(killer, "TotalDamage", temp0 + damage);              
            }

            else
            {
                if (hp < dataHp)
                {
                    hp -= damage;
                    if (hp > dataHp) hp = dataHp;

                    var healer = GameCenterTest.FindPlayerWithCustomProperty("CharacterViewID", enemy);
                    if (healer == null) return;
                    int temp = (int)healer.CustomProperties["TotalHeal"];
                    healer.CustomProperties["ParameterName"] = "TotalHeal";
                    healer.CustomProperties["PlayerAssistViewID"] = photonView.ViewID.ToString();
                    if (PhotonNetwork.IsMasterClient)
                        GameCenterTest.ChangePlayerCustomProperties(healer, "TotalHeal", temp + (-damage));
                }


            }

        }

        IEnumerator<WaitForSeconds> AILifeDead()
        {
            Dead.transform.position = transform.position;
            Dead.SetActive(true);
            gameObject.transform.position = new Vector3(0, -1000, 0);
            gameObject.GetComponent<NavMeshAgent>().enabled = false;
            if (ragdollRigid == null)
            {
                ragdollRigid = Dead.GetComponentsInChildren<Rigidbody>(true);
                ragdollPos = new Vector3[ragdollRigid.Length];
                ragdollRot = new Quaternion[ragdollRigid.Length];

                for (int i = 0; i < ragdollRigid.Length; i++)
                {
                    ragdollPos[i] = ragdollRigid[i].transform.localPosition;
                    ragdollRot[i] = ragdollRigid[i].transform.localRotation;
                }
            }
            InGameUI ui = GameObject.Find("GlobalUI").GetComponentInChildren<InGameUI>();
            ui.ShowKillUI("Aloy");
            ui.ShowKillLog("TestPlayer1", "Aloy", false, PhotonNetwork.LocalPlayer.ActorNumber);
            int temp2 = (int)PhotonNetwork.LocalPlayer.CustomProperties["KillCount"];
            if (PhotonNetwork.IsMasterClient)
                GameCenterTest.ChangePlayerCustomProperties(PhotonNetwork.LocalPlayer, "KillCount", temp2 + 1);
            yield return new WaitForSeconds(10.0f);
            Dead.SetActive(false);
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            gameObject.transform.position = new Vector3(82.0f, 17.5f, 0.62f);
            for (int i = 0; i < ragdollRigid.Length; i++)
            {
                ragdollRigid[i].transform.localPosition = ragdollPos[i];
                ragdollRigid[i].transform.localRotation = ragdollRot[i];
                ragdollRigid[i].velocity = new Vector3(0, 0, 0);
            }
            hp = dataHp;
            yield return new WaitForSeconds(0.5f);
            gameObject.GetComponent<NavMeshAgent>().enabled = true;
        }

        IEnumerator<WaitForSeconds> PlayerDead(float time, Photon.Realtime.Player killer, string damgePart, Vector3 direction, float force)
        {
            SetUltimatePoint(killer.ActorNumber);

            yield return new WaitForSeconds(0.2f);

            int temp1 = (int)PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DeadCount"];
            PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["ParameterName"] = "DeadCount";

            PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DamagePart"] = damgePart;
            PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DamageDirection"] = direction;
            PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DamageForce"] = force;

            if (PhotonNetwork.IsMasterClient)
                GameCenterTest.ChangePlayerCustomProperties(PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr], "DeadCount", temp1 + 1);

            PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["KillerName"] = killer.CustomProperties["Name"];

            int temp2 = (int)killer.CustomProperties["KillCount"];
            killer.CustomProperties["ParameterName"] = "KillCount";

            if (PhotonNetwork.IsMasterClient)
                GameCenterTest.ChangePlayerCustomProperties(killer, "KillCount", temp2 + 1);
        }

        protected void CheckPlayerLowHP(float ratio)
        {
            if (hp <= dataHp * ratio)
            {
                soundManager.PlayAudio("Heartbeat", 1.0f, true, false, "EffectSound");
            }

            else
            {
                soundManager.StopAudio("Heartbeat");
            }
        }

        [PunRPC]
        public virtual void PlayerDeadForAll(string damgePart, Vector3 direction, float force)
        {
            // Ragdoll로 처리
            Dead.SetActive(true);
            animator.enabled = false;
            //GetComponent<CapsuleCollider>().enabled = false;
            Alive.SetActive(false);
            moveVec = Vector3.zero;

            Rigidbody[] bodyParts = Dead.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in bodyParts)
            {
                if (rb.gameObject.name == damgePart)
                {
                    rb.AddForce(direction.normalized * force, ForceMode.Impulse);
                    Debug.LogWarning("DamagePart : "+ damgePart +"// Force Direction : " + direction);
                    return;
                }
            }

            characterSoundManager.GetComponent<SoundManager>().PlayAudio("Death", 1.0f, false, "SpeakSound", "VoiceSound");

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
            //GetComponent<CapsuleCollider>().enabled = true;
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
            Debug.Log("SetChargePoint to " + "<color=yellow>" + (string)PhotonNetwork.LocalPlayer.CustomProperties["Name"]);
            chargeCount++;
            if (chargeCount >= 4)
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
            if (hp < dataHp)
            {
                hp += (dataHp * addHp) / 100;
                if(hp > dataHp) hp = dataHp;
            }
        }

        [PunRPC]
        public void DisActiveMe()
        {
            gameObject.SetActive(false);
        }

        float weight = 1.0f;
        float bodyWeight = 0.9f;
        float lerpSpeed = 20;


        protected virtual void OnAnimatorIK()
        {
            SetLookAtObj();
        }

        void SetLookAtObj()
        {
            if (animator == null) return;
            if (photonView.IsMine)
            {
                animator.SetLookAtWeight(weight, bodyWeight);
                animator.SetLookAtPosition(sight.transform.position);
            }
            else
            {
                Vector3 newVec = Vector3.Lerp(currentSight, networkSight, 
                    Time.deltaTime * lerpSpeed);
                animator.SetLookAtWeight(weight, lerpSpeed);
                animator.SetLookAtPosition(newVec);
                currentSight = newVec;
            }
        }
        
        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // 데이터를 보내는 부분
                stream.SendNext(sight.transform.position);
            }
            else
            {
                // 데이터를 받는 부분
                networkSight = (Vector3)stream.ReceiveNext();
            }
        }
        
    }
}