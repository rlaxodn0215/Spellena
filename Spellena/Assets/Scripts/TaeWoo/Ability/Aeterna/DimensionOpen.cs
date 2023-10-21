using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Player
{
    public class DimensionOpen : Ability
    {
        //public enum Skill2State
        //{
        //    None,
        //    IsActive,
        //    IsDisActive,
        //    Execution
        //}

        public float maxDistance;

        private Aeterna Player;
        private Animator animator;
        private GameObject dimensionDoor;
        private GameObject dimensionDoorGUI;
        //public Skill2State curState = Skill2State.None;
        //public Skill2State updateState = Skill2State.None;

        public Vector3 spawnPoint;
        private Ray ray;
        private RaycastHit hit;
        private bool isShowGUI = false;
        int layerMask;

        //public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        //{
        //    base.OnPhotonSerializeView(stream, info);

        //    if (stream.IsWriting)
        //    {
        //        //데이터를 보내는 부분
        //        Debug.Log("Writing..");
        //         stream.SendNext(spawnPoint);
        //         stream.SendNext(curState);
        //    }

        //    else
        //    {
        //        // 데이터를 받는 부분
        //        Debug.Log("Receiving..");
        //        spawnPoint = (Vector3)stream.ReceiveNext();
                
        //        Skill2State updateState = (Skill2State)stream.ReceiveNext();
        //        if(curState !=updateState)
        //        {
        //            switch (updateState)
        //            {
        //                case Skill2State.None:
        //                    break;
        //                case Skill2State.IsActive:
        //                    photonView.RPC("IsActive",RpcTarget.AllBuffered);
        //                    break;
        //                case Skill2State.IsDisActive:
        //                    photonView.RPC("IsDisActive", RpcTarget.AllBuffered);
        //                    break;
        //                case Skill2State.Execution:
        //                    photonView.RPC("Execution", RpcTarget.AllBuffered);
        //                    break;
        //            }

        //            curState = updateState;
        //        }

        //    }
        //}

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
            //updateState = Skill2State.Execution;

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

            object[] temp = new object[1];
            temp[0] = "Player_" + ID + "_Potal";
            dimensionDoor = PhotonNetwork.Instantiate("TaeWoo/Prefabs/Portal", spawnPoint, Quaternion.identity,0,temp);
            dimensionDoor.tag = Player.gameObject.tag;
            dimensionDoor.GetComponent<DimensionDoor>().owner = Player;
            //PhotonNetwork.Instantiate()

            isShowGUI = false;
        }

        private void Update()
        {
            ShowGUI();

            //if (curState == updateState) return;
            //else
            //{
            //    switch (updateState)
            //    {
            //        case Skill2State.None:
            //            break;
            //        case Skill2State.IsActive:
            //            IsActive();
            //            break;
            //        case Skill2State.IsDisActive:
            //            IsDisActive();
            //            break;
            //        case Skill2State.Execution:
            //            photonView.RPC("Execution", RpcTarget.AllBuffered);
            //            break;
            //    }
            //    curState = updateState;
            //}
        }

        public override void IsActive()
        {
            //updateState = Skill2State.IsActive;

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
                dimensionDoorGUI.name = "Player_" + ID + "_PotalGUI";
                dimensionDoorGUI.tag = Player.gameObject.tag;
                isShowGUI = true;
            }

        }

        public override void IsDisActive()
        {
            //updateState = Skill2State.IsDisActive;

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