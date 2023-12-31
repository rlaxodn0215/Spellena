using Photon.Pun;
using Player;
using UnityEngine;

public class BurstFlareObject : SpawnObject
{
    public ElementalOrderData elementalOrderData;
   
    public int instantiateCode = -1;

    [HideInInspector]
    public bool isHit = false;

    Vector3 direction;

    float lifeTime;
    float currentLifeTime = 0f;

    public ParticleSystem shootParticle;
    public ParticleSystem explodeParticle;

    AudioSource[] audioSources;

    void Start()
    {
        Init();
    }

    private void FixedUpdate()
    {
         CheckTime();
    }

    void CheckTime()
    {
        currentLifeTime -= Time.deltaTime;
        if (currentLifeTime <= 0f)
        {
            if (PhotonNetwork.IsMasterClient)
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

        photonView.RPC("CallRPCTunnelElementalOrderSpell2", RpcTarget.All, _tempData);
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
        audioSources = GetComponents<AudioSource>();

        for(int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i].volume = SettingManager.Instance.effectVal * SettingManager.Instance.soundVal * 0.8f;
        }

        lifeTime = elementalOrderData.burstFlareLifeTime;

        if (data != null)
        {
            this.gameObject.transform.position = (Vector3)data[3];
            direction = (Vector3)data[4];
        }
        transform.rotation = Quaternion.LookRotation(direction);

        currentLifeTime = lifeTime;
        explodeParticle.Stop();
        //shootParticle.GetComponent<ParticleEventCall>().explodeEvent += TriggerParticle;
    }

    void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.IsMasterClient && isHit == false)
        {
            if (other.tag == "Wall" || other.gameObject.layer == 11)
            {
                isHit = true;
                photonView.RPC("RunExplode", RpcTarget.AllBuffered, other.ClosestPointOnBounds(transform.GetChild(2).position));
                photonView.RPC("DestoryObject", RpcTarget.AllBuffered, 0.8f);
                return;
            }

            GameObject _rootObject = other.transform.root.gameObject;
            if(_rootObject.GetComponent<Character>() != null)
            {
                if(_rootObject.tag != tag)
                {
                    isHit = true;
                    _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.All,
                     playerName, (int)(elementalOrderData.burstFlareDamage), other.name, transform.forward, 20f);
                    photonView.RPC("RunExplode", RpcTarget.All, other.ClosestPointOnBounds(transform.GetChild(2).position));
                    photonView.RPC("DestoryObject", RpcTarget.All, 0.8f);
                }
            }
            
        }
    }

    [PunRPC]
    public void RunExplode(Vector3 pos)
    {
        shootParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        explodeParticle.transform.position = pos;
        explodeParticle.Play(true);
        for(int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i].clip.name == "EO-FIRECRASH")
            {
                audioSources[i].volume = SettingManager.Instance.effectVal * SettingManager.Instance.soundVal;
                audioSources[i].Play();
            }
        }
    }
    
}
