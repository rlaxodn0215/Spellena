using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using GlobalEnum;

public class PlayerElementalOrder : PlayerCommon
{
    private GameObject castingAura;
    private float distance;

    private List<int> commands = new List<int>();
    private RenderTexture renderTexture;

    public GameObject rightOverlayOrbs;
    public GameObject leftOverlayOrbs;

    object[] instantiateData = new object[2];


    protected override void InitUniqueComponents()
    {
        castingAura = unique.transform.GetChild(0).gameObject;
        renderTexture = cameraMinimap.GetComponent<Camera>().targetTexture;

        for(int i = 0; i < skillDatas.Count; i++)
        {
            skillDatas[i].route.Add(SkillData.State.None);
            skillDatas[i].route.Add(SkillData.State.Unique);
            skillDatas[i].route.Add(SkillData.State.Casting);

            skillDatas[i].networkRoute.Add(SkillData.State.None);
            skillDatas[i].networkRoute.Add(SkillData.State.Casting);
        }

        instantiateData[0] = photonView.ViewID;
        instantiateData[1] = tag;
    }

    protected override void Update()
    {
        base.Update();
        if(photonView.IsMine)
        {
            if(castingAura.activeSelf)
                SetCastingAuraPos();
        }
    }

    protected override void OnMouseButton()
    {
        isClicked = !isClicked;
        if (isClicked)
        {
            //스킬 진행 중에는 다른 기능 사용 불가
            if (IsProgressing())
                return;

            int _unique = CheckUniqueState();
            if(_unique == 3 || _unique == 5)
            {
                if (!CheckPointStrike())
                    return;
            }

            int _index = GetIndexByCommands();//스킬 사용 확인
            if (_index >= 0)
            {
                if (skillDatas[_index].isReady)
                {
                    ChangeNextRoot(CallType.Skill, _index);
                    //photonView.RPC("CallChange", RpcTarget.Others);
                }
            }
        }
    }

    protected override void OnSkill1()
    {
        if (!IsProgressing())
        {
            AddCommand(1);
            photonView.RPC("SyncCommandEffect", RpcTarget.All, commands.ToArray());
        }
    }
    protected override void OnSkill2()
    {
        if (!IsProgressing())
        {
            AddCommand(2);
            photonView.RPC("SyncCommandEffect", RpcTarget.All, commands.ToArray());
        }
    }
    protected override void OnSkill3()
    {
        if (!IsProgressing())
        {
            AddCommand(3);
            photonView.RPC("SyncCommandEffect", RpcTarget.All, commands.ToArray());
        }
    }
    protected override void OnSkill4()
    {
    }

    private int CheckUniqueState()
    {
        for(int i = 0; i < skillDatas.Count; i++)
        {
            if (skillDatas[i].route[skillDatas[i].routeIndex] == SkillData.State.Unique)
                return i;
        }
        return -1;
    }

    private bool CheckPointStrike()
    {
        Ray _tempRay = cameraMinimap.ScreenPointToRay(Input.mousePosition);
        RaycastHit _hit;
        if (Physics.Raycast(_tempRay, out _hit, Mathf.Infinity, layerMaskMap))
        {
            pointStrike = _hit.point;
            return true;
        }
        return false;
    }

    protected override void CancelSkill()
    {
        base.CancelSkill();
        commands.Clear();
        photonView.RPC("SyncCommandEffect", RpcTarget.All, commands.ToArray());
    }


    private int GetIndexByCommands()
    {
        if(commands.Count >= 2)
        {
            if (commands[0] == 1 && commands[1] == 1)
                return 0;
            else if ((commands[0] == 1 && commands[1] == 2) || (commands[0] == 2 && commands[1] == 1))
                return 1;
            else if ((commands[0] == 1 && commands[1] == 3) || (commands[0] == 3 && commands[1] == 1))
                return 2;
            else if (commands[0] == 2 && commands[1] == 2)
                return 3;
            else if ((commands[0] == 2 && commands[1] == 3) || (commands[0] == 3 && commands[1] == 2))
                return 4;
            else
                return 5;
        }
        return -1;
    }

    private void AddCommand(int command)
    {
        if(commands.Count < 2)
            commands.Add(command);
    }

