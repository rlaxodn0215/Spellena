using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEventCall : MonoBehaviourPunCallbacks
{
    public event Action<GameObject, Vector3> explodeEvent;

    private void OnParticleCollision(GameObject other)
    {
        ParticleCollisionEvent[] _collisionEvents = new ParticleCollisionEvent[1];
        GetComponent<ParticleSystem>().GetCollisionEvents(other, _collisionEvents);
        explodeEvent(other, _collisionEvents[0].intersection);
    }
}
