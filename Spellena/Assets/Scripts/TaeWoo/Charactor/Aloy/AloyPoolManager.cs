using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class AloyPoolManager : SingletonTemplate<AloyPoolManager>
    {
        public List<PoolManager> poolManagers = new List<PoolManager>();

        public override void Awake()
        {
            base.Awake();

            for(int i = 0; i < transform.childCount; i++)
            {
                if(transform.GetChild(i).GetComponent<PoolManager>() !=null)
                poolManagers.Add(transform.GetChild(i).GetComponent<PoolManager>());
            }
        }
    }
}