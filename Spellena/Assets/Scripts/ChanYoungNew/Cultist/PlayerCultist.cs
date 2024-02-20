using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using GlobalEnum;

public class PlayerCultist : PlayerCommon
{
    private GameObject chaser;
    public GameObject dagger;
    protected ParticleSystem skill3CastingEffect;
    protected ParticleSystem skill3ChannelingEffect;
    protected ParticleSystem skill3OverlayCastingEffect;
    protected ParticleSystem plain2HoldingEffect;

    public int ritualStack = 0;

    protected override void InitUniqueComponents()
    {
        for(int i = 0; i < skillDatas.Count; i++)
        {
            skillDatas[i].route.Add(SkillData.State.None);
            skillDatas[i].route.Add(SkillData.State.Casting);
        }

        //스킬 1
        skillDatas[0].route.Add(SkillData.State.Channeling);
        //스킬 2
        skillDatas[1].route.Add(SkillData.State.Channeling);
        //스킬 3
        skillDatas[2].route.Add(SkillData.State.Channeling);

        for(int i = 0; i < plainDatas.Count; i++)
        {
            plainDatas[i].route.Add(SkillData.State.None);
        }
        plainDatas[0].route.Add(SkillData.State.Casting);

        plainDatas[1].route.Add(SkillData.State.Holding);
        plainDatas[1].route.Add(SkillData.State.Channeling);

        plainDatas[2].route.Add(SkillData.State.Casting);


        skill3CastingEffect = unique.transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
        skill3ChannelingEffect = unique.transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>();
        skill3OverlayCastingEffect = unique.transform.GetChild(0).GetChild(2).GetComponent<ParticleSystem>();
        plain2HoldingEffect = unique.transform.GetChild(1).GetComponent<ParticleSystem>();
    }

    protected override void OnMouseButton()
    {
        base.OnMouseButton();
        if(!isClicked)
        {
            if (plainDatas[1].routeIndex == 1)
                plainDatas[1].routeIndex++;
        }
    }

    protected override void OnMouseButton2()
    {
        isClicked2 = !isClicked;
        if(isClicked2)
        {
            if (IsProgressing())
                return;

            int _index = ChangePlainIndex(plainIndex, 1);
            if (_index >= 0)
            {
                ChangeNextRoot(CallType.Plain, _index);
                plainIndex = _index;
            }
        }
    }

    /*
    protected override void PlayNormalSkillLogic(int index)
    {
        if (skillDatas[index].route[skillDatas[index].routeIndex] == SkillData.State.Casting)
        {
            ResetPlain();
            skillDatas[index].progressTime = playerData.skillCastingTime[index];
            PlayLogic(CallType.Skill, SkillData.State.Casting, index);
            CallPlayAnimation(AnimationChangeType.Invoke, CallType.Skill, index);

            //다른 클라이언트에서도 로직 실행
            if(photonView.IsMine)
                photonView.RPC("NotifyUseSkill", RpcTarget.Others, (int)CallType.Skill, index, skillDatas[index].routeIndex);
        }
        else if (skillDatas[index].route[skillDatas[index].routeIndex] == SkillData.State.Channeling)
        {
            ResetPlain();
            skillDatas[index].progressTime = playerData.skillChannelingTime[index];
            PlayLogic(CallType.Skill, SkillData.State.Channeling, index);
        }
        else if (skillDatas[index].route[skillDatas[index].routeIndex] == SkillData.State.None)
        {
            PlayLogic(CallType.Skill, SkillData.State.None, index);
            skillDatas[index].coolDownTime = playerData.skillCoolDownTime[index];
        }
    }
    */

