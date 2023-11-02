using Photon.Pun;
using Player;
using UnityEngine;

public class BurstFlareObject : SpawnObject, IPunObservable
{
    Vector3 direction;
    Rigidbody rigidbody;

    float lifeTime = 4f;
    float currentLifeTime = 0f;



    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            OnEnable();
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        Init();
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            MovePosition();
            CheckTime();
        }
    }

    void MovePosition()
    {
        rigidbody.MovePosition(this.transform.position + direction * Time.deltaTime * 100);
    }

    void CheckTime()
    {
        currentLifeTime -= Time.deltaTime;
        if(currentLifeTime <= 0f)
        {
            DestorySpawnObject();
        }
    }

    void Init()
    {
        if (data != null)
        {
            this.gameObject.transform.position = (Vector3)data[3];
            direction = (Vector3)data[4];
        }
        currentLifeTime = lifeTime;
        rigidbody = GetComponent<Rigidbody>();
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
