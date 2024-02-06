using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace GameCenterTest0
{
    public abstract class CenterState : MonoBehaviourPunCallbacks
    {
        public GameCenterTest gameCenter;
        public void ConnectCenter(GameCenterTest center)
        {
            gameCenter = center;
        }
        public abstract void StateExecution();
    }
}