using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Player
{
    public class DimensionOpen : Ability
    {
        public float maxDistance;

        private Aeterna Player;
        private Animator animator;
        private GameObject dimensionDoor;
        private GameObject dimensionDoorGUI;

        public Vector3 spawnPoint;
        private Ray ray;
        private RaycastHit hit;
        private bool isShowGUI = false;
        int layerMask;

        public override void AddPlayer(Character player)
        {
            Player = (Aeterna)player;

            animator = Player.GetComponent<Animator>();

            if (photonView.IsMine)
            {
                layerMask = 1 << LayerMask.NameToLayer("Other");
            }

            else
            {
                layerMask = 1 << LayerMask.NameToLayer("Me");
            }

            layerMask = ~layerMask;

        }

        [PunRPC]
        public override void Execution()
        {
            if (photonView.IsMine)
            {
                IsDisActive();

                animator.SetTrigger("BasicAttack");

                if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
                {
                    spawnPoint = hit.point;
                }

                else
                {
                    spawnPoint = ray.GetPoint(maxDistance);
                }

                spawnPoint.y += 1;

            }

            if(PhotonNetwork.IsMasterClient)
            {
                SpawnPortal(spawnPoint);
            }

            else
            {
                photonView.RPC("RequestSpawnPortal", RpcTarget.MasterClient,spawnPoint);
            }

            isShowGUI = false;
        }
        [PunRPC]
        public void RequestSpawnPortal(Vector3 _spawnPoint)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                SpawnPortal(_spawnPoint);
            }
        }

        void SpawnPortal(Vector3 spawnPoint)
        {
            object[] temp = new object[3];
            temp[0] = Player.playerName;
            temp[1] = tag;
            temp[2] = "Portal";
            dimensionDoor = PhotonNetwork.Instantiate("TaeWoo/Prefabs/Portal", spawnPoint, Quaternion.identity, 0, temp);
        }

        private void Update()
        {
            ShowGUI();
        }

        public override void IsActive()
        {
            if (photonView.IsMine)
            {
                ray = new Ray(Player.camera.transform.position, Player.camera.transform.forward);

                if (Physics.Raycast(ray, out hit, maxDistance,layerMask))
                {
                    spawnPoint = hit.point;
                }

                else
                {
                    spawnPoint = ray.GetPoint(maxDistance);
                }

                spawnPoint.y += 1;
                dimensionDoorGUI = Instantiate(Player.DimensionDoorGUI, spawnPoint, Quaternion.identity);
                dimensionDoorGUI.name = "Player_" + Player.playerName + "_PotalGUI";
                dimensionDoorGUI.tag = Player.gameObject.tag;
                isShowGUI = true;
            }

        }

        public override void IsDisActive()
        {
            isShowGUI = false;
            Destroy(dimensionDoorGUI);
            dimensionDoorGUI = null;
        }

        public void ShowGUI()
        {
            if(isShowGUI && photonView.IsMine)
            {
                ray = new Ray(Player.camera.transform.position, Player.camera.transform.forward);

                if (Physics.Raycast(ray, out hit, maxDistance,layerMask))
                {
                    spawnPoint = hit.point;
                }

                else
                {
                    spawnPoint = ray.GetPoint(maxDistance);
                }

                spawnPoint.y += 1;
                dimensionDoorGUI.transform.position = spawnPoint;
            }
        }


    }
}