using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class DimensionSword : Ability
    {
        private Aeterna Player;
        private Animator animator;
        private GameObject Sword;
        public DimensionSword(Aeterna player)
        {
            Player = player;
            //animator = player.GetComponent<Animator>();
            //Sword = player.DimensionSword;
        }
        public void Execution()
        {
            //공격 애니매이션 사용, 칼 활성화
            //만일 타격이 없다면 원거리 공격
            //animator.SetBool("")
            Debug.Log("DimensionSword!");
            
        }
    }
}