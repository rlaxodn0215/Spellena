using UnityEngine;
using Photon.Pun;
using Player;

public class DimensionSlashAttackCollider : MonoBehaviourPunCallbacks
{
    public DimensionSlash dimensionSlash;

    private void OnTriggerEnter(Collider other)
    {
       if(PhotonNetwork.IsMasterClient)
       {
            if (dimensionSlash.CompareTag("TeamA") && other.transform.root.CompareTag("TeamA") ||
                dimensionSlash.CompareTag("TeamB") && other.transform.root.CompareTag("TeamB"))
            {
                if (dimensionSlash.isHealingSword)
                {
                    if (other.transform.root.GetComponent<Character>() && other.gameObject.layer == LayerMask.NameToLayer("Other"))
                    {
                        other.transform.root.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.AllBuffered, dimensionSlash.playerName,
                            -dimensionSlash.healing, null, Vector3.zero, 0.0f);
                        dimensionSlash.DestorySpawnObject();
                    }
                }
            }

            else if (dimensionSlash.CompareTag("TeamA") && other.transform.root.CompareTag("TeamB") ||
                dimensionSlash.CompareTag("TeamB") && other.transform.root.CompareTag("TeamA"))
            {
                if(!dimensionSlash.isHealingSword)
                {
                    if (other.transform.root.GetComponent<Character>())
                    {
                        other.transform.root.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.AllBufferedViaServer, dimensionSlash.playerName,
                            dimensionSlash.damage, other.name, transform.TransformDirection(Vector3.forward), 20.0f);
                        dimensionSlash.DestorySpawnObject();
                    }
                }
            }
        }
    }
}
