using UnityEngine;
using DefineDatas;

namespace Managers
{
    public class PoolObject : MonoBehaviour
    {
        public delegate void Callback_Disapear(PoolObjectName name, int id);
        Callback_Disapear onDisapear;

        [HideInInspector]
        public bool isUsed;
        [HideInInspector]
        public string userName;
        public int ObjID { get; private set; }
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

        public void SetCallback(Callback_Disapear callback_OnDisapear)
        {
            onDisapear = callback_OnDisapear;
        }
    }
}