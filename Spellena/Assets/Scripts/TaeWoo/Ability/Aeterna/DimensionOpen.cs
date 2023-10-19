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

        private GameObject dimensionDoor;
        public string dimensionDoorName;
        private GameObject dimensionDoorGUI;
        public string dimensionDoorGUIName;

        private Animator animator;

        private Ray ray;
        private RaycastHit hit;
        private bool isShowGUI = false;
        int layerMask;

        public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            base.OnPhotonSerializeView(stream, info);

            if (stream.IsWriting)
            {
                // 데이터를 보내는 부분
                stream.SendNext(dimensionDoorName);
                stream.SendNext(dimensionDoorGUIName);
            }

            else
            {
                // 데이터를 받는 부분
                dimensionDoorName = (string)stream.ReceiveNext();
                dimensionDoorGUIName = (string)stream.ReceiveNext();
            }
        }

        public override void AddPlayer(Character player)
        {
            Player = (Aeterna)player;
            ID = Player.ID;

            if(photonView.IsMine)
            {
                dimensionDoorGUI = PhotonNetwork.Instantiate("TaeWoo/Prefabs/" + Player.DimensionDoorGUI.name, transform.position, Quaternion.identity);
                
                dimensionDoor = PhotonNetwork.Instantiate("TaeWoo/Prefabs/" + Player.DimensionDoor.name, transform.position, Quaternion.identity);
            }

            photonView.RPC("InitSkill", RpcTarget.All,ID);
        }


        [PunRPC]
        void InitSkill(int id)
        {
            if (dimensionDoor == null || dimensionDoorGUI == null)
            {
                dimensionDoorGUI = GameObject.Find("Player_" + Player.ID + "_PortalGUI");
                dimensionDoor = GameObject.Find("Player_" + Player.ID + "_Portal");
            }

            else
            {
                dimensionDoorGUIName = "Player_" + Player.ID + "_PortalGUI";
                dimensionDoorName = "Player_" + Player.ID + "_Portal";
            }

            dimensionDoorGUI.GetComponent<SpawnObject>().ID =id;
            dimensionDoor.GetComponent<SpawnObject>().ID = id;

            animator = Player.GetComponent<Animator>();
            dimensionDoorGUI.tag = Player.gameObject.tag;
            dimensionDoor.tag = Player.gameObject.tag;
            dimensionDoor.GetComponent<DimensionDoor>().owner = Player;
            //dimensionDoor.SetActive(false);
            //dimensionDoorGUI.SetActive(false);

            layerMask = ((1 << LayerMask.NameToLayer("Me")) |
                        (1 << LayerMask.NameToLayer("Other")));  // Everything에서 Me, Other 레이어만 제외하고 충돌 체크함
            layerMask = ~layerMask;
        }


        public override void Execution()
        {
            animator.SetTrigger("BasicAttack");

            dimensionDoorGUI.SetActive(false);
            Vector3 temp;

            if (Physics.Raycast(ray, out hit, maxDistance, layerMask) && dimensionDoor)
            {
                temp = hit.point;
            }

            else
            {
                temp = ray.GetPoint(maxDistance);
            }

            temp.y += 1;

            dimensionDoor.transform.parent = transform.parent;
            dimensionDoor.SetActive(true);
            dimensionDoor.transform.position = temp;
            isShowGUI = false;
            Debug.Log("DimensionOpen");
        }

        private void Update()
        {
            ShowGUI();
            dimensionDoor.name = dimensionDoorName;
            dimensionDoorGUI.name = dimensionDoorGUIName;
        }

        public override void IsActive()
        {
            if (dimensionDoorGUI)
            {
                ray = new Ray(Player.camera.transform.position, Player.camera.transform.forward);
                Vector3 temp;
                if (Physics.Raycast(ray, out hit, maxDistance,layerMask))
                {
                    temp = hit.point;
                }

                else
                {
                    temp = ray.GetPoint(maxDistance);
                }

                temp.y += 1;
                dimensionDoorGUI.SetActive(true);
                dimensionDoorGUI.transform.position = temp;
            }

            isShowGUI = true;
        }

        public override void IsDisActive()
        {
            isShowGUI = false;
            dimensionDoorGUI.SetActive(false);
        }


        public void ShowGUI()
        {
            if(isShowGUI && dimensionDoorGUI)
            {
                ray = new Ray(Player.camera.transform.position, Player.camera.transform.forward);
                Vector3 temp;

                if (Physics.Raycast(ray, out hit, maxDistance,layerMask))
                {
                    temp = hit.point;
                }

                else
                {
                    temp = ray.GetPoint(maxDistance);
                }

                temp.y += 1;
                dimensionDoorGUI.transform.position = temp;
            }
        }


    }
}