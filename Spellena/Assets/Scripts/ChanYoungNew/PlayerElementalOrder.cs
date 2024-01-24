using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerElementalOrder : PlayerCommon
{
    private GameObject castingAura;
    private float distance;

    private List<int> commands = new List<int>();
    private RenderTexture renderTexture;
    private bool isPointStrikeOn;


    protected override void InitUniqueComponents()
    {
        AddSkill(2);
        for(int i = 0; i < skillDatas.Count; i++)
            skillDatas[i].isUnique = true;

        castingAura = unique.transform.GetChild(0).gameObject;
        renderTexture = cameraMinimap.GetComponent<Camera>().targetTexture;
    }

    private void Update()
    {
        if(photonView.IsMine)
        {
            if(castingAura.activeSelf)
                SetCastingAuraPos();
        }
    }

    protected override void OnSkill1()
    {
        AddCommand(1);
    }
    protected override void OnSkill2()
    {
        AddCommand(2);
    }
    protected override void OnSkill3()
    {
        AddCommand(3);
    }
    protected override void OnSkill4()
    {
    }

    protected override void OnMouseButton()
    {
        isClicked = !isClicked;
        if(isClicked)
        {
            if (!CheckUniqueMouseClick())
                return;
            int _index = GetIndexByCommands();
            //¿¤¸®¸àÅ» ¿À´õ´Â ÆòÅ¸°¡ ¾ø´Ù
            if (_index >= 0)
            {
                if (skillDatas[_index].skillState == SkillData.SkillState.None)
                    photonView.RPC("ClickMouse", RpcTarget.MasterClient, _index, false);
                else if (skillDatas[_index].skillState == SkillData.SkillState.Unique)
                    photonView.RPC("ClickMouse", RpcTarget.MasterClient, _index, true);
            }
        }
    }

    virtual protected bool CheckUniqueMouseClick()
    {
        if (isCameraLocked)
            return CheckPointStrike();
        return true;
    }

    virtual protected bool CheckPointStrike()
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

    [PunRPC]
    public override void SetSkillPlayer(int index, int nextSkillState)
    {
        base.SetSkillPlayer(index, nextSkillState);
        if (photonView.IsMine && (SkillData.SkillState)nextSkillState == SkillData.SkillState.Casting)
            commands.Clear();
    }

    protected override void PlayUniqueState(int index, bool IsOn)
    {

        if (index == 0 || index == 1 || index == 4)
            SetCastingAura(index, IsOn);
        else if (index == 3 || index == 5)
            SetPointStrike(index, IsOn);
    }
    private void SetPointStrike(int index, bool IsOn)
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
            else if(index == 4)
            {
                _tempColor = Color.green;
                castingAura.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
            _tempColor.a = 0.3f;
            castingAura.GetComponent<ParticleSystem>().startColor = _tempColor;
        }
    }

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

    protected override void PlaySkillLogic(int index, SkillTiming timing)
    {
        if (timing != SkillTiming.Immediately)
            return;

        if (index == 0)
            PlaySkillLogic1();
        else if(index == 1)
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

    private void PlaySkillLogic1()
    {
        //MeteorStrike, 1, 1
        object[] _temp = new object[2];
        _temp[0] = photonView.ViewID;
        _temp[1] = tag;
        PhotonNetwork.Instantiate("ChanYoungNew/ElementalOrder/ElementalOrderSkill1", castingAura.transform.position,
            Quaternion.identity, data: _temp);

        Debug.Log("Áö±ÝÀÌ´Ï?");
    }

    private void PlaySkillLogic2()
    {
        object[] _temp = new object[2];
        _temp[0] = photonView.ViewID;
        _temp[1] = tag;
        PhotonNetwork.Instantiate("ChanYoungNew/ElementalOrder/ElementalOrderSkill2", castingAura.transform.position,
    Quaternion.identity, data: _temp);
        //RagnaEdge 1, 2
    }

    private void PlaySkillLogic3()
    {
        //BurstFlare 1, 3
    }

    private void PlaySkillLogic4()
    {
        //GaiaTied 2, 2
        object[] _temp = new object[2];
        _temp[0] = photonView.ViewID;
        _temp[1] = tag;

        PhotonNetwork.Instantiate("ChanYoungNew/ElementalOrder/ElementalOrderSkill4", pointStrike,
        Quaternion.identity, data: _temp);
    }

    private void PlaySkillLogic5()
    {
        //TerraBreak 2, 3
        object[] _temp = new object[2];
        _temp[0] = photonView.ViewID;
        _temp[1] = tag;
        PhotonNetwork.Instantiate("ChanYoungNew/ElementalOrder/ElementalOrderSkill5", castingAura.transform.position,
            transform.rotation, data: _temp);
    }

    private void PlaySkillLogic6()
    {
        //Eterial Storm 3, 3
        object[] _temp = new object[2];
        _temp[0] = photonView.ViewID;
        _temp[1] = tag;
        PhotonNetwork.Instantiate("ChanYoungNew/ElementalOrder/ElementalOrderSkill6", pointStrike,
            Quaternion.identity, data: _temp);
    }

}
