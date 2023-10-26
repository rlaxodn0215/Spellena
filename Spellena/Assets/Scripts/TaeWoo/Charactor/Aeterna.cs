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
        public DimensionTransport dimensionTransport;
        [HideInInspector]
        public DimensionCut dimensionCut;

        [HideInInspector]
        public bool isMouseButton = false;
        [HideInInspector]
        public int skillButton = 0;
        [HideInInspector]
        public float[] skillTimer; //serizeview로 공유

        [HideInInspector]
        public int ultimateCount = 0;
        [HideInInspector]
        public int doUltimateNum = 0;
        [HideInInspector]
        public int chargeCount = 0;
        [HideInInspector]
        public float[] chargeCountTime; // index - 0: 3단계 까지 가기위한 총 시간, 1: 1단계, 2: 2단계 (3단계는 0초)
        private IEnumerator ultimateCoroutine;

        // 0 : 기본 공격
        // 1 : 스킬 1
        // 2 : 스킬 2
        // 3 : 스킬 3
        // 4 : 스킬 4 (궁극기)

        [HideInInspector]
        public int skill2Phase; //1: duration, 2: hold, 3: cool
        [HideInInspector]
        public int skill3Phase; //1: duration, 2: cool

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

            dimensionTransport = this.gameObject.AddComponent<DimensionTransport>();
            dimensionTransport.AddPlayer(this);
            Skills["Skill3"] = dimensionTransport;

            dimensionCut = this.gameObject.AddComponent<DimensionCut>();
            dimensionCut.AddPlayer(this);
            Skills["Skill4"] = dimensionCut;

            skillTimer = new float[Skills.Count+1];

            for(int i = 0; i <= Skills.Count;i++)
            {
                skillTimer[i] = -1;
            }

            skill2Phase = 1;
            skill3Phase = 1;

            hp = AeternaData.Hp;
            walkSpeed = AeternaData.moveSpeed;
            jumpHeight = AeternaData.jumpHeight;

            chargeCountTime = new float[3];

            chargeCountTime[0] = 5;
            chargeCountTime[1] = 4;
            chargeCountTime[2] = 2;
        }

        [PunRPC]
        protected override void SetTag(string team)
        {
            base.SetTag(team);
            DimensionSword.GetComponent<PhotonView>().RPC("SetSwordTag", RpcTarget.AllBufferedViaServer);
        }

        void OnButtonCancel()
        {
            if (photonView.IsMine)
            {
                foreach (KeyValuePair<string, Ability> keyValue in Skills)
                {
                    Ability ability = keyValue.Value;
                    ability.IsDisActive();
                }

                if (skillTimer[0] <= 0.0f)
                {
                    skillButton = 0;
                    Debug.Log("BasicAttack Ready");
                }
            }
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

                if(skill2Phase == 2)
                {
                    skillButton = 2;
                    Debug.Log("Skill2 Ready");
                    return;
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

                if (skillTimer[3] <= 0.0f)
                {
                    if (skillButton == 3)
                    {
                        skillButton = 0;
                        Debug.Log("BasicAttack Ready");
                    }

                    else
                    {
                        skillButton = 3;
                        Skills["Skill3"].IsActive();
                        Debug.Log("Skill3 Ready");
                    }
                }

                else if (skillTimer[0] <= 0.0f)
                {
                    skillButton = 0;
                    Debug.Log("BasicAttack Ready");
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

                if (ultimateCount >= doUltimateNum)
                {
                    skillButton = 4;
                    Skills["Skill4"].IsActive();
                }

                else if(skillTimer[0] <= 0.0f)
                {
                    skillButton = 0;
                    Debug.Log("BasicAttack Ready");
                }


            }
        }

        void OnMouseButton()
        {
            if (photonView.IsMine)
            {
                isMouseButton = !isMouseButton;

                switch (skillButton)
                {
                    case 1:
                        Skill1Execute();
                        break;
                    case 2:
                        Skill2Execute();
                        break;
                    case 3:
                        Skill3Execute();
                        break;
                    case 4:
                        Skill4Execute();
                        break;
                    default:
                        BasicAttackExecute();
                        break;
                }

            }
        }

        private void BasicAttackExecute()
        {
            if (skillTimer[0] <= 0.0f && isMouseButton)
            {
                playerActionDatas[(int)PlayerActionState.BasicAttack].isExecuting = true;
                Skills["BasicAttack"].Execution();
                skillTimer[0] = AeternaData.basicAttackTime;
                StartCoroutine(SkillTimer(0));
            }
        }

        private void Skill1Execute()
        {
            if (skillTimer[1] <= 0.0f && isMouseButton)
            {
                Skills["Skill1"].Execution();
                playerActionDatas[(int)PlayerActionState.Skill1].isExecuting = true;
                skillTimer[1] = AeternaData.skill1Time;
                StartCoroutine(SkillTimer(1));
            }
        }

        private void Skill2Execute()
        {
            if (isMouseButton)
            {
                if (playerActionDatas[(int)PlayerActionState.Skill2].isExecuting == false)
                {
                    switch (skill2Phase)
                    {
                        case 1:
                            skillTimer[2] = AeternaData.skill2DurationTime;
                            DimensionSword.GetComponent<AeternaSword>().skill2BuffParticle.SetActive(true);
                            StartCoroutine(SkillTimer(2));
                            break;
                        case 2:
                            if (skillTimer[2] >= 0.0f)
                                Skills["Skill2"].Execution(ref skill2Phase);
                            break;
                    }

                    playerActionDatas[(int)PlayerActionState.Skill2].isExecuting = true;
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
        }

        private void Skill3Execute()
        {
            if (isMouseButton)
            {
                if (playerActionDatas[(int)PlayerActionState.Skill3].isExecuting == false)
                {
                    if (skill3Phase == 1)
                    {
                        skillTimer[3] = AeternaData.skill3DurationTime;
                        DimensionSword.GetComponent<AeternaSword>().skill3BuffParticle.SetActive(true);
                        StartCoroutine(SkillTimer(3));
                    }
                    playerActionDatas[(int)PlayerActionState.Skill3].isExecuting = true;
                }

                else
                {
                    if (skill3Phase == 1 && skillTimer[0] <= 0.0f)
                    {
                        playerActionDatas[(int)PlayerActionState.Skill3].isExecuting = true;
                        Skills["Skill3"].Execution();
                        skillTimer[0] = AeternaData.basicAttackTime;
                        StartCoroutine(SkillTimer(0));
                    }

                }
            }
        }

        private void Skill4Execute()
        {

            if(playerActionDatas[(int)PlayerActionState.Skill4].isExecuting == false)
            {
                ultimateCount -= doUltimateNum;
                skillTimer[4] = AeternaData.skill4DurationTime;
                StartCoroutine(SkillTimer(4));
            }

            if(isMouseButton)
            {
                skillTimer[5] = chargeCountTime[0];
                ultimateCoroutine = SkillTimer(5);
                StartCoroutine(ultimateCoroutine);
            }

            else
            {
                StopCoroutine(ultimateCoroutine);
                skillTimer[5] = chargeCountTime[0];
                Skills["Skill4"].Execution(ref chargeCount);
                chargeCount = 0;
            }

            playerActionDatas[(int)PlayerActionState.Skill4].isExecuting = true;
        }

        public IEnumerator SkillTimer(int index)
        {
            while (skillTimer[index] > 0.0f)
            {
                skillTimer[index] -= Time.deltaTime;

                if (index == 5)
                {
                    Skill4ChargeTimeCheck(skillTimer[index]);
                }

                yield return null;
            }
            
            if(index !=5)
                playerActionDatas[(int)PlayerActionState.BasicAttack + index].isExecuting = false;

            if (index == 2)
            {
                Skill2TimeOut(ref skill2Phase);
            }

            else if(index == 3)
            {
                Skill3TimeOut(ref skill3Phase);
            }
        }

        void Skill2TimeOut(ref int phase)
        {
            if(phase==1 || phase==2)
            {
                skillTimer[2] = AeternaData.skill2CoolTime;
                DimensionSword.GetComponent<AeternaSword>().skill2BuffParticle.SetActive(false);
                playerActionDatas[(int)PlayerActionState.Skill2].isExecuting = true;
                StartCoroutine(SkillTimer(2));
                phase = 3;
            }

            else
            {
                phase = 1;
            }
        }

        void Skill3TimeOut(ref int phase)
        {
            if (phase == 1)
            {
                skillTimer[3] = AeternaData.skill3CoolTime;
                DimensionSword.GetComponent<AeternaSword>().skill3BuffParticle.SetActive(false);
                playerActionDatas[(int)PlayerActionState.Skill3].isExecuting = true;
                phase = 2;
                StartCoroutine(SkillTimer(3));
            }

            else if(phase==2)
            {
                phase = 1;
            }
        }

        void Skill4ChargeTimeCheck(float time)
        {

            if(time > chargeCountTime[2] && time <= chargeCountTime[1])
            {
                chargeCount = 1;
            }

            else if(time <= chargeCountTime[2] && time > 0.0f)
            {
                chargeCount = 2;
            }

            else if(time <= 0.0f)
            {
                chargeCount = 3;
            }

            else
            {
                chargeCount = 0;
            }
        }

        private void OnGUI()
        {
            GUI.TextField(new Rect(10, 10, 150, 30), "스킬 1 : " + skillTimer[1].ToString());
            GUI.TextField(new Rect(10, 40, 150, 30), "스킬 2 : " + skillTimer[2].ToString());
            GUI.TextField(new Rect(10, 70, 150, 30), "스킬 3 : " + skillTimer[3].ToString());
            GUI.TextField(new Rect(10, 100, 150, 30), "궁 게이지 : " + chargeCount);
            GUI.TextField(new Rect(10, 130, 150, 30), "궁 시전 시간 : " + skillTimer[4].ToString());
        }

    }
}