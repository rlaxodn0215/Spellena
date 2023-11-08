using Photon.Pun;
using Player;
using UnityEngine;

public class BurstFlareObject : SpawnObject, IPunObservable
{
    Vector3 direction;

    float lifeTime = 4f;
    float currentLifeTime = 0f;

    public ParticleSystem shootParticle;
    public ParticleSystem explodeParticle;
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
            CheckTime();
        }
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
        transform.rotation = Quaternion.LookRotation(direction);
        currentLifeTime = lifeTime;
        explodeParticle.Stop();
        shootParticle.GetComponent<ParticleEventCall>().explodeEvent += RunExplode;
    }

    void RunExplode(Vector3 pos)
    {
        shootParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        explodeParticle.transform.position = pos;
        explodeParticle.Play(true);
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
