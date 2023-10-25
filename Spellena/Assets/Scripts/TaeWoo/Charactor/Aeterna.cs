using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

namespace Player
{
    public class Aeterna : Character
    {
        public CharacterData AeternaData;
        public GameObject DimensionSword;
        public GameObject DimensionSlash;
        public GameObject DimensionDoor;
        public GameObject DimensionDoorGUI;

        [HideInInspector]
        public DimensionSword dimensionSword;
        [HideInInspector]
        public DimensionOpen dimensionOpen;
        [HideInInspector]
        public DimensionIO dimensionIO;
        [HideInInspector]
        public int skillButton = 0;
        [HideInInspector]
        public float[] skillTimer;

        // 0 : 기본 공격
        // 1 : 스킬 1
        // 2 : 스킬 2
        // 3 : 스킬 3
        // 4 : 스킬 4 (궁극기)

        [HideInInspector]
        public int skill2Phase; // 0: None 1: duration, 2: hold, 3: cool

        protected override void Start() 
        {
            base.Start();
            Initialize();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        void Initialize()
        {
            DimensionSword.tag = tag;
            camera.GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer(tag);

            dimensionSword = this.gameObject.AddComponent<DimensionSword>();
            dimensionSword.AddPlayer(this);
            Skills["BasicAttack"] = dimensionSword;

            dimensionOpen = this.gameObject.AddComponent<DimensionOpen>();
            dimensionOpen.AddPlayer(this);
            dimensionOpen.maxDistance = AeternaData.skill1DoorRange;
            Skills["Skill1"] = dimensionOpen;

            dimensionIO = this.gameObject.AddComponent<DimensionIO>();
            dimensionIO.AddPlayer(this);
            Skills["Skill2"] = dimensionIO;

            skillTimer = new float[Skills.Count];

            for(int i = 0; i < Skills.Count;i++)
            {
                skillTimer[i] = -1;
            }

            skill2Phase = 1;

            hp = AeternaData.Hp;
            walkSpeed = AeternaData.moveSpeed;
            jumpHeight = AeternaData.jumpHeight;
        }

        [PunRPC]
        protected override void SetTag(string team)
        {
            base.SetTag(team);
            DimensionSword.GetComponent<PhotonView>().RPC("SetSwordTag", RpcTarget.AllBufferedViaServer);
        }

        void OnSkill1()
        {
            if (photonView.IsMine)
            {
                foreach (KeyValuePair<string, Ability> keyValue in Skills)
                {
                    Ability ability = keyValue.Value;
                    ability.IsDisActive();
                }

                if (skillTimer[1] <= 0.0f)
                {
                    if (skillButton == 1)
                    {
                        skillButton = 0;
                        Debug.Log("BasicAttack Ready");
                    }

                    else
                    {
                        skillButton = 1;
                        Skills["Skill1"].IsActive();
                        Debug.Log("Skill1 Ready");
                    }
                }

                else if (skillTimer[0] <= 0.0f)
                {
                    skillButton = 0;
                    Debug.Log("BasicAttack Ready");
                }
            }
        }

        void OnSkill2()
        {
            if (photonView.IsMine)
            {
                foreach (KeyValuePair<string, Ability> keyValue in Skills)
                {
                    Ability ability = keyValue.Value;
                    ability.IsDisActive();
                }

                if (skillTimer[2] <= 0.0f)
                {
                    if (skillButton == 2)
                    {
                        skillButton = 0;
                        Debug.Log("BasicAttack Ready");
                    }

                    else
                    {
                        skillButton = 2;
                        Debug.Log("Skill2 Ready");
                    }
                }

                else if (skillTimer[0] <= 0.0f)
                {
                    skillButton = 0;
                    Debug.Log("BasicAttack Ready");
                }
            }
        }

        void OnSkill3()
        {
            if (photonView.IsMine)
            {
                foreach (KeyValuePair<string, Ability> keyValue in Skills)
                {
                    Ability ability = keyValue.Value;
                    ability.IsDisActive();
                }
            }
        }

        void OnSkill4()
        {
            if (photonView.IsMine)
            {
                foreach (KeyValuePair<string, Ability> keyValue in Skills)
                {
                    Ability ability = keyValue.Value;
                    ability.IsDisActive();
                }
            }
        }

        void OnMouseButton()
        {
            if (photonView.IsMine)
            {
                if (skillButton == 1 && skillTimer[1] <= 0.0f)
                {
                    Skills["Skill1"].Execution();
                    playerActionDatas[(int)PlayerActionState.Skill1].isExecuting = true;
                    skillTimer[1] = AeternaData.skill1Time;
                    StartCoroutine(SkillTimer(1));
                }

                else if (skillButton == 2)
                {
                    if (playerActionDatas[(int)PlayerActionState.Skill2].isExecuting == false)
                    {
                        switch (skill2Phase)
                        {
                            case 0:
                                break;
                            case 1:
                                skillTimer[2] = AeternaData.skill2DurationTime;
                                playerActionDatas[(int)PlayerActionState.Skill2].isExecuting = true;
                                StartCoroutine(SkillTimer(2));
                                break;
                            case 2:
                                if (skillTimer[2] >= 0.0f)
                                    Skills["Skill2"].Execution(ref skill2Phase);
                                playerActionDatas[(int)PlayerActionState.Skill2].isExecuting = true;
                                StartCoroutine(SkillTimer(2));
                                break;
                            case 3:
                                skillTimer[2] = AeternaData.skill2CoolTime;
                                playerActionDatas[(int)PlayerActionState.Skill2].isExecuting = true;
                                StartCoroutine(SkillTimer(2));
                                break;
                        }
                    }

                    else
                    {
                        if (skill2Phase == 1 && skillTimer[0] <= 0.0f)
                        {
                            Skills["Skill2"].Execution(ref skill2Phase);
                            skillTimer[0] = AeternaData.basicAttackTime;
                            StartCoroutine(SkillTimer(0));
                        }

                    }

                }

                else
                {
                    if (skillButton == 0 && skillTimer[0] <= 0.0f)
                    {
                        playerActionDatas[(int)PlayerActionState.BasicAttack].isExecuting = true;
                        Skills["BasicAttack"].Execution();
                        skillTimer[0] = AeternaData.basicAttackTime;
                        StartCoroutine(SkillTimer(0));
                    }
                }
            }
        }

        public IEnumerator SkillTimer(int index)
        {
            while (skillTimer[index] > 0.0f)
            {
                skillTimer[index] -= Time.deltaTime;
                yield return null;
            }

            playerActionDatas[(int)PlayerActionState.BasicAttack + index].isExecuting = false;

            Skill2TimeOut(index);
        }

        void Skill2TimeOut(int index)
        {
            if(index == 2)
            {
                skillTimer[2] = AeternaData.skill2CoolTime;
                playerActionDatas[(int)PlayerActionState.Skill2].isExecuting = true;
                StartCoroutine(SkillTimer(2));
            }
        }

        private void OnGUI()
        {
            GUI.TextField(new Rect(10, 10, 100, 30), skillTimer[2].ToString());
        }

    }
}