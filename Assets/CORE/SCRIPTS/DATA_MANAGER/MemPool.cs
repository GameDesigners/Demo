using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Framework.DataManager
{
    public class MemPool<T>  where T : class, new()
    {
        protected class PoolElement
        {
            public T m_data;
            public bool m_isUsed;
            
            public PoolElement(T data)
            {
                m_data = data;
                SetUnActive();
            }

            public void SetActive() => m_isUsed = true;
            public void SetUnActive() => m_isUsed = false;

        }

        protected const uint _default_count = 10;
        protected const uint _increase_count = 10;


        protected List<PoolElement> m_pool;
        protected Type data_type;

        public MemPool(uint defaultCount = _default_count)
        {
            m_pool = new List<PoolElement>();
            data_type = typeof(T);

            if (typeof(T) == typeof(GameObject))
                return;

            for (int index = 0; index < defaultCount; index++)
                m_pool.Add(CreatePoolElement());
        }

        public MemPool(T template, uint defaultCount = _default_count)
        {
            m_pool = new List<PoolElement>();
            for (int index = 0; index < defaultCount; index++)
                m_pool.Add(CreatePoolElement());
        }

        public int GetPoolSize() => m_pool.Count;

        public ref T GetFreeElem()
        {
            for(int index=0;index<m_pool.Count;index++)
            {
                if(!m_pool[index].m_isUsed)
                {
                    m_pool[index].SetActive();
                    UsedObject(ref m_pool[index].m_data);
                    return ref m_pool[index].m_data;
                }
            }
            int newStartIndex = GetPoolSize();
            ExpansePool();
            m_pool[newStartIndex].SetActive();
            UsedObject(ref m_pool[newStartIndex].m_data);
            return ref m_pool[newStartIndex].m_data;
        }

        public void FreeUsedElem(T usedElem)
        {
            for (int index = 0; index < m_pool.Count; index++)
            {
                if (ReferenceEquals(m_pool[index].m_data,usedElem))
                {
                    m_pool[index].SetUnActive();
                    FreeObject(ref m_pool[index].m_data);
                }
            }
        }

        public Type GetPoolElementType() => data_type;

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
                    catch { Debug.Log("Throw error..."); }
                }
            }

            return new PoolElement(template);
        }

        protected virtual void UsedObject(ref T obj) { }
        protected virtual void FreeObject(ref T obj) { }

        private void ExpansePool()
        {
            PoolElement template = new PoolElement(m_pool[0].m_data);
            for (int index = 0; index < _increase_count; index++)
                m_pool.Add(template);
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
            m_pool = new List<PoolElement>();
            for (int index = 0; index < defaultCount; index++)
                m_pool.Add(CreatePoolElement(template));
        }

        protected override PoolElement CreatePoolElement(GameObject template)
        {
            GameObject go = GameObject.Instantiate(template);
            go.SetActive(false);
            return new PoolElement(go);
        }

        protected override void UsedObject(ref GameObject obj) 
        {
            obj.SetActive(true);
        }
        protected override void FreeObject(ref GameObject obj) 
        {
            obj.SetActive(false);
        }
    }
}
