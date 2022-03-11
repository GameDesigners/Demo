using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditorSample
{
    public class ParentScriptableObject : ScriptableObject
    {
        [SerializeField] ChildScriptableObject child;

        const string PATH = "Assets/EditorSamples/ScriptableAsset/New ParentScriptableObject.asset";

        [MenuItem("Help/EditorExample/Create ParentScriptableObject")]
        private static void CreateParentScriptableObject()
        {
            var parent = ScriptableObject.CreateInstance<ParentScriptableObject>();
            parent.child = ScriptableObject.CreateInstance<ChildScriptableObject>();
            parent.child.hideFlags = HideFlags.None;
            AssetDatabase.AddObjectToAsset(parent.child, PATH);
            AssetDatabase.CreateAsset(parent, PATH);
            AssetDatabase.ImportAsset(PATH);
        }
    }
}
