using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class PoolObject : MonoBehaviour
    {
        public delegate void Callback_Disapear(PoolObjectName name, int id);
        Callback_Disapear onDisapear;

        public int objID { get; private set; }
        protected PoolObjectName objectName;
        protected Transform objTrans;

        public void SetPoolObjectData(int _id, PoolObjectName _name, Transform _transform)
        {
            objID = _id;
            objectName = _name;
            objTrans = transform;
        }
        public virtual void InitPoolObject() { }
        public virtual void SetPoolObject(Vector3 direction) { }

        protected virtual void DisActive()
        {
            onDisapear(objectName, objID);  
        }

        public void SetCallback(Callback_Disapear callback_OnDisapear)
        {
            onDisapear = callback_OnDisapear;
        }
    }
}