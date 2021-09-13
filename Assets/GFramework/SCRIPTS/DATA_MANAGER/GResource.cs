using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Framework.DataManager
{
    /// <summary>
    /// Unity Resource的抽象类
    /// </summary>
    /// <typeparam name="T">Resource的类型</typeparam>
    public class GResource<T> where T : class
    {
        protected AssetReference m_asset;

        public GResource() { }
        public GResource(AssetReference asset)
        {
            m_asset = asset;
        }

        public async Task<T> LoadAssetAsync()
        {
            return await m_asset.LoadAssetAsync<T>().Task;
        }
    }


    /// <summary>
    /// 继承自GResource的GameObject资源
    /// </summary>
    public class GGameObjectResource : GResource<GameObject>
    {
        public GGameObjectResource(AssetReference asset)
        {
            m_asset = asset;
        }

        public async Task<GameObject> InstantiateAsync()
        {
            return await m_asset.InstantiateAsync().Task;
        }
    }
}
