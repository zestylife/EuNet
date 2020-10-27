using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace EuNet.Unity
{
    public class GameObjectPool : SceneSingleton<GameObjectPool>
    {
        public class Pool
        {
            private GameObject _parent;
            private Object _prefab;
            private LinkedList<GameObject> _freeList;
            private int _totalAllocCount = 0;
            private string _prefabPath;

            public Pool(Object prefab, string prefabPath, GameObject parent)
            {
                _parent = parent;
                _prefab = prefab;
                _prefabPath = prefabPath;
                _freeList = new LinkedList<GameObject>();
            }

            public void Prepare(int count)
            {
                CheckParent();

                if (_freeList.Count >= count)
                    return;

                count -= _freeList.Count;

                for (int i = 0; i < count; i++)
                {
                    GameObject obj = GameObject.Instantiate(_prefab) as GameObject;
                    obj.SetActive(false);
                    obj.transform.parent = _parent.transform;

                    NetView view = obj.GetComponent<NetView>();
                    if (view != null)
                    {
                        view.PrefabPath = _prefabPath;
                    }

                    _freeList.AddLast(obj);
                }

                _totalAllocCount += count;
            }

            public GameObject Alloc(Vector3 pos, Quaternion rot, System.Action<GameObject> func = null)
            {
                CheckParent();

                GameObject obj = null;

                while (_freeList.Count > 0)
                {
                    obj = _freeList.First.Value;
                    _freeList.RemoveFirst();

                    if (obj == null)
                        continue;

                    break;
                }

                if (obj == null)
                {
                    try
                    {
                        obj = GameObject.Instantiate(_prefab, pos, rot) as GameObject;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogErrorFormat("Pool Alloc GameObject.Instantiate Exception : {0}", ex.Message);
                    }

                    if (obj == null)
                    {
                        Debug.LogErrorFormat("Pool Alloc GameObject.Instantiate Error : {0}", _prefab.name);
                        return null;
                    }

                    NetView view = obj.GetComponent<NetView>();
                    if (view != null)
                    {
                        view.PrefabPath = _prefabPath;
                    }

                    obj.transform.parent = _parent.transform;

                    obj.transform.localPosition = pos;
                    obj.transform.localRotation = rot;

                    ++_totalAllocCount;

                }
                else
                {
                    obj.transform.localPosition = pos;
                    obj.transform.localRotation = rot;

                    obj.SetActive(true);
                }

                obj.BroadcastMessage("OnPoolInit", SendMessageOptions.DontRequireReceiver);

                if (func != null)
                    func(obj);

                obj.BroadcastMessage("OnPoolPostInit", SendMessageOptions.DontRequireReceiver);

                return obj;
            }

            public void Free(GameObject obj)
            {
                if (_freeList.Contains(obj) == true)
                {
                    Debug.LogError("already free object : " + obj.name, obj);
                    return;
                }

                CheckParent();

                if (obj.transform.parent != _parent.transform)
                    obj.transform.parent = _parent.transform;

                obj.BroadcastMessage("OnPoolDestroy", SendMessageOptions.DontRequireReceiver);
                obj.SetActive(false);
                obj.transform.localPosition = new Vector3(100f, 0f, 100f);

                _freeList.AddLast(obj);
            }

            public void DestroyFreeObject()
            {
                while (_freeList.Count > 0)
                {
                    GameObject.Destroy(_freeList.First.Value);
                    _freeList.RemoveFirst();
                }
            }

            private void CheckParent()
            {
                if (_parent == null)
                {
                    _parent = new GameObject(string.Format("{0} pool", _prefab.name));
                }
            }

            public int GetAllocCount()
            {
                return _totalAllocCount - _freeList.Count;
            }

            public int GetTotalCount()
            {
                return _totalAllocCount;
            }
        }

        private Dictionary<int, Pool> _pool = new Dictionary<int, Pool>();
        private Dictionary<int, int> _hashMap = new Dictionary<int, int>();
        private Dictionary<int, Object> _prefabMap = new Dictionary<int, Object>();

        public static void Prepare(string prefabPath, int count)
        {
            Object prefabObj = Resources.Load(prefabPath);
            if (prefabObj == null)
            {
                Debug.LogErrorFormat("Can't found Prefab : {0}", prefabPath);
                return;
            }

            int pathHash = prefabPath.GetHashCode();

            Instance._prefabMap[pathHash] = prefabObj;

            int hash = prefabObj.GetHashCode();

            Pool pool = null;
            if (Instance._pool.TryGetValue(hash, out pool) == false)
            {
                pool = new Pool(prefabObj, prefabPath, Instance.gameObject);
                Instance._pool[hash] = pool;
            }

            pool.Prepare(count);
        }

        public static GameObject Alloc(string prefabPath, Vector3 pos, Quaternion rot, System.Action<GameObject> func = null)
        {
            int pathHash = prefabPath.GetHashCode();
            Object prefabObj = null;
            Instance._prefabMap.TryGetValue(pathHash, out prefabObj);

            if (prefabObj == null)
            {
                prefabObj = Resources.Load(prefabPath);
                if (prefabObj == null)
                {
                    //Debug.LogErrorFormat("Can't found Prefab : {0}", prefabPath);
                    return null;
                }

                Instance._prefabMap[pathHash] = prefabObj;
            }

            GameObject gameObj = null;

            int hash = prefabObj.GetHashCode();

            Pool pool = null;
            Instance._pool.TryGetValue(hash, out pool);

            if (pool == null)
            {
                pool = new Pool(prefabObj, prefabPath, Instance.gameObject);
                Instance._pool[hash] = pool;
            }

            gameObj = pool.Alloc(pos, rot, func);
            if (gameObj == null)
                return null;

            int objHash = gameObj.GetHashCode();
            if (Instance._hashMap.ContainsKey(objHash) == false)
            {
                Instance._hashMap.Add(objHash, hash);
            }

#if UNITY_EDITOR
            //Instance.SetDebugName();
#endif

            return gameObj;
        }

        public static GameObject Alloc(string name, System.Action<GameObject> func = null)
        {
            return Alloc(name, Vector3.zero, Quaternion.identity, func);
        }

        public static void ClearPrefabCache()
        {
            Instance._prefabMap.Clear();
        }

        public static void Free(GameObject obj, float delaySeconds = 0.0f)
        {
            if (delaySeconds > 0.0001f)
            {
                Instance.StartCoroutine(Instance.DelayedFree(obj, delaySeconds));
            }
            else
            {
                int poolHash = 0;
                int objHash = obj.GetHashCode();

                if (Instance._hashMap.TryGetValue(objHash, out poolHash) == true)
                {
                    Pool pool = null;
                    if (Instance._pool.TryGetValue(poolHash, out pool) == true)
                    {
                        pool.Free(obj);

#if UNITY_EDITOR
                        //Instance.SetDebugName();
#endif
                    }
                    else
                    {
                        Destroy(obj);
                        Debug.LogErrorFormat("invalid Pool Free : {0}", obj);
                    }
                }
                else
                {
                    Destroy(obj);
                    Debug.LogErrorFormat("invalid Free : {0}", obj);
                }
            }
        }

        IEnumerator DelayedFree(GameObject obj, float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);

            Free(obj, 0.0f);
        }

        private void SetDebugName()
        {
            int allocCount = 0;
            int totalCount = 0;

            foreach (KeyValuePair<int, Pool> pair in _pool)
            {
                allocCount += pair.Value.GetAllocCount();
                totalCount += pair.Value.GetTotalCount();
            }

            gameObject.name = string.Format("GameObjectPool {0}/{1}", allocCount, totalCount);

        }
    }
}