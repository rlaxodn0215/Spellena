using UnityEngine;
using Photon.Pun;
using Player;

public class DimensionSlashWallHitCollider : MonoBehaviourPunCallbacks
{
    public DimensionSlash dimensionSlash;

    private void OnTriggerEnter(Collider other)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            if (other.CompareTag("Wall"))
            {
                dimensionSlash.DestorySpawnObject(other.ClosestPointOnBounds(transform.position));
            }
        }
    }
}
