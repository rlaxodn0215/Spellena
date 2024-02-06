using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CultistSkill3 : MonoBehaviour
{
    List<GameObject> teamAPlayers;
    List<GameObject> teamBPlayers;
    private void Start()
    {
        teamAPlayers = GameObject.FindGameObjectsWithTag("TeamA").ToList();
        teamBPlayers = GameObject.FindGameObjectsWithTag("TeamB").ToList();
    }

    private void FixedUpdate()
    {
        CheckRay();
    }

    private void CheckRay()
    {
        for (int i = 0; i < teamAPlayers.Count; i++)
        {
            if (teamAPlayers[i].transform.root.GetComponent<PhotonView>().IsMine)
            {
                Vector3 _origin = transform.root.position + transform.root.forward * 2;
                Vector3 _direction = teamAPlayers[i].transform.root.forward - transform.root.position;
                PointRay(_origin, _direction);
                return;
            }
        }

        for (int i = 0; i < teamBPlayers.Count; i++)
        {
            if (teamBPlayers[i].transform.root.GetComponent<PhotonView>().IsMine)
            {
                Vector3 _origin = transform.root.position + transform.root.forward * 2;
                Vector3 _direction = teamAPlayers[i].transform.root.forward - transform.root.position;
                PointRay(_origin, _direction);
                return;
            }
        }
    }

    private void PointRay(Vector3 origin, Vector3 direction)
    {
        Ray _ray = new Ray();
        _ray.origin = origin;
        _ray.direction = direction;

        RaycastHit _hit;

        if(Physics.Raycast(_ray, out _hit, Mathf.Infinity,
            LayerMask.GetMask("Map") | LayerMask.GetMask("Wall") | LayerMask.GetMask("Player")))
        {
            if(_hit.collider.transform.root.gameObject.layer == 15)
                _hit.collider.GetComponent<PlayerCommon>().DownCamera();
        }
    }

}
