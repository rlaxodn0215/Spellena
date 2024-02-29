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
            public List<int> objIDs;
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
                temp.objIDs = new List<int>();

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
                Logging.Log("��ȯ�� PoolObject�� �����ϴ�.");
                ErrorManager.SaveErrorData(ErrorCode.NoSpawnPoolObjects);
                return;
            }

            for (int i = 0; i < poolDatas.Count; i++)
            {
                PoolObjectData data = poolDatas[(PoolObjectName)i];
                data.addObjectNum = data.initObjectNum;

                for (int j = 1; j <= data.initObjectNum; j++)
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
            pObjData.objIDs.Add(id);

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
                Logging.Log("�ش� �̸��� PoolObject�� ã�� �� �����ϴ� : " + name.ToString());
                ErrorManager.SaveErrorData(ErrorCode.CannotFindPoolObjectName, " : " + name.ToString());
                return null;
            }

            PoolObjectData data = poolDatas[name];
            PoolObject ob;
            if (data.objIDs.Count > 0)
            {
                ob = SearchObject(name, data.objIDs[0]);
            }

            else
            {
                ob = CreateNewObject(data, ++data.addObjectNum);
                data.objs.Add(ob);
            }

            data.objIDs.Remove(data.objIDs[0]);
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
                Logging.Log("�ش� �̸��� PoolObject�� ã�� �� �����ϴ� : " + name.ToString());
                ErrorManager.SaveErrorData(ErrorCode.CannotFindPoolObjectName, " : " + name.ToString());
                return null;
            }

            PoolObjectData data = poolDatas[name];
            PoolObject ob;
            if (data.objIDs.Count > 0)
            {
                ob = SearchObject(name, data.objIDs[0]);
            }

            else
            {
                ob = CreateNewObject(data, ++data.addObjectNum);
                data.objs.Add(ob);
            }

            data.objIDs.Remove(data.objIDs[0]);
            poolDatas[name] = data;

            ob.transform.SetPositionAndRotation(pos, rot);
            ob.userName = userName;
            ob.isUsed = true;
            ob.gameObject.SetActive(true);
            return ob;
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
                Logging.Log("�ش� �̸��� PoolObject�� ã�� �� �����ϴ� : " + name.ToString());
                ErrorManager.SaveErrorData(ErrorCode.CannotFindPoolObjectName, " : " + name.ToString());
                return null;
            }

            // PoolObject�� ���� Ž������ ã�´�
            PoolObject ob = BinarySearch(poolDatas[name], id);

            if (ob == null)
            {
                Logging.Log("�ش� ID �� PoolObject�� ã�� �� �����ϴ� : " + id.ToString());
                ErrorManager.SaveErrorData(ErrorCode.CannotFindPoolObjectID, " : " + id.ToString());
                return null;
            }

            return ob;
        }

        /*
        * PoolObject ID�� ����Ž��
        * �Ű�����: Ž�� �� PoolObject ������(PoolObjectData), Ž�� �� PoolObject ID(int)
        * ���� ��: PoolObject
        */
        public PoolObject BinarySearch(PoolObjectData data, int target)
        {
            int start = 0;
            int end = data.addObjectNum-1;
            int mid;

            while(start<=end)
            {
                mid = (start + end) / 2;
                if (data.objs[mid].ObjID == target) return data.objs[mid];
                else if (data.objs[mid].ObjID > target) end = mid - 1;
                else start = mid + 1;
            }

            return null;
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
                Logging.Log("�ش� �̸��� PoolObject�� ã�� �� �����ϴ� : " + name.ToString());
                ErrorManager.SaveErrorData(ErrorCode.CannotFindPoolObjectName, " : " + name.ToString());
                return;
            }

            PoolObject temp = poolDatas[name].objs.Find(item => item.ObjID == id);
            if (temp == default)
            {
                Logging.Log("�ش� ID �� PoolObject�� ã�� �� �����ϴ� : " + id.ToString());
                ErrorManager.SaveErrorData(ErrorCode.CannotFindPoolObjectID, " : " + id.ToString());
                return;
            }

            if (!temp.isUsed) return;
            
            temp.isUsed = false;
            temp.userName = null;
            temp.gameObject.SetActive(false);
            poolDatas[name].objIDs.Add(id);            
        }

    }
}