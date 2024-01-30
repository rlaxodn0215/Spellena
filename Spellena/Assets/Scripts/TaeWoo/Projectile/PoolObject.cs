using System.Collections;
using UnityEngine;

namespace Managers
{
    public class PoolObject : MonoBehaviour
    {
        public delegate void Callback_Disapear(PoolObjectName name, int id);
        Callback_Disapear onDisapear;

        public int ObjID { get; private set; }
        [HideInInspector]
        public bool isUsed;
        protected PoolObjectName objectName;
        protected Transform objTrans;

        public void SetPoolObjectData(int _id, PoolObjectName _name, Transform _trans)
        {
            ObjID = _id;
            objectName = _name;
            objTrans = _trans;
        }
        public virtual void InitPoolObject() { }
        public virtual void SetPoolObjectTransform(Transform trans) { }

        public virtual void DisActive()
        {
            onDisapear(objectName, ObjID);  
        }

        public virtual void DisActive(float time)
        {
            if(isActiveAndEnabled)
            StartCoroutine(DisActiveTimer(time));
        }

        IEnumerator DisActiveTimer(float time)
        {
            yield return new WaitForSeconds(time);
            DisActive();
        }

        public void SetCallback(Callback_Disapear callback_OnDisapear)
        {
            onDisapear = callback_OnDisapear;
        }
    }
}