    //이 함수 실행 후 리스너가 변경됨
    protected override void PlaySkillLogic(int index)
    {
        if (photonView.IsMine)
        {
            if (skillDatas[index].route[skillDatas[index].routeIndex] == SkillData.State.Unique)
            {
                if (index == 0 || index == 1 || index == 2 || index == 4)
                    SetCastingAura(index, true);
                else if (index == 3 || index == 5)
                    SetPointStrike(true);
            }
            else if (skillDatas[index].route[skillDatas[index].routeIndex] == SkillData.State.Casting)
            {
                skillDatas[index].isReady = false;
                skillDatas[index].isLocalReady = false;
                skillDatas[index].progressTime = playerData.skillCastingTime[index];
            }
        }
        else
        {

        }
    }

    protected override void PlayPlainLogic(int index)
    {
        if (photonView.IsMine)
        {

        }
        else
        {

        }
    }

    /*
    protected override void PlaySkillLogic(int index)
    {
        if (skillDatas[index].route[skillDatas[index].routeIndex] == SkillData.State.Unique)
        {
            if (index == 0 || index == 1 || index == 2 || index == 4)
                SetCastingAura(index, true);
            else if (index == 3 || index == 5)
                SetPointStrike(true);
        }
        else if (skillDatas[index].route[skillDatas[index].routeIndex] == SkillData.State.Casting)
        {
            skillDatas[index].isReady = false;
            skillDatas[index].isLocalReady = false;
            skillDatas[index].progressTime = playerData.skillCastingTime[index];

            //스킬 로직 실행 -> 자신의 클라이언트는 정상적으로 다음 상태로 이동하는 것이기 때문에 강제적으로 이동한 것이 아님
            PlayLogic(CallType.Skill, skillDatas[index].route[skillDatas[index].routeIndex], index);
            CallPlayAnimation(AnimationChangeType.Invoke, CallType.Skill, index);

            SetCastingAura(index, false);
            SetPointStrike(false);

            commands.Clear();
            //스킬 시전 알림 -> 다른 클라이언트들은 Unique 상태가 존재하지 않아 강제적으로 2계단을 건너뛰는 것으로 인식됨 routeIndex는 이미 바뀌어있음
            if (photonView.IsMine)
            {
                photonView.RPC("NotifyUseSkill", RpcTarget.Others, (int)CallType.Skill, index, skillDatas[index].routeIndex);
                photonView.RPC("SyncCommandEffect", RpcTarget.All, commands.ToArray());
            }
        }
        else if (skillDatas[index].route[skillDatas[index].routeIndex] == SkillData.State.None)
        {
            skillDatas[index].coolDownTime = playerData.skillCoolDownTime[index];
        }
    }
    */
    

    /*
    protected override void PlayForceSkillLogic(int index)
    {
        if (photonView.IsMine)
        {
            SetCastingAura(index, false);
            SetPointStrike(false);
        }
        else//다른 클라이언트에서 변경될 때
        {
            if (skillDatas[index].route[skillDatas[index].routeIndex] == SkillData.State.Casting)
            {
                skillDatas[index].progressTime = playerData.skillCastingTime[index];
                if(PhotonNetwork.IsMasterClient)
                    skillDatas[index].isReady = false;
            }
        }
    }
    */

    protected override void PlayLogic(CallType callType, SkillData.State state, int index)
    {
        if (callType == CallType.Skill)
        {
            if (state == SkillData.State.Casting)
            {
                if (index == 0)
                    PlaySkillLogic1();
                else if (index == 1)
                    PlaySkillLogic2();
                else if (index == 2)
                    PlaySkillLogic3();
                else if (index == 3)
                    PlaySkillLogic4();
                else if (index == 4)
                    PlaySkillLogic5();
                else if (index == 5)
                    PlaySkillLogic6();
            }
        }
    }
    
    /*
    기능 : 지점 타격 위치를 결정하는 기능
    인자 ->
    IsOn : 지점 타격 기능을 On할지 Off할지 결정하는 인자
    */
    private void SetPointStrike(bool IsOn)
    {
        if (IsOn)
        {
            cameraMinimap.GetComponent<Camera>().targetTexture = null;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            minimapMask.SetActive(false);
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            cameraMinimap.GetComponent<Camera>().targetTexture = renderTexture;
            minimapMask.SetActive(true);
        }
        isCameraLocked = IsOn;
    }


