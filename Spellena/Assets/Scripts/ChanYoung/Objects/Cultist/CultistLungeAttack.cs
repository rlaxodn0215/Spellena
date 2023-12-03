using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultistLungeAttack : MonoBehaviourPunCallbacks
{
    public GameObject lungeCollider;
    public CultistData cultistData;

    List<string> hitObjects = new List<string>();

    public bool isColliderOn = false;
    string playerName;

    private void Start()
    {
        lungeCollider.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerEvent;
        playerName = GetComponent<Player.Character>().playerName;
    }

    public void ResetHitObjects()
    {
        hitObjects.Clear();
    }

    void TriggerEvent(GameObject hitObject)
    {
        if (isColliderOn && PhotonNetwork.IsMasterClient)
        {
            if (hitObject.transform.root.gameObject.name != hitObject.name)
            {
                GameObject _rootObject = hitObject.transform.root.gameObject;
                if (_rootObject.GetComponent<Player.Character>() != null)
                {
                    if (_rootObject.tag != tag)
                    {
                        for (int i = 0; i < hitObjects.Count; i++)
                        {
                            if (_rootObject.name == hitObjects[i])
                                return;
                        }
                        _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.MasterClient,
                         playerName, (int)(cultistData.lungeAttackDamage), hitObject.name, transform.forward, 20f);

                        hitObjects.Add(_rootObject.name);
                    }
                }
            }
        }
    }
}
