using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;

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

        //실시간 갱신 데이터
        public string playerName;
        public string murder;
        public bool isOccupying = false;

        public int hp;
        public float sitSpeed;
        public float walkSpeed;
        public float runSpeed;
        public float jumpHeight;

        // 데이터 베이스에서 받는 데이터들
        [HideInInspector]
        public int dataHp;
        [HideInInspector]
        public float dataSitSpeed;
        [HideInInspector]
        public float dataWalkSpeed;
        [HideInInspector]
        public float dataRunSpeed;
        [HideInInspector]
        public float dataJumpHeight;

        // 컴포넌트
        [HideInInspector]
        public Animator animator;
        [HideInInspector]
        public Rigidbody rigidbody;
        [HideInInspector]
        public RaycastHit hit;

        // 임시 사용 데이터
        public Vector3 moveVec;
        private bool isGrounded = false;
        private bool isSitting = false;
        private Vector3 cameraPos;
        private Transform avatarForOther;
        private Transform avatarForMe;

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
            cameraPos = camera.transform.position;

        }
        protected virtual void Update()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                isOccupying = false;
            }
            if (photonView.IsMine)
            {
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

                if(playerActionDatas[(int)PlayerActionState.Sit].isExecuting)
                    rigidbody.MovePosition(rigidbody.transform.position + _temp * sitSpeed * Time.deltaTime);
                else if(playerActionDatas[(int)PlayerActionState.Run].isExecuting)
                    rigidbody.MovePosition(rigidbody.transform.position + _temp * runSpeed * Time.deltaTime);
                else
                    rigidbody.MovePosition(rigidbody.transform.position + _temp * walkSpeed * Time.deltaTime);

            }

            _temp = transform.InverseTransformVector(_temp);

            animator.SetInteger("VerticalSpeed", (int)moveVec.z);
            animator.SetInteger("HorizontalSpeed", (int)moveVec.x);
        }

        void OnMove(InputValue value)
        {
            if (photonView.IsMine)
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

            if (photonView.IsMine)
            {
                if (isGrounded)
                {
                    rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpHeight, rigidbody.velocity.z);
                }
            }
        }
        void OnRun()
        {
            if (photonView.IsMine)
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
            if(photonView.IsMine)
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
            if (photonView.IsMine)
            {
                Debug.Log("Interaction");
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(photonView.IsMine)
            {
                if (collision.gameObject.tag == "Ground")
                {
                    isGrounded = true;
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
                }
            }
        }
        void OnTriggerStay(Collider other)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (other.tag == "OccupationArea")
                {
                    Debug.Log("점령중...");
                    isOccupying = true;
                }
            }
        }

        public virtual void IsLocalPlayer()
        {
            if (photonView.IsMine)
            {
                GetComponent<PlayerInput>().enabled = true;
                camera.SetActive(true);
                avatarForOther = transform.GetChild(0).GetChild(0);//다른 사람들이 보는 자신의 아바타
                avatarForMe = transform.GetChild(1).GetChild(0);//자신이 보는 자신의 아바타
                for (int i = 0; i < avatarForOther.childCount; i++)
                {
                    avatarForOther.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Me");
                    avatarForOther.GetChild(i).gameObject.GetComponent<SkinnedMeshRenderer>().enabled = false;
                }

                for(int i = 0; i < avatarForMe.childCount; i++)
                {
                    avatarForMe.GetChild(i).gameObject.layer = LayerMask.NameToLayer("OverlayCameraForMe");
                }

                UI.SetActive(true);
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
        [PunRPC]
        public void PlayerTeleport(Vector3 pos)
        {
            transform.position = pos;
        }

        [PunRPC]
        public void PlayerDamaged(string enemy ,int damage)
        {
            if(hp <= dataHp)
                hp-=damage;

            if (hp <= 0)
            {
                // 투척 무기에 쏜 사람 이름 저장
                murder = enemy;
                // 히트 스캔일 경우 RPC에 쏜 사람 이름 매개변수로 전달
                Debug.Log("죽는것 확인");
            }

            if(damage>0)
                Debug.Log("Player Damaged !!  EnemyName: " + enemy);
            else
                Debug.Log("Player Healing !!");
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
                stream.SendNext(murder);
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
                murder = (string)stream.ReceiveNext();
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