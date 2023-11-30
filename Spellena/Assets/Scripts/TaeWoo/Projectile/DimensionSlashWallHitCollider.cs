using UnityEngine;
using Photon.Pun;
using Player;

public class DimensionSlashWallHitCollider : MonoBehaviourPunCallbacks
{
    public DimensionSlash dimensionSlash;
    private int index = 1;

    private void OnTriggerEnter(Collider other)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            if (other.CompareTag("Wall"))
            {
                if(dimensionSlash.isHealingSword)
                {
                    dimensionSlash.DestorySpawnObject(other.ClosestPointOnBounds(transform.position), index);
                }

                else
                {
                    dimensionSlash.DestorySpawnObject(other.ClosestPointOnBounds(transform.position),index-1);
                }
            }
        }
    }
}