    /*
    */
    private void SetCastingAura(int index, bool IsOn)
    {
        castingAura.SetActive(IsOn);
        if (IsOn)
        {
            distance = playerData.skillDistance[index];
            Color _tempColor = Color.red;
            if (index == 0 || index == 1)
            {
                _tempColor = Color.red;
                castingAura.transform.localScale = new Vector3(3, 3, 3);
            }
            else if(index == 2)
            {
                _tempColor = Color.red;
                castingAura.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
            else if(index == 4)
            {
                _tempColor = Color.green;
                castingAura.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
            _tempColor.a = 0.3f;
            castingAura.GetComponent<ParticleSystem>().startColor = _tempColor;
        }
    }


    /*
    기능 : 스킬 시전 범위 위치를 실시간으로 확인하는 기능
    */
    private void SetCastingAuraPos()
    {
        Ray _ray = new Ray();
        _ray = cameraMain.ScreenPointToRay(aim.transform.position);

        RaycastHit _hit;
        if (Physics.Raycast(_ray, out _hit, distance, layerMaskWall | layerMaskMap))
            _ray.origin = _hit.point + new Vector3(0, 0.1f, 0);
        else
            _ray.origin = transform.position + transform.forward * distance;
        _ray.direction = Vector3.down;

        if (Physics.Raycast(_ray, out _hit, Mathf.Infinity, layerMaskMap))
            castingAura.transform.position = _hit.point + new Vector3(0, 0.05f, 0);
    }

    private void PlaySkillLogic1()
    {
        //MeteorStrike, 1, 1
        PhotonNetwork.Instantiate("ChanYoungNew/ElementalOrder/ElementalOrderSkill1", castingAura.transform.position,
            Quaternion.identity, data: instantiateData);
    }

    private void PlaySkillLogic2()
    {
        PhotonNetwork.Instantiate("ChanYoungNew/ElementalOrder/ElementalOrderSkill2", castingAura.transform.position,
        Quaternion.identity, data: instantiateData);
        //RagnaEdge 1, 2
    }

    private void PlaySkillLogic3()
    {
        //BurstFlare 1, 3
        PhotonNetwork.Instantiate("ChanYoungNew/ElementalOrder/ElementalOrderSkill3", castingAura.transform.position,
        transform.rotation, data: instantiateData);
    }

    private void PlaySkillLogic4()
    {
        //GaiaTied 2, 2
        PhotonNetwork.Instantiate("ChanYoungNew/ElementalOrder/ElementalOrderSkill4", pointStrike,
        Quaternion.identity, data: instantiateData);
    }

    private void PlaySkillLogic5()
    {
        //TerraBreak 2, 3
        PhotonNetwork.Instantiate("ChanYoungNew/ElementalOrder/ElementalOrderSkill5", castingAura.transform.position,
            transform.rotation, data: instantiateData);
    }

    private void PlaySkillLogic6()
    {
        //Eterial Storm 3, 3
        PhotonNetwork.Instantiate("ChanYoungNew/ElementalOrder/ElementalOrderSkill6", pointStrike,
            Quaternion.identity, data: instantiateData);
    }




    [PunRPC]
    public void SyncCommandEffect(int[] syncCommand)
    {
        for (int i = 0; i < 3; i++)
        {
            bool _IsOn = rightOverlayOrbs.transform.GetChild(i).gameObject.activeSelf;
            if (syncCommand.Length <= 0)
                rightOverlayOrbs.transform.GetChild(i).gameObject.SetActive(false);
            else if (_IsOn && i != syncCommand[0] - 1)
                rightOverlayOrbs.transform.GetChild(i).gameObject.SetActive(false);
            else if (!_IsOn && i == syncCommand[0] - 1)
                rightOverlayOrbs.transform.GetChild(i).gameObject.SetActive(true);
        }


        for (int i = 0; i < 3; i++)
        {
            bool _IsOn = leftOverlayOrbs.transform.GetChild(i).gameObject.activeSelf;
            if (syncCommand.Length <= 1)
                leftOverlayOrbs.transform.GetChild(i).gameObject.SetActive(false);
            else if (_IsOn && i != syncCommand[1] - 1)
                leftOverlayOrbs.transform.GetChild(i).gameObject.SetActive(false);
            else if (!_IsOn && i == syncCommand[1] - 1)
                leftOverlayOrbs.transform.GetChild(i).gameObject.SetActive(true);
        }

    }

}
