using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


namespace Player
{
    public class Ability : MonoBehaviourPunCallbacks,IPunObservable
    {
        public int ID;          // player ID
        public virtual void IsActive() { }
        public virtual void IsDisActive() { }
        public virtual void AddPlayer(Character player) { }
        public virtual void Execution() { }
        public virtual void Execution(ref int time) { }

        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // 데이터를 보내는 부분
                stream.SendNext(ID);
            }

            else
            {
                // 데이터를 받는 부분
                ID = (int)stream.ReceiveNext();
            }
        }
    }
}