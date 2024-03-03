using System.Collections.Generic;
using UnityEngine;
using System.Text;
using DefineDatas;

namespace Managers
{
    // ������Ʈ Ǯ�� ����ϴ� PoolManager
    public class PoolManager : SingletonTemplate<PoolManager>
    {
        // PoolObject�� ���� ���� ����
        public struct PoolObjectData
        {
            public PoolObjectName name;
            public GameObject obj;
            public int initObjectNum;
            public int addObjectNum;
            public List<PoolObject> objs;
            public Queue<int> objIDs;
        }

        // PoolObject ������ ��� ScriptableObject
        public PoolManagerData managerData;

        private Dictionary<PoolObjectName, PoolObjectData> poolDatas
             = new Dictionary<PoolObjectName, PoolObjectData>();

        // �ʱ�ȭ �Լ�
        void Start()
        {
            InitPoolDatas();
            CreateObjects();
        }

        /*
        * managerData�� �ִ� ������ poolDatas�� ����
        * �Ű�����: X
        * ���� ��: void
        */
        void InitPoolDatas()
        {
            for (int i = 0; i < managerData.poolObjectDatas.Count; i++)
            {
                PoolObjectData temp = new PoolObjectData();

                temp.name = managerData.poolObjectDatas[i].name;
                temp.obj = managerData.poolObjectDatas[i].obj;
                temp.initObjectNum = managerData.poolObjectDatas[i].initObjectNum;
                temp.addObjectNum = 0;
                temp.objs = new List<PoolObject>();
                temp.objIDs = new Queue<int>();

                poolDatas[temp.name] = temp;
            }
        }

        /*
        * poolDatas�� �ִ� PoolObjectData�� PoolObject ����
        * �Ű�����: X
        * ���� ��: void
        */
        void CreateObjects()
        {
            if (poolDatas.Count == 0)
            {
                ErrorManager.Log("��ȯ�� PoolObject�� �����ϴ�.");
                ErrorManager.SaveErrorData(ErrorCode.NoSpawnPoolObjects);
                return;
            }

            for (int i = 0; i < poolDatas.Count; i++)
            {
                PoolObjectData data = poolDatas[(PoolObjectName)i];
                data.addObjectNum = data.initObjectNum;

                for (int j = 0; j < data.initObjectNum; j++)
                {
                    data.objs.Add(CreateNewObject(data, j));
                }

                poolDatas[(PoolObjectName)i] = data;
            }
        }

        /*
        * ���ο� PoolObject ����
        * �Ű�����: ���� ���� PoolObject�� ������(PoolObjectData), ���� �ΰ��� ID(int)
        * ���� ��: PoolObject
        */
        PoolObject CreateNewObject(PoolObjectData pObjData, int id)
        {
            GameObject gb = Instantiate(pObjData.obj, transform);   // �θ� �Ʒ��� ��ȯ
            gb.name = MakeObjectName(pObjData.obj.name, id);

            PoolObject poolObj = gb.GetComponent<PoolObject>();
            if (poolObj == null) poolObj = gb.AddComponent<PoolObject>();

            gb.SetActive(false);
            poolObj.SetPoolObjectData(id, pObjData.name, transform);
            poolObj.SetCallback(DisActiveObject);                   // Delegate�� DisActiveObject �Լ� ����  
            poolObj.InitPoolObject();
            poolObj.isUsed = false;
            pObjData.objIDs.Enqueue(id);

            return poolObj;
        }

        /*
        * PoolObject �̸� ����
        * �Ű�����: PoolObject�̸�(string), PoolObject ID(int)
        * ���� ��: string
        */
        string MakeObjectName(string objName, int id)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(objName).Append("_").Append(id);
            return sb.ToString();
        }

        /*
        * PoolObject ��� �Լ�
        * �Ű�����: ����� �̸�(string), ��� �� PoolObject �̸�(PoolObjectName)
        * ���� ��: PoolObject
        */
        public PoolObject GetObject(string userName, PoolObjectName name)
        {
            if (!poolDatas.ContainsKey(name))
            {
                ErrorManager.Log("�ش� �̸��� PoolObject�� ã�� �� �����ϴ�(GetObject(string, PoolObjectName)) : " + name.ToString());
                ErrorManager.SaveErrorData(ErrorCode.CannotFindPoolObjectName, " : " + name.ToString() + "from GetObject(string, PoolObjectName)");
                return null;
            }

            PoolObjectData data = poolDatas[name];
            PoolObject ob;
            if (data.objIDs.Count > 0)
            {
                ob = SearchObject(name, data.objIDs.Peek());
            }

            else
            {
                ob = CreateNewObject(data, ++data.addObjectNum);
                data.objs.Add(ob);
            }

            data.objIDs.Dequeue();
            poolDatas[name] = data;

            ob.userName = userName;
            ob.isUsed = true;
            ob.gameObject.SetActive(true);
            return ob;
        }

