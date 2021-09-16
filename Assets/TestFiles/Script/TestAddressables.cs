using UnityEngine;
using Framework.DataManager;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

public class TestAddressables : MonoBehaviour
{
    GameObjectPool pool;

    /*使用Addressables加载游戏对象*/
    [SerializeField] private AssetReference _asset;
    private GameObject templateObj;

    GGameObjectResource templateObj1;

    private int _skinIndex = 1;
    [ContextMenu("Change Texture")]
    public async void ChangeTexture()
    {
        _skinIndex = _skinIndex == 1 ? 2 : 1;
        var textures = await Addressables.LoadAssetsAsync<Texture2D>(new List<object> { "Sphere Texture", $"Skin{_skinIndex}" }, null, Addressables.MergeMode.Intersection).Task;
        GDebug.Instance.Log($"load texture count={textures.Count}");
        templateObj.GetComponent<Renderer>().material.mainTexture = textures[0];
    }

    private async void OnEnable()
    {
        templateObj = await _asset.LoadAssetAsync<GameObject>().Task;
        GDebug.Instance.Log("Instantiated finished");
        if (pool == null && templateObj != null)
        {
            pool = new GameObjectPool(templateObj);
            pool.ResetMemPoolElemFunc += Reset;
        }
    }

    private void OnDisable()
    {
        _asset.ReleaseInstance(templateObj);
    }

    private async void Start()
    {
        templateObj1 = new GGameObjectResource(_asset);
        GameObject go = await templateObj1.InstantiateAsync();
        go.name = "新创建的对象";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            pool.GetFreeElem().GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(0.0f,2.0f),-1, Random.Range(0.0f, 2.0f)),ForceMode.Impulse);
    }

    private void Reset(GameObject go)
    {
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
}