    /*
    protected override void PlayNormalPlainLogic(int index)
    {
        Debug.Log(plainDatas[index].routeIndex);
        Debug.Log(plainDatas[index].route[plainDatas[index].routeIndex]);
        if (plainDatas[index].route[plainDatas[index].routeIndex] == SkillData.State.Casting)
        {
            plainDatas[index].progressTime = playerData.plainCastingTime[index];
            PlayLogic(CallType.Plain, SkillData.State.Casting, index);
            CallPlayAnimation(AnimationChangeType.Invoke, CallType.Plain, index);

            if (photonView.IsMine)
                photonView.RPC("NotifyUseSkill", RpcTarget.Others, (int)CallType.Plain, index, plainDatas[index].routeIndex);
        }
        else if (plainDatas[index].route[plainDatas[index].routeIndex] == SkillData.State.Holding)
        {

            plainDatas[index].progressTime = playerData.plainCastingTime[index];
            PlayLogic(CallType.Plain, SkillData.State.Holding, index);
            CallPlayAnimation(AnimationChangeType.Invoke, CallType.Plain, index);

            if (photonView.IsMine)
                photonView.RPC("NotifyUseSkill", RpcTarget.Others, (int)CallType.Plain, index, plainDatas[index].routeIndex);
        }
        else if (plainDatas[index].route[plainDatas[index].routeIndex] == SkillData.State.Channeling)
        {
            if (photonView.IsMine && plainDatas[index].progressTime > 0f)
                photonView.RPC("NotifyUseSkill", RpcTarget.Others, (int)CallType.Plain, index, plainDatas[index].routeIndex);

            plainDatas[index].progressTime = playerData.plainChannelingTime[index];
            PlayLogic(CallType.Plain, SkillData.State.Channeling, index);
            CallPlayAnimation(AnimationChangeType.Change, CallType.Plain, index);
        }
        else if (plainDatas[index].route[plainDatas[index].routeIndex] == SkillData.State.None)
        {
            if(index == 1 || index == 2)
                ResetPlain();
        }
    }
    */



    protected override void PlayLogic(CallType callType, SkillData.State state, int index)
    {
        if(photonView.IsMine)
        {
            if(callType == CallType.Skill)
            {
                if (state == SkillData.State.Channeling)
                {
                    if (index == 0)
                        PlaySkillLogic1();
                    else if (index == 1)
                        PlaySkillLogic2();
                    else if (index == 2)
                        PlaySkillLogic3(state);
                }
                else if(state == SkillData.State.Casting)
                {
                    if (index == 2)
                        PlaySkillLogic3(state);
                    if(index == 3)
                        PlaySkillLogic4();
                }
                else if(state == SkillData.State.None)
                {
                    if (index == 1)
                        EndSkillLogic2();
                }

            }
            else if(callType == CallType.Plain)
            {
                if(state == SkillData.State.Casting)
                {
                    if (index == 0)
                        PlayPlainLogic1();
                    else if(index == 2)
                        PlayPlainLogic3();
                }
                else if(state == SkillData.State.Channeling)
                {
                    if(index == 1)
                        EndPlainLogic2();
                }
                else if(state == SkillData.State.Holding)
                {
                    if (index == 1)
                        PlayPlainLogic2();
                }
            }
        }
    }

    private void PlayPlainLogic3()
    {
        //단검 투척
        if(photonView.IsMine)
        {
            object[] _temp = new object[3];
            _temp[0] = photonView.ViewID;
            _temp[1] = tag;
            _temp[2] = cameraMain.ScreenPointToRay(aim.transform.position);

            PhotonNetwork.Instantiate("Dagger", transform.position + transform.forward * 2f, Quaternion.identity, data: _temp);
        }
    }

    private void EndPlainLogic2()
    {
        //돌진 종료
        plain2HoldingEffect.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

    }

    private void PlayPlainLogic2()
    {
        //돌진 상태
        plain2HoldingEffect.startLifetime = playerData.plainCastingTime[1];
        plain2HoldingEffect.Play(false);
    }

    private void PlayPlainLogic1()
    {
        //단검 생성
        dagger.SetActive(true);
    }

    private void PlaySkillLogic1()
    {
        if (photonView.IsMine)
        {
            Debug.Log("크아악");
            //힐
            Ray _ray = cameraMain.ScreenPointToRay(aim.transform.position);
            RaycastHit _hit;
            if (Physics.Raycast(_ray, out _hit, Mathf.Infinity, LayerMask.GetMask("Player") | layerMaskWall | layerMaskMap))
            {
                if (_hit.collider.transform.root.gameObject.layer == 15)
                {
                    if (_hit.collider.transform.root.tag == tag)
                    {
                        PhotonView _photonView = _hit.collider.transform.root.GetComponent<PhotonView>();

                        object[] _tempOther = new object[2];
                        _tempOther[0] = _photonView.ViewID;
                        _tempOther[1] = tag;

                        PhotonNetwork.Instantiate("ChanYoungNew/Cultist/CultistSkill1",
                            _photonView.transform.position + new Vector3(0, 1f, 0), Quaternion.identity, data: _tempOther);

                        return;
                        //_photonView.RPC(); -> 데미지
                    }
                }
            }

            object[] _temp = new object[2];
            _temp[0] = photonView.ViewID;
            _temp[1] = tag;

            PhotonNetwork.Instantiate("ChanYoungNew/Cultist/CultistSkill1",
                            photonView.transform.position + new Vector3(0, 1f, 0), Quaternion.identity, data: _temp);

            //photonView.RPC()-> 데미지
        }

    }

