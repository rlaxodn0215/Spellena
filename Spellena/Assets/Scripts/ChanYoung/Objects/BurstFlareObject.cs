using Photon.Pun;
using Player;
using UnityEngine;

public class BurstFlareObject : SpawnObject
{
    public ElementalOrderData elementalOrderData;

    public int instantiateCode = -1;

    Vector3 direction;

    float lifeTime;
    float currentLifeTime = 0f;

    public ParticleSystem shootParticle;
    public ParticleSystem explodeParticle;
    void Start()
    {
        Init();
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
            CheckTime();
    }

    void CheckTime()
    {
        currentLifeTime -= Time.deltaTime;
        if(currentLifeTime <= 0f)
        {
            RequestRPC("RequestDestroy");
        }
    }

    void RequestRPC(string tunnelCommand)
    {
        object[] _tempData;
        if(tunnelCommand == "UpdateData")
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
            _tempData[1] = currentLifeTime;
        }
        else
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
        }

        photonView.RPC("CallRPCTunnelElementalOrderSpell2", RpcTarget.AllBuffered, _tempData);
    }

    [PunRPC]
    public void CallRPCTunnelElementalOrderSpell2(object[] data)
    {
        if ((string)data[0] == "UpdateData")
            UpdateData();
        else if ((string)data[0] == "RequestDestroy")
            RequestDestroy();
    }

    void RequestDestroy()
    {
        if(photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    void UpdateData()
    {
        currentLifeTime = (float)data[1];
    }



    void Init()
    {
        lifeTime = elementalOrderData.burstFlareLifeTime;
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
}
