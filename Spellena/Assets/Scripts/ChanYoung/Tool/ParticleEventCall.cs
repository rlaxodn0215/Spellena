using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEventCall : MonoBehaviourPunCallbacks, IPunObservable
{
    public event Action<Vector3> explodeEvent;

    private void OnParticleCollision(GameObject other)
    {
        if(other.layer == 11)
        {
            ParticleCollisionEvent[] _collisionEvents = new ParticleCollisionEvent[1];
            GetComponent<ParticleSystem>().GetCollisionEvents(other, _collisionEvents);
            explodeEvent(_collisionEvents[0].intersection);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
