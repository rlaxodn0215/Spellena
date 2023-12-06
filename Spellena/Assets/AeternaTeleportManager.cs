using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AeternaTeleportManager : MonoBehaviourPunCallbacks
{
    public GameObject particle;

    Vector3 particlePos;
    RaycastHit hit;

    [PunRPC]
    public void UseTeleportManager(Vector3 startPos, Vector3 destPos, int actorNumber)
    {
        transform.position = startPos;
        SpawnPartice();

        transform.position = destPos;
        SpawnPartice();

        PlayAudioLocal(actorNumber);
    }

    void SpawnPartice()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        Physics.Raycast(ray, out hit, Mathf.Infinity);
        particlePos = hit.point + new Vector3(0f, 0.01f, 0f);
        Instantiate(particle,particlePos,Quaternion.Euler(-90,0,0));
    }

    void PlayAudioLocal(int actorNumber)
    {
        if(PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
            GetComponent<AudioSource>().Play();
    }

}