        /*
        * PoolObject ��� �Լ� (��� ��ġ, ȸ�� ����)
        * �Ű�����: ����� �̸�(string), ��� �� PoolObject �̸�(PoolObjectName), ��� ��ġ(Vector3), ȸ�� ��(Quaternion)
        * ���� ��: PoolObject
        */
        public PoolObject GetObject(string userName, PoolObjectName name, Vector3 pos, Quaternion rot)
        {
            if (!poolDatas.ContainsKey(name))
            {
                ErrorManager.Log("�ش� �̸��� PoolObject�� ã�� �� �����ϴ�(GetObject(string, PoolObjectName, Vector3, Quaternion)) : " + name.ToString());
                ErrorManager.SaveErrorData(ErrorCode.CannotFindPoolObjectName, " : " + name.ToString() + "from GetObject(string, PoolObjectName, Vector3, Quaternion)");
                return null;
            }

            PoolObjectData data = poolDatas[name];
            PoolObject ob;
            if (data.objIDs.Count > 0)
            {
                ob = SearchObject(name, data.objIDs.Peek());
            }

            else
            {
                ob = CreateNewObject(data, ++data.addObjectNum);
                data.objs.Add(ob);
            }

            data.objIDs.Dequeue();
            poolDatas[name] = data;

            ob.transform.SetPositionAndRotation(pos, rot);
            ob.userName = userName;
            ob.isUsed = true;
            ob.gameObject.SetActive(true);
            return ob;
        }

        /*
        * PoolObject ��ȯ �Լ�
        * �Ű�����: ��ȯ �� PoolObject �̸�(PoolObjectName), ��ȯ �� PoolObject ID(int)
        * ���� ��: void
        */
        public void DisActiveObject(PoolObjectName name, int id)
        {
            if (!poolDatas.ContainsKey(name))
            {
                ErrorManager.Log("�ش� �̸��� PoolObject�� ã�� �� �����ϴ�(DisActiveObject(PoolObjectName, int)) : " + name.ToString());
                ErrorManager.SaveErrorData(ErrorCode.CannotFindPoolObjectName, " : " + name.ToString() + "from DisActiveObject(PoolObjectName, int)");
                return;
            }

            PoolObject ob = SearchObject(name, id);
            if (ob == null)
            {
                ErrorManager.Log("�ش� ID �� PoolObject�� ã�� �� �����ϴ�(DisActiveObject(PoolObjectName, int)) : " + id.ToString());
                ErrorManager.SaveErrorData(ErrorCode.CannotFindPoolObjectID, " : " + id.ToString() + "from DisActiveObject(PoolObjectName, int)");
                return;
            }

            if (!ob.isUsed) return;

            ob.isUsed = false;
            ob.userName = null;
            ob.gameObject.SetActive(false);
            poolDatas[name].objIDs.Enqueue(id);
        }

        /*
        * PoolObject Ž�� �Լ�
        * �Ű�����: Ž�� �� PoolObject �̸�(PoolObjectName), Ž�� �� PoolObject ID(int)
        * ���� ��: PoolObject
        */
        public PoolObject SearchObject(PoolObjectName name, int id)
        {
            if (!poolDatas.ContainsKey(name))
            {
                ErrorManager.Log("�ش� �̸��� PoolObject�� ã�� �� �����ϴ�(SearchObject(PoolObjectName, int)) : " + name.ToString());
                ErrorManager.SaveErrorData(ErrorCode.CannotFindPoolObjectName, " : " + name.ToString() + "from SearchObject(PoolObjectName, int)");
                return null;
            }

            // ID�� ����Ʈ �ε��� ��ȣ ����
            PoolObject ob = poolDatas[name].objs[id];
            if (ob == null)
            {
                ErrorManager.Log("�ش� ID �� PoolObject�� ã�� �� �����ϴ�(SearchObject(PoolObjectName, int)) : " + id.ToString());
                ErrorManager.SaveErrorData(ErrorCode.CannotFindPoolObjectID, " : " + id.ToString() + "from SearchObject(PoolObjectName, int)");
                return null;
            }

            return ob;
        }
    }
}