using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class PoolObject : MonoBehaviour
    {
        public delegate void Callback_Disapear(int id);
        Callback_Disapear onDisapear;

        public int ObjID { get; private set; }
        protected Transform objTrans;

        public void SetID(int _id)
        {
            ObjID = _id;
        }

        public void SetStartTransform(Transform _transform)
        {
            objTrans = _transform;
        }

        public virtual void InitPoolObject() { }
        public virtual void SetPoolObject(Vector3 direction) { }

        protected virtual void DisActive()
        {
            transform.position = objTrans.position;
            transform.rotation = objTrans.rotation;
            gameObject.SetActive(false);
            onDisapear(ObjID);  
        }

        public void SetCallback(Callback_Disapear callback_OnDisapear)
        {
            onDisapear = callback_OnDisapear;
        }
    }
}