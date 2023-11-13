using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaiaTiedObject : SpawnObject, IPunObservable
{
    public ElementalOrderData elementalOrderData;

    float castingTime;
    float currentCastingTime = 0f;

    float lifeTime;
    float currentLifeTime = 0f;

    float cylinderLifeTime;
    float currentCylinderLifeTime = 0f;


    Vector3 scaleCorrect = new Vector3(0.3f, 0.5f, 0.3f);


    int cylinderCount = 6;

    List<GameObject> cylinders = new List<GameObject>();
    List<bool> reverseScale = new List<bool>();

    int currentCylinderCount = -1;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            OnEnable();
        }
    }
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CheckTimer();
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
            if(currentCylinderLifeTime <= 0f)
            {
                if (currentCylinderCount < cylinderCount - 1)
                {
                    currentCylinderLifeTime = cylinderLifeTime;
                    currentCylinderCount++;
                    cylinders[currentCylinderCount].GetComponent<Collider>().enabled = true;
                }
                else
                {
                    currentCylinderLifeTime = 10f;
                }
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
                                Mathf.Lerp(cylinders[i].transform.localScale.y, 2f * scaleCorrect.y, Time.deltaTime * 2 * scaleCorrect.y), scaleCorrect.z);
                        }
                        else
                        {
                            cylinders[i].transform.localScale = new Vector3(scaleCorrect.x,
                                Mathf.Lerp(cylinders[i].transform.localScale.y, -0.5f * scaleCorrect.y, Time.deltaTime * 2 * scaleCorrect.y), scaleCorrect.z);

                            if(cylinders[i].transform.localScale.y < 0f)
                            {
                                cylinders[i].GetComponent<Collider>().enabled = false;
                            }
                        }
                        cylinders[i].transform.localPosition = new Vector3(cylinders[i].transform.localPosition.x, cylinders[i].transform.localScale.y, cylinders[i].transform.localPosition.z);
                        if (cylinders[i].transform.localScale.y > scaleCorrect.y)
                        {
                            reverseScale[i] = true;
                        }
                    }
                }
                currentCylinderLifeTime -= Time.deltaTime;
            }


            currentLifeTime -= Time.deltaTime;
            if(currentLifeTime <= 0f)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        Init();
    }

    void Init()
    {
        castingTime = elementalOrderData.gaiaTiedCastingTime;
        lifeTime = elementalOrderData.gaiaTiedLifeTime;

        Vector3 _target = (Vector3)data[3];
        transform.rotation = Quaternion.LookRotation(_target);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);

        currentCastingTime = castingTime;
        currentLifeTime = lifeTime;

        //cylinderLifeTime = lifeTime / cylinderCount;
        cylinderLifeTime = 0.1f;

        for(int i = 0; i < cylinderCount; i++)
        {
            cylinders.Add(transform.GetChild(i).gameObject);
            cylinders[i].GetComponent<Collider>().enabled = false;
            cylinders[i].transform.localScale = Vector3.zero;
            cylinders[i].GetComponent<TriggerEventer>().hitTriggerEvent += triggerCylinderEvent;
            reverseScale.Add(false);
        }

    }

    void triggerCylinderEvent(GameObject gameObject)
    {
        if(gameObject.layer != 11)
        {
            Debug.Log("Å¸°Ý");
        }
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);
        if(stream.IsWriting)
        {

        }
        else
        {

        }
    }
}
