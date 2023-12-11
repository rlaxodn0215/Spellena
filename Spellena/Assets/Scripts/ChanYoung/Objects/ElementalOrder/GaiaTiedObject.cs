using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GaiaTiedObject : SpawnObject
{
    public ElementalOrderData elementalOrderData;

    float castingTime;
    float currentCastingTime = 0f;

    float lifeTime;
    float currentLifeTime = 0f;

    float cylinderCheckTime;
    float currentCylinderCheckTime = 0f;


    Vector3 scaleCorrect = new Vector3(0.3f, 0.5f, 0.3f);

    int cylinderCount;

    List<GameObject> cylinders = new List<GameObject>();
    List<bool> reverseScale = new List<bool>();
    List<Vector3> cylindersLerpScale = new List<Vector3>();
    List<Vector3> cylindersLerpPos = new List<Vector3>();
    List<string> hitObjects = new List<string>();

    int currentCylinderCount = -1;

    void Start()
    {
        Init();
    }
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
            CheckTimer();
        LerpCylinder();
    }

    void LerpCylinder()
    {
        for(int i = 0; i < cylinders.Count; i++)
        {
            cylinders[i].transform.localScale = Vector3.Lerp(cylinders[i].transform.localScale, cylindersLerpScale[i], Time.deltaTime * 10);
        }

        for(int i = 0; i < cylinders.Count; i++)
        {
            cylinders[i].transform.localPosition = Vector3.Lerp(cylinders[i].transform.localPosition, cylindersLerpPos[i], Time.deltaTime * 10);
        }
    }

    void CheckTimer()
    {
        if(currentCastingTime > 0f)
        {
            currentCastingTime -= Time.deltaTime;
        }
        else
        {
            if(currentCylinderCheckTime <= 0f)
            {
                if (currentCylinderCount < cylinderCount - 1)
                {
                    currentCylinderCount++;
                    currentCylinderCheckTime = cylinderCheckTime;
                    RequestRPC("ActiveCollider", currentCylinderCount);
                }
                else
                    currentCylinderCheckTime = 100f;
            }
            else
            {
                for(int i = 0; i < cylinders.Count; i++)
                {
                    if(cylinders[i].GetComponent<Collider>().enabled == true)
                    {
                        if (reverseScale[i] == false)
                        {
                            cylinders[i].transform.localScale = new Vector3(scaleCorrect.x,
                                cylinders[i].transform.localScale.y + elementalOrderData.gaiaTiedLifeTime[i] * Time.deltaTime * 2, scaleCorrect.z);
                        }
                        else
                        {
                            if (cylinders[i].transform.localScale.y >= 0f)
                            {
                                cylinders[i].transform.localScale = new Vector3(scaleCorrect.x,
                                   cylinders[i].transform.localScale.y - elementalOrderData.gaiaTiedLifeTime[i] * Time.deltaTime * 2, scaleCorrect.z);
                            }
                            if(cylinders[i].transform.localScale.y <= 0f)
                                RequestRPC("InactiveCollider", i);
                        }
                        cylinders[i].transform.localPosition = new Vector3(cylinders[i].transform.localPosition.x, cylinders[i].transform.localScale.y, cylinders[i].transform.localPosition.z);
                        if (cylinders[i].transform.localScale.y > scaleCorrect.y)
                            reverseScale[i] = true;
                    }
                }
                currentCylinderCheckTime -= Time.deltaTime;
            }
            currentLifeTime -= Time.deltaTime;
            if(currentLifeTime <= 0f)
            {
                RequestRPC("RequestDestroy");
            }
        }

        RequestRPC("UpdateData");
    }

    void Init()
    {
        cylinderCount = elementalOrderData.gaiaTiedLifeTime.Length;
        castingTime = elementalOrderData.gaiaTiedCastingTime;

        float _tempLifeTime = 0;
        for(int i = 0; i < 6; i++)
        {
            _tempLifeTime += elementalOrderData.gaiaTiedLifeTime[i];
        }
        lifeTime = _tempLifeTime + 0.5f;

        Vector3 _target = (Vector3)data[3];
        transform.rotation = Quaternion.LookRotation(_target);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);

        currentCastingTime = castingTime;
        currentLifeTime = lifeTime;

        //cylinderLifeTime = lifeTime / cylinderCount;
        cylinderCheckTime = lifeTime / cylinderCount / 5;

        for (int i = 0; i < cylinderCount; i++)
        {
            cylinders.Add(transform.GetChild(i).gameObject);
            cylinders[i].GetComponent<Collider>().enabled = false;
            cylinders[i].transform.localScale = new Vector3(scaleCorrect.x, 0, scaleCorrect.z);
            cylindersLerpScale.Add(cylinders[i].transform.localScale);
            cylindersLerpPos.Add(cylinders[i].transform.localPosition);
            cylinders[i].GetComponent<TriggerEventer>().hitTriggerEvent += TriggerCylinderEvent;
            reverseScale.Add(false);
        }
    }

    void RequestRPC(string tunnelCommand)
    {
        object[] _tempData;
        if(tunnelCommand == "UpdateData")
        {
            _tempData = new object[6 + cylinderCount * 2 + 1];
            _tempData[0] = tunnelCommand;
            _tempData[1] = currentCastingTime;
            _tempData[2] = currentLifeTime;
            _tempData[3] = currentCylinderCheckTime;
            _tempData[4] = currentCylinderCount;
            _tempData[5] = reverseScale.ToArray();

            for(int i = 0; i < cylinderCount; i++)
            {
                _tempData[6 + i] = cylinders[i].transform.localScale;
            }

            for(int i = 0; i < cylinderCount; i++)
            {
                _tempData[6 + cylinderCount + i] = cylinders[i].transform.localPosition;
            }

            _tempData[18] = hitObjects.ToArray();
        }
        else
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
        }

        photonView.RPC("CallRPCTunnelElementalOrderSpell3", RpcTarget.AllBuffered, _tempData);
    }

    void RequestRPC(string tunnelCommand, int cylinderNum)
    {
        object[] _tempData;
        if (tunnelCommand == "ActiveCollider" || tunnelCommand == "InactiveCollider")
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
            _tempData[1] = cylinderNum;
            photonView.RPC("CallRPCTunnelElementalOrderSpell3", RpcTarget.AllBuffered, _tempData);
        }
    }


    [PunRPC]
    public void CallRPCTunnelElementalOrderSpell3(object[] data)
    {
        if ((string)data[0] == "UpdateData")
            UpdateData(data);
        else if ((string)data[0] == "ActiveCollider")
            ActiveCollider(data);
        else if ((string)data[0] == "InactiveCollider")
            InactiveCollider(data);
        else if ((string)data[0] == "RequestDestroy")
            RequestDestroy();
    }

    void RequestDestroy()
    {
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    void ActiveCollider(object[] data)
    {
        cylinders[(int)data[1]].GetComponent<Collider>().enabled = true;
        cylinders[(int)data[1]].GetComponent<AudioSource>().volume = SettingManager.Instance.effectVal * SettingManager.Instance.soundVal;
        cylinders[(int)data[1]].GetComponent<AudioSource>().Play();
    }

    void InactiveCollider(object[] data)
    {
        cylinders[(int)data[1]].GetComponent<Collider>().enabled = false;
    }

    void UpdateData(object[] data)
    {
        currentCastingTime = (float)data[1];
        currentLifeTime = (float)data[2];
        currentCylinderCheckTime = (float)data[3];
        currentCylinderCount = (int)data[4];
        reverseScale = ((bool[])data[5]).ToList();

        for(int i = 0; i < cylinders.Count; i++)
        {
            cylindersLerpScale[i] = (Vector3)data[6 + i];
        }

        for(int i = 0; i < cylinders.Count; i++)
        {
            cylindersLerpPos[i] = (Vector3)data[6 + cylinderCount + i];
        }

        hitObjects = ((string[])data[18]).ToList();
    }

    void TriggerCylinderEvent(GameObject hitObject)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if(hitObject.transform.root.gameObject.name != hitObject.name)
            {
                GameObject _rootObject = hitObject.transform.root.gameObject;
                if (_rootObject.GetComponent<Character>() != null)
                {
                    if (tag != _rootObject.tag)
                    {
                        for (int i = 0; i < hitObjects.Count; i++)
                        {
                            if (hitObjects[i] == _rootObject.name)
                                return;
                        }

                        hitObjects.Add(_rootObject.name);
                        _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.All,
                                playerName, (int)elementalOrderData.gaiaTiedDamage, hitObject.name, transform.forward, 20f);
                    }
                }
            }
        }
    }
}
