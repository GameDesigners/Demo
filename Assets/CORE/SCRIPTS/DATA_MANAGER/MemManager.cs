using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MemManager
{
    public abstract void Allocation<T>(ref T val) where T : new();
    public abstract void Deallocation<T>(ref T val);

#if UNITY_EDITOR
    protected class DebugLayer
    {
        private uint m_uiAllocationNum;
        private uint m_uiDeallocationNum;

        private static DebugLayer ms_debugLayer;
        public static DebugLayer Instance
        {
            get
            {
                if (ms_debugLayer == null)
                    ms_debugLayer = new DebugLayer();
                return ms_debugLayer;
            }
        }

        private DebugLayer()
        {
            m_uiAllocationNum = 0;
            m_uiDeallocationNum = 0;
        }

        public void AddCallAllocCount() { m_uiAllocationNum++; }
        public void AddCallDeallocCount() { m_uiDeallocationNum++; }
    }
#endif
}

/// <summary>
/// 托管堆内存管理器[针对静态变量的内存管理器]
/// 其他依靠GC即可
/// </summary>
public class MemHeapStaticValManager : MemManager
{
    private MemHeapStaticValManager ms_Instance;
    public MemHeapStaticValManager Instance
    {
        get
        {
            if (ms_Instance == null)
                ms_Instance = new MemHeapStaticValManager();
            return ms_Instance;
        }
    }

    public override void Allocation<T>(ref T val)
    {
        val = new T();
    }

    public override void Deallocation<T>(ref T val)
    {
        
    }
}
