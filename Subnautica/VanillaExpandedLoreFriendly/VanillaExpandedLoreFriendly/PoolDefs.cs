using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VanillaExpandedLoreFriendly
{

    [System.Serializable]
    public class PoolObjectSpawnpoint
    {
        public GameObject spawnpoint;
        public PoolContainer pool;
    }

    public static class PoolDefs
    {
        /// <summary>created pools</summary>
        public static Dictionary<string, PoolContainer> pools = new Dictionary<string, PoolContainer>();


        public static void AddObjectToPool(this PoolObject poolObject)
        {
            // deactivate object
            poolObject.gameObject.SetActive(false);

            // add object to pool
            var pool = poolObject.assignedPool;
            pool.poolObjects.Enqueue(poolObject);

            // remove pool object from active objects
            int activeObjectIndex = pool.activeObjects.IndexOf(poolObject);
            if (activeObjectIndex != -1) { pool.activeObjects.Remove(poolObject); }

            // stop pool object (life time) coroutine
            StopPoolObjectLifetimeCoroutine(poolObject);
        }


        public static void StartPoolObjectLifetimeCoroutine(this PoolObject poolObject)
        {
            // exit if no object lifetime is assigned
            if(poolObject.assignedPool.objectLifetime == -1) { return; }

            // stop coroutine if it already exists
            StopPoolObjectLifetimeCoroutine(poolObject);

            // start coroutine lifetime
            poolObject.coroutine_lifeTime = poolObject.StartCoroutine(IPoolObjectLifetime(poolObject));
        }

        public static void StopPoolObjectLifetimeCoroutine(this PoolObject poolObject)
        {
            // stop coroutine & set it to null
            Coroutine coroutine_lifeTime = poolObject.coroutine_lifeTime;
            if (coroutine_lifeTime != null)
            {
                poolObject.StopCoroutine(coroutine_lifeTime);
                poolObject.coroutine_lifeTime = null;
            }
        }

        public static PoolObject TakeObjectFromPool(this PoolContainer pool, Vector3 position, Quaternion rotation, object[] params_onPoolObjectTake = null)
        {
            var activeObjects = pool.activeObjects;
            var poolObjects = pool.poolObjects;

            // if there are no objects in the pool
            if (poolObjects.Count == 0)
            {

                // take action to deactivate and re-use the old or instantiate a new one
                var takeAction = pool.takeAction;
                switch (takeAction)
                {
                    case PoolObjectTakeAction.REUSE_OLD_IF_MAX_REACHED:
                        {
                            // if re-using the old if max is reached
                            if(activeObjects.Count > 0)
                            {
                                // add the oldest object to pool and take it again from the pool for use
                                activeObjects[0].AddObjectToPool();
                                return TakeObjectFromPool(pool, position, rotation);
                            }
                            break;
                        }
                    case PoolObjectTakeAction.CREATE_NEW_IF_MAX_REACHED:
                        // instantiate pool object
                        CreatePoolObject(pool.prefab, pool, pool.poolObjects.Count+1, pool.params_onObjectCreate);

                        // take new created pool object
                        return TakeObjectFromPool(pool, position, rotation);
                    default:
                        { break; }
                }



                return null;
            }

            // activate object and add it to active objects
            PoolObject poolObject = poolObjects.Dequeue();
            var t_poolObject = poolObject.transform;
            t_poolObject.position = position;
            t_poolObject.rotation = rotation;

            poolObject.gameObject.SetActive(true);
            pool.activeObjects.Add(poolObject);

            var action = pool.onPoolObjectTake;
            if (action != null) { action.Invoke(poolObject, params_onPoolObjectTake); }

            return poolObject;
        }

        public static System.Collections.IEnumerator IPoolObjectLifetime(PoolObject poolObject)
        {
            // delay
            yield return poolObject.assignedPool.objectLifeTimeDelay;

            // if pool object exists and is active
            if(poolObject != null && poolObject.gameObject.activeSelf)
            {
                // add object to pool
                poolObject.AddObjectToPool();
            }
        }

        public static void CreatePool(
            string poolName,
            GameObject prefab,
            int size,
            float objectLifetime = -1, 
            PoolObjectTakeAction takeAction = PoolObjectTakeAction.REUSE_OLD_IF_MAX_REACHED, 
            Type objectType = null,
            Action<PoolObject, object[]> onPoolObjectCreate = null,
            Action<PoolObject, object[]> onPoolObjectTake = null,
            object[] params_onObjectCreate = null
            )
        {
            if(objectType == null) { objectType = typeof(PoolObject); }

            // create pool instance
            var pool = new PoolContainer
            {
                prefab = prefab,
                size = size,
                objectLifetime = objectLifetime,
                objectLifeTimeDelay = (objectLifetime != -1) ? new WaitForSeconds(objectLifetime) : null,
                takeAction = takeAction,
                objectType = objectType,
                onPoolObjectCreate = onPoolObjectCreate,
                onPoolObjectTake = onPoolObjectTake,
                params_onObjectCreate = params_onObjectCreate
            };
            

            // add pool to dictionary
            pools.Add(poolName, pool);

            string objectName = prefab.name;

            // fill pool with objects
            for (int i = 0; i < size; i++)
            {
                CreatePoolObject(prefab, pool, i, pool.params_onObjectCreate);
                /*
                PoolObject clone = GameObject.Instantiate(prefab).AddComponent<PoolObject>();
                clone.assignedPool = pool;
                clone.name = $"po_{objectName}[{i + 1}]";
                GameObject.DontDestroyOnLoad(clone);

                clone.AddObjectToPool();*/
            }
        }

        public static PoolObject CreatePoolObject(GameObject prefab, PoolContainer pool, int i, object[] onPoolObjectCreateParams = null)
        {
            // create pool object
            PoolObject clone = GameObject.Instantiate(prefab).AddComponent(pool.objectType) as PoolObject;
            string objectName = prefab.name;
            clone.assignedPool = pool;
            clone.name = $"po_{objectName}[{i + 1}]";
            GameObject.DontDestroyOnLoad(clone);

            // execute onPoolObjectCreate
            if (pool.onPoolObjectCreate != null) { pool.onPoolObjectCreate.Invoke(clone, onPoolObjectCreateParams); }

            // add object to pool
            clone.AddObjectToPool();

            /*
            PoolObject clone = GameObject.Instantiate(prefab).AddComponent<PoolObject>();
            string objectName = prefab.name;
            clone.assignedPool = pool;
            clone.name = $"po_{objectName}[{i + 1}]";
            GameObject.DontDestroyOnLoad(clone);

            clone.AddObjectToPool();
            */
            return clone;
        }

    }


    public class PoolContainer
    {
        // prefab & pool size
        public GameObject prefab;
        public int size;

        // take action
        public PoolObjectTakeAction takeAction;

        // life time
        public float objectLifetime = -1;
        public WaitForSeconds objectLifeTimeDelay;


        // pool object type
        public Type objectType;

        // list of pool & active objects
        public Queue<PoolObject> poolObjects = new Queue<PoolObject>();
        public Action<PoolObject, object[]> onPoolObjectCreate;
        public object[] params_onObjectCreate;
        public Action<PoolObject, object[]> onPoolObjectTake;
        public List<PoolObject> activeObjects = new List<PoolObject>();
    }

    public enum PoolObjectTakeAction
    {
        REUSE_OLD_IF_MAX_REACHED,
        CREATE_NEW_IF_MAX_REACHED
    }
}
