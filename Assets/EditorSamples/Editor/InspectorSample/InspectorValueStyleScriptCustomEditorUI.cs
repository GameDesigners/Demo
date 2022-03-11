using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditorSample
{
    [CanEditMultipleObjects]  //可同时编辑多个脚本参数（当使用自定义InspectorGUI时，需要用此修饰才能同时修改脚本参数）
    [CustomEditor(typeof(InspectorValueStyle))]
    public class InspectorValueStyleScriptCustomEditorUI : Editor
    {
        InspectorValueStyle instance = null;

        SerializedProperty num1Property;
        SerializedProperty propertyDrawerExampleProperty;

        PreviewRenderUtility previewRenderUtility;
        GameObject previewObject;

        private void OnEnable()
        {
            instance = (InspectorValueStyle)(this.target);
            num1Property = this.serializedObject.FindProperty("num1");
            propertyDrawerExampleProperty = this.serializedObject.FindProperty("propertyDrawerExample");


            //预览窗口的设置
            previewRenderUtility = new PreviewRenderUtility(true);
            previewRenderUtility.cameraFieldOfView = 30f;
            previewRenderUtility.camera.nearClipPlane = 0.3f;
            previewRenderUtility.camera.farClipPlane = 1000f;
            var component=(Component)target;
            previewObject = component.gameObject;
        }

        private void OnDisable()
        {
            if(previewRenderUtility!=null)
                previewRenderUtility.Cleanup();
        }

        /// <summary>
        /// Inspector窗口重写函数
        /// </summary>
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            GUILayout.Label("默认的Inspector样式");
            base.OnInspectorGUI();
            EditorGUILayout.Space(20);

            GUILayout.Label("自定义的Inspector样式");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.IntSlider(num1Property, 0, 100);
            var num2 = EditorGUILayout.IntSlider((int)instance.num2, 0, 100);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(instance, "Change Num2");
                instance.num2 = num2;
            }
            GUILayout.Label($"num1+num2+num3+num4={instance.num1 + instance.num2 + instance.num3 + instance.num4}");

            EditorGUILayout.PropertyField(propertyDrawerExampleProperty);
            this.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 预览窗口重写函数
        /// </summary>
        /// <returns></returns>
        public override bool HasPreviewGUI()
        {
            return true;
        }

        /// <summary>
        /// 重写预览窗口的名称
        /// </summary>
        /// <returns></returns>
        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("Custom Preview Window");
        }


        /// <summary>
        /// 用于在右上角的标题中添加GUI
        /// </summary>
        public override void OnPreviewSettings()
        {
            GUIStyle preLabel = new GUIStyle("preLabel");
            GUIStyle preButton = new GUIStyle("preButton");

            GUILayout.Label("Label", preLabel);
            GUILayout.Button("Button", preButton);
        }


        /// <summary>
        /// 预览窗口的GUI
        /// </summary>
        /// <param name="r"></param>
        /// <param name="background"></param>
        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            previewRenderUtility.BeginPreview(r, background);
            var previewCamera = previewRenderUtility.camera;
            previewCamera.transform.position = previewObject.transform.position + new Vector3(0f, 2.5f, -5f);
            previewCamera.transform.LookAt(previewObject.transform);
            previewCamera.Render();
            previewRenderUtility.EndAndDrawPreview(r);
        }
    }
}
