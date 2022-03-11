using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityEditorSample
{
    //在 “Asset/Create”  菜单添加创建菜单按钮
    [CreateAssetMenu(menuName = "EditorExample/Create ExampleAsset Instance(In Asset Menu)")]
    public class ExampleAsset : ScriptableObject
    {
        //数据
        public int id = 1;
        public string assetName = "ExampleAsset";



        [MenuItem("Help/EditorExample/Create ExampleAsset Instance")]
        private static void CreateExampleAsserInstance()
        {
            var exampleAsset = CreateInstance<ExampleAsset>();
            AssetDatabase.CreateAsset(exampleAsset, "Assets/EditorSamples/ScriptableAsset/ExampleAsset.asset");
            AssetDatabase.Refresh();
        }

        [MenuItem("Help/EditorExample/Load ExampleAsset Instance Sample")]
        private static void LoadScriptableObjectSample()
        {
            var exampleAsset = AssetDatabase.LoadAssetAtPath<ExampleAsset>("Assets/EditorSamples/ScriptableAsset/ExampleAsset.asset");
            if (exampleAsset != null)
                Debug.Log("读取成功");
            else
                Debug.Log("Asset 不存在");
        }
    }
}
