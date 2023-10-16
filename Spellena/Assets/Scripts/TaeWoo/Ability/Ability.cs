using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class Ability : MonoBehaviour
    {
        public virtual void AddPlayer(Charactor player) { }
        public virtual void Execution() { }
    }
}