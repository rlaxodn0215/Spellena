using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

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
        ElementalOrder
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
        public GameObject enemyCam;
        public GameObject UI;

        public GameObject Alive;
        public GameObject Dead;

        //실시간 갱신 데이터
        public string playerName;
        public bool isOccupying = false;
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
            //임시적으로 만듬
            playerName = GetComponent<PhotonView>().ViewID.ToString();

            gameObject.name = "Player_" + playerName;
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
            if (PhotonNetwork.IsMasterClient)
            {
                isOccupying = false;
            }

            if (photonView.IsMine)
            {
                lookRay = camera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

                if(Physics.Raycast(lookRay,out lookHit,2.0f))
                {
                    if(lookHit.collider.name ==" AngelStatue")
                    {
                        canInteraction = true;
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
                }
            }
        }

        protected virtual void FixedUpdate()
        {
            if(photonView.IsMine)
            {
                PlayerMove();
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
                    playerActionDatas[(int)PlayerActionState.Move].isExecuting = false;
                else
                    playerActionDatas[(int)PlayerActionState.Move].isExecuting = true;
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
                if(canInteraction)
                {
                    Debug.Log("Interaction!!");
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if(photonView.IsMine)
            {
                if (collision.gameObject.tag == "Ground")
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
                if (collision.gameObject.tag == "Ground")
                {
                    isGrounded = false;
                    playerActionDatas[(int)PlayerActionState.Jump].isExecuting = true;
                }
            }
        }
        void OnTriggerStay(Collider other)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (other.tag == "OccupationArea")
                {
                    isOccupying = true;
                }
            }
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
            photonView.RPC("SetTag", RpcTarget.AllBufferedViaServer, team);
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
            if (damage > 0)
            {
                if (hp <= dataHp)
                {
                    hp -= damage;
                    //Debug.Log("Player Damaged !!  EnemyName: " + enemy);
                }

                // 마스터 클라이언트이기 때문에 동기화 안되도 게임센터의 값과 같다. 
                if (PhotonNetwork.IsMasterClient)
                {
                    var killer = GameCenterTest.FindPlayerWithCustomProperty("CharacterViewID", enemy);

                    if (killer == null)
                    {
                        Debug.Log("자해");
                        return;
                    }

                    int temp = (int)killer.CustomProperties["TotalDamage"];
                    killer.CustomProperties["ParameterName"] = "TotalDamage";
                    killer.CustomProperties["PlayerAssistViewID"] = photonView.ViewID.ToString();
                    GameCenterTest.ChangePlayerCustomProperties(killer, "TotalDamage", temp + damage);

                    // 사망시
                    if (hp <= 0)
                    {
                        int temp1 = (int)PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DeadCount"];
                        PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["ParameterName"] = "DeadCount";

                        PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DamagePart"] = damgePart;
                        PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DamageDirection"] = direction;
                        PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr].CustomProperties["DamageForce"] = force;

                        GameCenterTest.ChangePlayerCustomProperties
                            (PhotonNetwork.CurrentRoom.Players[photonView.OwnerActorNr], "DeadCount", temp1 + 1);

                        Photon.Realtime.Player killer1 = GameCenterTest.FindPlayerWithCustomProperty("CharacterViewID", enemy);

                        if (killer1 != null)
                        {
                            int temp2 = (int)killer1.CustomProperties["KillCount"];
                            killer1.CustomProperties["ParameterName"] = "KillCount";
                            GameCenterTest.ChangePlayerCustomProperties(killer1, "KillCount", temp2 + 1);
                        }

                        else
                        {
                            Debug.Log("자살");
                        }
                    }
                }
            }

            else
            {
                if (hp < dataHp)
                {
                    hp -= damage;
                    var healer = GameCenterTest.FindPlayerWithCustomProperty("CharacterViewID", enemy);
                    int temp = (int)healer.CustomProperties["TotalHeal"];
                    healer.CustomProperties["ParameterName"] = "TotalHeal";
                    healer.CustomProperties["PlayerAssistViewID"] = photonView.ViewID.ToString();
                    GameCenterTest.ChangePlayerCustomProperties(healer, "TotalHeal", temp + (-damage));
                }

                //Debug.Log("Player Healing !!");
            }

            
        }

        [PunRPC]
        public void PlayerDeadForAll(string damgePart, Vector3 direction, float force)
        {
            // Ragdoll로 처리
            hp = dataHp;
            Dead.SetActive(true);
            Alive.SetActive(false);
            GetComponent<CapsuleCollider>().enabled = false;

            Rigidbody[] bodyParts = Dead.GetComponentsInChildren<Rigidbody>();
            
            foreach(Rigidbody rb in bodyParts)
            {
                if(rb.gameObject.name == damgePart)
                {
                    //Debug.Log(damgePart + " / " + direction + " / " + force);
                    rb.AddForce(direction.normalized * force, ForceMode.Impulse);
                    return;
                }
            }

            //Debug.Log("No DamagePart");
        }

        [PunRPC]
        public void PlayerDeadPersonal()
        {
            camera.transform.SetParent(Dead.transform);

            //camera.transform.Translate(new Vector3(0f, 1.5f, -1f));
            //camera.transform.Rotate(new Vector3(32, 0, 0));

            camera.GetComponent<MouseControl>().enabled = false;
            camera.GetComponent<DeadCamera>().enabled = true;

        }

        [PunRPC]
        public void PlayerReBornForAll(Vector3 pos)
        {
            gameObject.transform.position = pos;
            gameObject.transform.rotation = Quaternion.identity;

            for(int i = 0; i < ragdollRigid.Length; i++)
            {
                ragdollRigid[i].transform.localPosition = ragdollPos[i];
                ragdollRigid[i].transform.localRotation = ragdollRot[i];
                ragdollRigid[i].velocity = new Vector3(0, 0, 0);
            }
            
            Alive.SetActive(true);
            GetComponent<CapsuleCollider>().enabled = true;
            Dead.SetActive(false);
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
        public void SetChargePoint()
        {
            chargeCount++;
            if(chargeCount>=4)
            {
                chargeCount = 0;
                if(ultimateCount < 10)
                    ultimateCount++;
            }
        }

        [PunRPC]
        public void SetUltimatePoint()
        {
            if (ultimateCount < 10)
                ultimateCount++;
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
                //Debug.Log("current : " + currentSight);
                //Debug.Log("networkSight : " + networkSight);
            }
        }
        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // 데이터를 보내는 부분
                stream.SendNext(playerName);
                stream.SendNext(hp);
                stream.SendNext(isOccupying);
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
                playerName = (string)stream.ReceiveNext();
                hp = (int)stream.ReceiveNext();
                isOccupying = (bool)stream.ReceiveNext();
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