using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Photon.Pun;

public class AeternaSwordTrigger : MonoBehaviour
{
    public AeternaSword aeternaSword;

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit Collider");

        if (other.transform.root.CompareTag(aeternaSword.enemyTag))
        {
            Debug.Log("Hit Enemy");

            if (aeternaSword.player.playerActionDatas[(int)PlayerActionState.Skill2].isExecuting && aeternaSword.player.skill2Phase == 1)
            {
                if (other.transform.root.GetComponent<SpawnObject>())
                {
                    Debug.Log("Hit SpawnObject");

                    if (other.transform.root.GetComponent<SpawnObject>().type == SpawnObjectType.Projectile)
                    {
                        Debug.Log("Hit Projectile");
                        aeternaSword.contactObjectData = other.transform.root.GetComponent<SpawnObject>().data;
                        aeternaSword.player.dimensionIO.CheckHold();
                    }

                    other.transform.root.GetComponent<PhotonView>().RPC("DestoryObject", RpcTarget.AllBuffered);
                }

            }

            else if (aeternaSword.player.playerActionDatas[(int)PlayerActionState.Skill3].isExecuting && aeternaSword.player.skill3Phase == 1)
            {
                //Debug.Log("When skill3");

                if (other.transform.root.GetComponent<Character>())
                {
                    //Debug.Log("Do Skill3");
                    aeternaSword.player.dimensionTransport.Transport(other.transform.root.gameObject);
                }
            }

            else
            {
                if (other.transform.root.GetComponent<Character>())
                    other.transform.root.GetComponent<Character>().PlayerDamaged(aeternaSword.player.playerName, aeternaSword.damage, null, new Vector3(0, 0, 0), 0.0f);
            }
        }

    }
}

