using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace UnityEditorSample
{
    public class GetBuildInAsset : MonoBehaviour
    {
        /// <summary>
        /// 获取内置的Asset名称
        /// </summary>
        //[RuntimeInitializeOnLoadMethod]
        private static void GetBuildinAssetNames()
        {
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var info = typeof(EditorGUIUtility).GetMethod("GetEditorAssetBundle", flags);
            var bundle = info.Invoke(null, new object[0]) as AssetBundle;

            foreach (var n in bundle.GetAllAssetNames())
                Debug.Log(n);
        }
    }
}