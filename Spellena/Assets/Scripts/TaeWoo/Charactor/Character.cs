using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;

namespace Player
{
    public enum PlayerActionState
    {
        Move, Jump, Run, Sit, Interaction, Skill1, Skill2, Skill3, Skill4
    }

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

    // 서버에 사망 및 살인자를 알리기 위한 코드
    public struct PlayerData
    {
        public string playerName;
        public string murder;

        public PlayerData(string name, string murder)
        {
            this.playerName = name;
            this.murder = murder;
        }
    }

    public class Character : MonoBehaviourPunCallbacks, IPunObservable
    {
        PlayerInput playerInput;
        public List<PlayerActionData> playerActionDatas = new List<PlayerActionData>();

        public string playerName;
        public int Hp;
        public PlayerCharactor charactor;
        public GameObject Sight;
        public GameObject groundRaycast;
        public GameObject camera;
        public GameObject UI;

        [HideInInspector]
        public float moveSpeed;
        [HideInInspector]
        public float jumpHeight;
        [HideInInspector]
        public PlayerData playerData;
        [HideInInspector]
        public Animator animator;
        [HideInInspector]
        public Rigidbody rigidbody;
        [HideInInspector]
        public RaycastHit hit;

        // 능력 넣는 Dictionary   
        [HideInInspector]
        public Dictionary<string, Ability> Skills;

        private Vector3 moveVec;
        private bool grounded;

        public bool isOccupying = false;

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // 데이터를 보내는 부분
                stream.SendNext(isOccupying);
                stream.SendNext(playerName);
                stream.SendNext(Hp);
            }
            else
            {
                // 데이터를 받는 부분
                isOccupying = (bool)stream.ReceiveNext();
                playerName = (string)stream.ReceiveNext();
                Hp = (int)stream.ReceiveNext();
            }
        }

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            SetPlayerKeys(PlayerActionState.Move, "Move");
            SetPlayerKeys(PlayerActionState.Jump, "Jump");
            SetPlayerKeys(PlayerActionState.Run, "Run");
            SetPlayerKeys(PlayerActionState.Sit, "Sit");
            SetPlayerKeys(PlayerActionState.Interaction, "Interaction");
            SetPlayerKeys(PlayerActionState.Skill1, "Skill1");
            SetPlayerKeys(PlayerActionState.Skill2, "Skill2");
            SetPlayerKeys(PlayerActionState.Skill3, "Skill3");
            SetPlayerKeys(PlayerActionState.Skill4, "Skill4");
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
        protected virtual void Update()
        {
            RayCasting();
            if (photonView.IsMine)
            {
                isOccupying = false;
            }
        }

        protected virtual void FixedUpdate()
        {
            PlayerMove();
        }

        // 캐릭터에 따른 초기화
        // 캐릭터에 따른 Update
        //gameObject.tag = "Friendly";

        void Initialize()
        {
            animator = GetComponent<Animator>();
            rigidbody = GetComponent<Rigidbody>();
            Skills = new Dictionary<string, Ability>();
            playerData = new PlayerData(playerName, "");
        }

        void RayCasting()
        {
            Ray ray = new Ray(camera.transform.position,camera.transform.forward);
            Physics.Raycast(ray,out hit);
        }

        void OnMove(InputValue value)
        {
            moveVec = new Vector3(value.Get<Vector2>().x, 0, value.Get<Vector2>().y);

            //List<int> _temp = new List<int>();
            if (moveVec.magnitude <= 0)
                playerActionDatas[(int)PlayerActionState.Move].isExecuting = false;
            else
                playerActionDatas[(int)PlayerActionState.Move].isExecuting = true;
        }

        private void OnAnimatorIK()
        {
            SetLookAtObj();
        }

        void SetLookAtObj()
        {
            if (animator == null) return;

            animator.SetLookAtWeight(1f,0.9f);
            animator.SetLookAtPosition(Sight.transform.position);
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

                rigidbody.MovePosition(rigidbody.transform.position + _temp* moveSpeed * Time.deltaTime);
                
            }

            _temp = transform.InverseTransformVector(_temp);

            animator.SetFloat("VerticalSpeed", _temp.z);
            animator.SetFloat("HorizontalSpeed", _temp.x);
        }

        void OnJump()
        {
            if (grounded)
            {
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpHeight, rigidbody.velocity.z);
                playerActionDatas[(int)PlayerActionState.Jump].isExecuting = true;
                animator.SetTrigger("Jump");
                grounded = false;
                animator.SetBool("Grounded", grounded);
            }
        }

        void OnRun()
        {
            Debug.Log("Run!");

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

        void OnSit()
        {

        }

        void OnInteraction()
        {
            Debug.Log("Interaction");
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (animator == null) return;

            if (collision.gameObject.tag == "Enemy")
            {
                //투척무기
                //PlayerDamaged(collision.gameObject.playerName,10);
                //destory
            }

            if (collision.gameObject.tag == "Ground")
            {
                grounded = true;
                playerActionDatas[(int)PlayerActionState.Jump].isExecuting = false;
                animator.SetBool("Grounded", grounded);
            }

        }

        private void OnCollisionStay(Collision collision)
        {
            //if(collision.gameObject.tag == "탑")

            //if (Input.GetKey(KeyCode.F))
            //{
            //    Debug.Log("Healing");
            //}

        }


        private void OnTriggerEnter(Collider other)
        {

        }

        void OnTriggerStay(Collider other)
        {
            if (photonView.IsMine)
            {
                if (other.tag == "OccupationArea")
                {
                    Debug.Log("점령중...");
                    isOccupying = true;
                }
            }
        }

        public void IsLocalPlayer()
        {
            GetComponent<PlayerInput>().enabled = true;
            camera.SetActive(true);
            Transform _temp = transform.GetChild(0).GetChild(0);
            for(int i =0; i < _temp.childCount;i++)
            {
                _temp.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Me");
            }

            UI.SetActive(true);

        }

        [PunRPC]
        public void PlayerDamaged(string enemy ,int damage)
        {
            Hp-=damage;
            if (Hp <= 0)
            {
                // 투척 무기에 쏜 사람 이름 저장
                playerData.murder = enemy;
                // 히트 스캔일 경우 RPC에 쏜 사람 이름 매개변수로 전달
                Debug.Log("죽는것 확인");
                // 죽은 것 서버에 연락 
                //GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, );
            }
            Debug.Log("맞는것 확인");
        }


        //public void PlayerDead(PlayerData data)
        //{
        //    Debug.Log("player dead");
        //}

    }
}