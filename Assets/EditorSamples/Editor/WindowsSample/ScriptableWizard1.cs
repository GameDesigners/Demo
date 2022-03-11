using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityEditorSample
{
    public class ScriptableWizard1 : ScriptableWizard
    {
        public string gameObjectName;

        [MenuItem("Help/EditorExample/Windows/ScriptableWizard 1")]
        private static void Open()
        {
            DisplayWizard<ScriptableWizard1>("Example Wizard 1", "创建", "查找");
        }

        /// <summary>
        /// 按下Create 按钮时调用的方法 
        /// </summary>
        private void OnWizardCreate()
        {
            new GameObject(gameObjectName);
        }

        /// <summary>
        /// 按下除Create外的另一个按钮时调用的方法
        /// </summary>
        private void OnWizardOtherButton()
        {
            var gameObject = GameObject.Find(gameObjectName);
            if (gameObject == null)
            {
                Debug.Log("要查找的游戏对象不存在");
            }
            else
                Selection.activeGameObject = gameObject;
        }

        /// <summary>
        /// 当所有字段值发生更改时调用的方法
        /// </summary>
        private void OnWizardUpdate()
        {
            Debug.Log("您修改了属性");
        }

        /// <summary>
        /// 自定义GUI　
        /// </summary>
        /// <returns></returns>
        protected override bool DrawWizardGUI()
        {
            EditorGUILayout.LabelField("Label");
            return true;
        }
    }
}
