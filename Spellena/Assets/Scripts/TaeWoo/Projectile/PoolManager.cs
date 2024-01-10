using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class PoolManager : MonoBehaviour
    {
        [SerializeField]
        GameObject obj;

        [SerializeField]
        Transform objRoot;

        [SerializeField]
        int initObjectNum;

        [SerializeField]
        private List<PoolObject> objs = new List<PoolObject>();

        [SerializeField]
        private Queue<int> objIDs = new Queue<int>();

        int addObjectNum;

        // Start is called before the first frame update
        void Start()
        {
            CreateObjects(initObjectNum);
        }

        void CreateObjects(int objectNum)
        {
            //Debug.Log("[PoolManager] CreatePlayers : #" + objectNum);
            addObjectNum = objectNum;
            for (int i = 0; i < objectNum; i++)
            {
                int id = i + 1;
                objs.Add(CreatePoolObject(id));
            }
        }

        PoolObject CreatePoolObject(int id)
        {
            GameObject gb = Instantiate(obj, objRoot); //부모 아래에 소환
            gb.name = obj.name + "_" + id;
            gb.transform.position = objRoot.position;
            gb.transform.rotation = objRoot.rotation;

            PoolObject poolObj = gb.GetComponent<PoolObject>();
            if (poolObj == null) Debug.LogError("PoolObject 컴포넌트가 없습니다");
            poolObj.SetID(id);
            poolObj.SetStartTransform(transform);
            poolObj.SetCallback(AddID);
            poolObj.InitPoolObject();

            gb.SetActive(false);

            objIDs.Enqueue(id);

            //Debug.Log("[PoolManager] Created Object ID : " + id);

            return poolObj;
        }

        PoolObject CreateNewPoolObject(int id)
        {
            GameObject gb = Instantiate(obj, objRoot); //부모 아래에 소환
            gb.name = obj.name + "_" + id;
            gb.transform.position = objRoot.position;
            gb.transform.rotation = objRoot.rotation;

            PoolObject poolObj = gb.GetComponent<PoolObject>();
            if (poolObj == null) Debug.LogError("PoolObject 컴포넌트가 없습니다");
            poolObj.SetID(id);
            poolObj.SetStartTransform(transform);
            poolObj.SetCallback(AddID);
            poolObj.InitPoolObject();

            //Debug.Log("[PoolManager] Created new Object ID : " + id);

            return poolObj;
        }

        public PoolObject GetObject()
        {
            if (objIDs.Count > 0)
            {
                PoolObject ob = objs.Find(item => item.ObjID == objIDs.Peek());
                objIDs.Dequeue();
                ob.gameObject.SetActive(true);
                return ob;
            }

            else
            {
                PoolObject ob = CreateNewPoolObject(++addObjectNum);
                objs.Add(ob);
                return ob;
            }
        }

        public PoolObject GetObject(Transform _transform)
        {
            if (objIDs.Count > 0)
            {
                PoolObject ob = objs.Find(item => item.ObjID == objIDs.Peek());
                objIDs.Dequeue();
                ob.transform.position = _transform.position;
                ob.transform.rotation = _transform.rotation;
                ob.gameObject.SetActive(true);
                return ob;
            }

            else
            {
                PoolObject ob = CreateNewPoolObject(++addObjectNum);
                objs.Add(ob);
                return ob;
            }
        }

        void AddID(int id)
        {
            objIDs.Enqueue(id);
        }

    }
}