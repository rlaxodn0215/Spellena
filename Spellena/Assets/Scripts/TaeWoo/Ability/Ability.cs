using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


namespace Player
{
    public class Ability : MonoBehaviourPunCallbacks
    {
        public virtual void IsActive() { }
        public virtual void AddPlayer(Character player) { }
        public virtual void Execution() { }
    }
}