    private void PlaySkillLogic2()
    {
        if (photonView.IsMine)
        {
            object[] _temp = new object[2];
            _temp[0] = photonView.ViewID;
            _temp[1] = tag;

            chaser = PhotonNetwork.Instantiate("ChanYoungNew/Cultist/CultistSkill2",
                            cameraMain.transform.position + transform.forward,
                            transform.rotation * cameraMain.transform.localRotation, data: _temp);

            playerInput.enabled = false;
            //새로운 오브젝트 생성
            //오브젝트로 카메라를 붙음
            //오브젝트 이동
        }
    }

    private void EndSkillLogic2()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(chaser);
            chaser = null;

            playerInput.enabled = true;
        }
    }

    private void PlaySkillLogic3(SkillData.State state)
    {
        //상대의 카메라를 내리는 효과를 킴
        //모든 플레이어에게 Ray를 쏴서 맞는 사람만 시선이 내려감
        if(unique.transform.GetChild(0).gameObject.activeSelf == false)
            unique.transform.GetChild(0).gameObject.SetActive(true);


        if(state == SkillData.State.Casting)
        {
            skill3CastingEffect.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            skill3CastingEffect.Play(false);
            skill3CastingEffect.startLifetime = skillDatas[2].progressTime;

            if (photonView.IsMine)
            {
                skill3OverlayCastingEffect.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                skill3OverlayCastingEffect.Play(false);
                skill3OverlayCastingEffect.startLifetime = skillDatas[2].progressTime;
            }
        }
        else if(state == SkillData.State.Channeling)
        {
            skill3ChannelingEffect.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            skill3ChannelingEffect.Play(false);
            skill3ChannelingEffect.startLifetime = skillDatas[2].progressTime;
        }

    }

    private void PlaySkillLogic4()
    {
        //적군 모두에게 즉사기를 쏨
        if(photonView.IsMine)
        {
            GameObject[] _enemies;
            if (tag == "TeamA")
                _enemies = GameObject.FindGameObjectsWithTag("TeamB");
            else
                _enemies = GameObject.FindGameObjectsWithTag("TeamA");

            List<GameObject> _targets = new List<GameObject>();

            for (int i = 0; i < _enemies.Length; i++)
            {
                GameObject _rootObject = _enemies[i].transform.root.gameObject;
                if(_rootObject.layer == 15)
                    _targets.Add(_rootObject);
            }

            object[] _temp = new object[3];
            _temp[0] = photonView.ViewID;
            _temp[1] = tag;

            for (int i = 0; i < _targets.Count; i++)
            {
                _temp[2] = _targets[i].GetComponent<PhotonView>().ViewID;
                PhotonNetwork.Instantiate("ChanYoungNew/Cultist/CultistSkill4", transform.position + transform.forward * 2, Quaternion.identity, data: _temp);
            }
        }
    }

    [PunRPC]
    public void TeleportPlayer(int photonViewNum)
    {
        if (photonView.IsMine)
        {
            PhotonView _targetView = PhotonNetwork.GetPhotonView(photonViewNum);
            transform.position = _targetView.transform.position + _targetView.transform.forward * 2f;

            Vector3 _direction = -_targetView.transform.forward;

            transform.rotation = Quaternion.LookRotation(_direction);

            cameraMain.transform.localRotation = Quaternion.identity;

            _targetView.RPC("AddState", _targetView.Owner, "Horror", 1.5f, photonView.ViewID);

        }

        if (skillDatas[1].routeIndex == 2)
        {
            ChangeNextRoot(CallType.Skill, 1);
            skillDatas[1].progressTime = 0;
        }
    }

    protected override int ChangePlainIndex(int start, int type)
    {
        if (start == -1)
            return 0;
        else if(start == 0)
        {
            if (type == 0)
                return 1;
            else if (type == 1)
                return 2;
        }
        return -1;
    }

    private void ResetPlain()
    {
        plainIndex = -1;
        for(int i = 0; i < plainDatas.Count; i++)
        {
            plainDatas[i].progressTime = 0;
            plainDatas[i].routeIndex = 0;
        }
        dagger.SetActive(false);
    }

}
