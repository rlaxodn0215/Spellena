using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public enum PoolObjectName
    {
        Arrow,
        Ball,
        Strike
    }

    public struct PoolObjectData
    {
        public GameObject obj;
        public int initObjectNum;
        public Transform spawnPosition;

        [HideInInspector]
        public int addObjectNum;
        [HideInInspector]
        public List<PoolObject> objs;
        [HideInInspector]
        public Queue<int> objIDs;
    }

    public class PoolManager : SingletonTemplate<PoolManager>
    {

        //public scriptableobject

        private Dictionary<PoolObjectName, PoolObjectData> poolDatas
             = new Dictionary<PoolObjectName, PoolObjectData>();

        void Start()
        {
            InitPoolDatas();
            CreateObjects();
        }

        void InitPoolDatas()
        {

        }

        void CreateObjects()
        {
            if(poolDatas.Count == 0)
            {
                Debug.Log("소환할 PoolObject가 없습니다.");
                return;
            }

            Dictionary<PoolObjectName, PoolObjectData>.Enumerator iter = poolDatas.GetEnumerator();

            while(iter.MoveNext())
            {
                KeyValuePair<PoolObjectName, PoolObjectData> temp = iter.Current;
                PoolObjectData data = temp.Value;

                data.addObjectNum = data.initObjectNum;
                
                for(int i = 0; i < data.initObjectNum; i++)
                {
                    int id = i + 1;
                    data.objs.Add(CreateNewPoolObject(temp.Key, data, id));
                }

                poolDatas[temp.Key] = data;
            }
        }

        PoolObject CreateNewPoolObject(PoolObjectName name , PoolObjectData pObjData, int id)
        {
            GameObject gb = Instantiate(pObjData.obj, pObjData.spawnPosition); //부모 아래에 소환
            gb.name = pObjData.obj.name + "_" + id;

            gb.transform.position = pObjData.spawnPosition.position;
            gb.transform.rotation = pObjData.spawnPosition.rotation;

            PoolObject poolObj = gb.GetComponent<PoolObject>();
            if (poolObj == null) poolObj = gb.AddComponent<PoolObject>();

            poolObj.SetPoolObjectData(id, name, transform);
            poolObj.SetCallback(DisActiveObject);
            poolObj.InitPoolObject();

            gb.SetActive(false);

            pObjData.objIDs.Enqueue(id);

            return poolObj;
        }

        public PoolObject GetPoolObject(PoolObjectName name)
        {
            if(!poolDatas.ContainsKey(name))
            {
                Debug.LogError("해당 이름의 PoolObject를 찾을 수 없습니다.");
                return null;
            }

            PoolObjectData data = poolDatas[name];

            if (data.objIDs.Count > 0)
            {
                PoolObject ob = data.objs.Find(item => item.objID == data.objIDs.Peek());
                data.objIDs.Dequeue();
                ob.gameObject.SetActive(true);
                return ob;
            }

            else
            {
                PoolObject ob = CreateNewPoolObject(name ,data, ++data.addObjectNum);
                data.objs.Add(ob);
                return ob;
            }
        }

        public PoolObject GetPoolObject(PoolObjectName name, Vector3 pos, Quaternion rot)
        {
            if (!poolDatas.ContainsKey(name))
            {
                Debug.LogError("해당 이름의 PoolObject를 찾을 수 없습니다.");
                return null;
            }

            PoolObjectData data = poolDatas[name];

            if (data.objIDs.Count > 0)
            {
                PoolObject ob = data.objs.Find(item => item.objID == data.objIDs.Peek());
                data.objIDs.Dequeue();
                ob.transform.position = pos;
                ob.transform.rotation = rot;
                ob.gameObject.SetActive(true);
                return ob;
            }

            else
            {
                PoolObject ob = CreateNewPoolObject(name, data, ++data.addObjectNum);
                ob.transform.position = pos;
                ob.transform.rotation = rot;
                data.objs.Add(ob);
                return ob;
            }
        }

        public PoolObject GetPoolObject(PoolObjectName name, Transform _transform)
        {
            if (!poolDatas.ContainsKey(name))
            {
                Debug.LogError("해당 이름의 PoolObject를 찾을 수 없습니다.");
                return null;
            }

            PoolObjectData data = poolDatas[name];

            if (data.objIDs.Count > 0)
            {
                PoolObject ob = data.objs.Find(item => item.objID == data.objIDs.Peek());
                data.objIDs.Dequeue();
                ob.transform.position = _transform.position;
                ob.transform.rotation = _transform.rotation;
                ob.gameObject.SetActive(true);
                return ob;
            }

            else
            {
                PoolObject ob = CreateNewPoolObject(name, data, ++data.addObjectNum);
                ob.transform.position = _transform.position;
                ob.transform.rotation = _transform.rotation;
                data.objs.Add(ob);
                return ob;
            }
        }

        public void DisActiveObject(PoolObjectName name, int id)
        {
            if (!poolDatas.ContainsKey(name))
            {
                Debug.LogError("해당 이름의 PoolObject를 찾을 수 없습니다.");
                return;
            }

            Transform trans = poolDatas[name].objs.Find(item => item.objID == id).transform;

            trans.position = poolDatas[name].spawnPosition.position;
            trans.rotation = poolDatas[name].spawnPosition.rotation;
            trans.gameObject.SetActive(false);

            poolDatas[name].objIDs.Enqueue(id);
        }

    }
}