using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;

namespace Player
{
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

        private Vector3 input;
        private bool grounded;

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
            input = transform.TransformVector(new Vector3(value.Get<Vector2>().x, 0, value.Get<Vector2>().y));
            input.Normalize();
        }

        protected void PlayerMove()
        {
            rigidbody.MovePosition(rigidbody.transform.position + input * moveSpeed * Time.deltaTime);
        }

        void OnJump()
        {
            if (grounded)
            {
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpHeight, rigidbody.velocity.z);
                grounded = false;
            }
        }

        //바닥 콜라이더 접촉 확인
        protected void OnTriggerEnter(Collider other)
        {
            grounded = true;
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