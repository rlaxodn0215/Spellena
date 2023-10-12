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
        Move, Jump, Walk, Sit, Interaction, Skill1, Skill2, Skill3, Skill4
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

    public class Charactor : MonoBehaviour
    {
        PlayerInput playerInput;
        public List<PlayerActionData> playerActionDatas = new List<PlayerActionData>();

        public string playerName;
        public int Hp;
        public PlayerCharactor charactor;
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

        // 능력 넣는 Dictionary   
        [HideInInspector]
        public Dictionary<string, Ability> Skills;

        private Vector3 moveVec;
        private bool IsMoving;
        private bool grounded;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            SetPlayerKeys(PlayerActionState.Move, "Move");
            SetPlayerKeys(PlayerActionState.Jump, "Jump");
            SetPlayerKeys(PlayerActionState.Walk, "Walk");
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

        protected void Start()
        {
            //gameObject.tag = "Friendly";
            Initialize();
            CharactorStart();
        }
        protected void Update()
        {
            PlayerSkillInput();
            CharactorUpdate();
        }

        protected void FixedUpdate()
        {
            PlayerMove();
            CharactorFixedUpdate();
        }
        // 캐릭터에 따른 초기화
        protected virtual void CharactorStart() { }
        // 캐릭터에 따른 Update
        protected virtual void CharactorUpdate() { }
        protected virtual void PlayerSkillInput() { }
        protected virtual void CharactorFixedUpdate() { }

        void Initialize()
        {
            animator = GetComponent<Animator>();
            rigidbody = GetComponent<Rigidbody>();
            Skills = new Dictionary<string, Ability>();
            playerData = new PlayerData(playerName, "");
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

        protected void PlayerMove()
        {
            if (playerActionDatas[(int)PlayerActionState.Move].isExecuting)
            {
                Vector3 _temp = new Vector3(0, 0, 0);
                if (moveVec.z > 0)
                    _temp += transform.forward;
                else if (moveVec.z < 0)
                    _temp -= transform.forward;
                if (moveVec.x > 0)
                    _temp += transform.right;
                else if (moveVec.x < 0)
                    _temp -= transform.right;

                _temp.Normalize();

                rigidbody.MovePosition(rigidbody.transform.position + _temp* moveSpeed * Time.deltaTime);
                
            }
        }

        void OnJump()
        {
            if (grounded)
            {
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpHeight, rigidbody.velocity.z);
                playerActionDatas[(int)PlayerActionState.Jump].isExecuting = true;
                grounded = false;
            }
        }

        //바닥 콜라이더 접촉 확인
        protected void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Ground")
            {
                grounded = true;
                playerActionDatas[(int)PlayerActionState.Jump].isExecuting = false;
            }
        }

        protected void OnTriggerStay(Collider other)
        {
            if (other.tag == "OccupationArea")
            {
                Debug.Log("점령중...");
            }
        }

        protected void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Enemy")
            {
                //투척무기
                //PlayerDamaged(collision.gameObject.playerName,10);
                //destory
            }
        }

        // 타워 거점 힐링
        protected void OnCollisionStay(Collision collision)
        {
            //if(collision.gameObject.tag == "탑")

            if(Input.GetKey(KeyCode.F))
            {
                Debug.Log("Healing");
            }
        }
        public void IsLocalPlayer()
        {
            GetComponent<PlayerInput>().enabled = true;
            camera.SetActive(true);
            UI.SetActive(true);
        }

        [PunRPC]
        public void PlayerDamaged(string enemy ,int damage)
        {
            Hp-=damage;
            if (Hp <= 0)
            {
                // 투척 무기에 쏜 사람 이름 저장
                // playerData.murder = tag.gameObject.playerName;
                // 히트 스캔일 경우 RPC에 쏜 사람 이름 매개변수로 전달
                // 죽은 것 서버에 연락 
                //GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, );
            }
        }

        //public void PlayerDead(PlayerData data)
        //{
        //    Debug.Log("player dead");
        //}

    }
}