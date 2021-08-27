using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Framework.DataManager
{
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
