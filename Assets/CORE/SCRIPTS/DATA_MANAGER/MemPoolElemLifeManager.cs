using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Framework.DataManager
{
    public class MemPool<T> where T : class, new()
    {
        protected const uint _default_count = 10;
        protected const uint _increase_count = 10;

        public class PoolElement
        {
            public T m_data;
            public bool m_isUsed;
            public float m_lastActiveTime;

            public PoolElement(T data)
            {
                m_data = data;
                m_lastActiveTime = Time.time;
                SetUnActive();
            }

            public void SetActive()
            {
                m_isUsed = true;
                m_lastActiveTime = Time.time;
            }

            public void SetUnActive() => m_isUsed = false;
        };

        protected List<PoolElement> m_freePool;
        protected List<PoolElement> m_usedPool;
        protected Type m_elemValType;

        public MemPool(uint defaultCount = _default_count)
        {
            m_freePool = new List<PoolElement>();
            m_usedPool = new List<PoolElement>();
            m_elemValType = typeof(T);

            if (typeof(T) == typeof(GameObject))
                return;

            for (int index = 0; index < defaultCount; index++)
                m_freePool.Add(CreatePoolElement());
        }

        public MemPool(T template, uint defaultCount = _default_count)
        {
            m_freePool = new List<PoolElement>();
            m_usedPool = new List<PoolElement>();

            for (int index = 0; index < defaultCount; index++)
                m_freePool.Add(CreatePoolElement(template));
        }

        public int GetPoolSize() => m_freePool.Count + m_usedPool.Count;

        public T GetFreeElem()
        {
            PoolElement freeElem;
            if (m_freePool.Count == 0)
                ExpansePool();

            freeElem = m_freePool[m_freePool.Count - 1];
            UsedObject(freeElem.m_data);
            m_freePool.Remove(freeElem);
            m_usedPool.Add(freeElem);
            freeElem.SetActive();
            Debug.Log($"used_count:{GetUsedElemCount()}  free_count:{GetFreeElemCount()}");
            return freeElem.m_data;
        }

        public void FreeUsedElem(PoolElement elem)
        {
            m_usedPool.Remove(elem);
            m_freePool.Add(elem);
            FreeObject(elem.m_data);
            elem.SetUnActive();
        }

        public Type GetElemValType() => m_elemValType;
        public List<PoolElement> GetFreeElements() => m_freePool;
        public List<PoolElement> GetUsedElemenets() => m_usedPool;
        public int GetFreeElemCount() => m_freePool.Count;
        public int GetUsedElemCount() => m_usedPool.Count;


        /*
         *  深拷贝创建
         */
        protected virtual PoolElement CreatePoolElement()
        {
            T data = new T();
            return new PoolElement(data);
        }
        protected virtual PoolElement CreatePoolElement(T template)
        {

            //如果是字符串或值类型则直接返回
            if (!(template is string || template.GetType().IsValueType))
            {
                object retval = Activator.CreateInstance(template.GetType());
                FieldInfo[] fields = template.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (FieldInfo field in fields)
                {
                    try { field.SetValue(retval, field.GetValue(template)); }
                    catch { Debug.Log("throw error..."); }
                }
            }

            return new PoolElement(template);
        }

        protected virtual void UsedObject(T obj) { }
        protected virtual void FreeObject(T obj) { }

        private void ExpansePool()
        {
            for (int index = 0; index < _increase_count; index++)
            {
                PoolElement data = CreatePoolElement(m_usedPool[0].m_data);
                m_freePool.Add(data);
            }
        }  
    }


    public class GameObjectPool : MemPool<GameObject>
    {
        public GameObjectPool(uint defaultCount = _default_count)
        {
            Debug.Log("Come here <GameObject> Child");
        }

        public GameObjectPool(GameObject template, uint defaultCount = _default_count)
        {
            m_freePool = new List<PoolElement>();
            m_usedPool = new List<PoolElement>();

            for (int index = 0; index < defaultCount; index++)
                m_freePool.Add(CreatePoolElement(template));
            CreateElemLifeManager(5);
        }

        private void CreateElemLifeManager(float life)
        {
            GameObject go = new GameObject("MemPool->LifeManager");
            MemPoolElemLifeManager script = go.AddComponent(typeof(MemPoolElemLifeManager)) as MemPoolElemLifeManager;
            script.Initialize(this, life);
        }


        protected override PoolElement CreatePoolElement(GameObject template)
        {
            GameObject go = GameObject.Instantiate(template);
            go.SetActive(false);
            return new PoolElement(go);
        }

        protected override void UsedObject(GameObject obj)
        {
            obj.SetActive(true);
        }
        protected override void FreeObject(GameObject obj)
        {
            obj.SetActive(false);
        }
    }

    public class MemPoolElemLifeManager : MonoBehaviour
    {
        public float m_lifeLength;
        private MemPool<GameObject> memPool;
        private bool m_bInited = false;

        public void Initialize(MemPool<GameObject> pool,float lifeLength)
        {
            memPool = pool;
            m_lifeLength = lifeLength;
            m_bInited = true;
        }

        private void Update()
        {
            if (m_bInited)
            {
                CheckFreePoolElem();
            }
        }

        private void CheckFreePoolElem()
        {
            if (memPool != null)
            {
                for (int index = 0; index < memPool.GetUsedElemCount(); index++)
                {
                    if (index >= memPool.GetFreeElemCount())
                        break;
                    if (Time.time - memPool.GetUsedElemenets()[index].m_lastActiveTime >= m_lifeLength)
                        memPool.FreeUsedElem(memPool.GetUsedElemenets()[index]);
                }
            }
        }

    }
}
