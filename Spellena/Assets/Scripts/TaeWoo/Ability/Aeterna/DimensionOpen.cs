using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Player
{
    public class DimensionOpen : Ability
    {
        public int maxDistance;

        private Aeterna Player;
        private GameObject dimensionDoor;
        private GameObject dimensionDoorGUI;

        private Ray ray;
        private RaycastHit hit;
        private bool isShowGUI = false;

        public override void AddPlayer(Character player)
        {
            Player = (Aeterna)player;
        }

        public override void Execution()
        {
            //ray = new Ray(Player.camera.transform.position, Player.camera.transform.forward);

            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                Vector3 temp = hit.point;
                temp.y += 1;
                dimensionDoor = PhotonNetwork.Instantiate("TaeWoo/Prefabs/" + Player.DimensionDoor.name, temp, Quaternion.identity);
                dimensionDoor.GetComponent<DimensionDoor>().owner = Player;
                isShowGUI = false;
            }

            Debug.Log("DimensionOpen");
        }

        private void Update()
        {
            ShowGUI();
        }

        public override void IsActive()
        {
            ray = new Ray(Player.camera.transform.position, Player.camera.transform.forward);

            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                Vector3 temp = hit.point;
                temp.y += 1;
                dimensionDoorGUI = PhotonNetwork.Instantiate("TaeWoo/Prefabs/" + Player.DimensionDoorGUI.name, temp, Quaternion.identity);
            }

            //else
            //{
            //    tempGUI = PhotonNetwork.Instantiate("TaeWoo/Prefabs/" + dimensionDoorGUI.name, , Quaternion.identity);
            //}

            isShowGUI = true;
        }


        public void ShowGUI()
        {
            if(isShowGUI && dimensionDoorGUI)
            {
                ray = new Ray(Player.camera.transform.position, Player.camera.transform.forward);

                if (Physics.Raycast(ray, out hit, maxDistance))
                {
                    Vector3 temp = hit.point;
                    temp.y += 1;
                    dimensionDoorGUI.transform.position = temp;
                    Debug.Log(hit.point);
                    //PhotonNetwork.Instantiate("TaeWoo/Prefabs/" + dimensionDoorGUI.name, hit.transform.position, Quaternion.identity);
                }
            }
        }


    }
}