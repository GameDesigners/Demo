using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditorSample
{
    [System.Serializable]
    public class PropertyDrawerExample
    {
        public int MaxValue;
        public int MinValue;
    }

    [DisallowMultipleComponent]                                     //禁止同一个GameObject添加两个此组件
    [AddComponentMenu("CustomComponents/Add InspectorValueStyle")]  //在菜单栏的Component中添加一个菜单按钮
    public class InspectorValueStyle : MonoBehaviour
    {
        [Header("数值范围Style")]
        [Range(1, 10)] public int num1;
        [Range(1, 10)] public float num2;
        [Range(1, 10)] public long num3;
        [Range(1, 10)] public double num4;


        [Header("字符串Style")]
        [Multiline(5)] public string multiline;
        [TextArea(3, 5)] public string textArea;


        //脚本菜单
        [ContextMenu("RandomNumbers")]
        public void RandomNumbers()
        {
            number = Random.Range(100, 200);
        }



        [Header("属性右键菜单")]
        [ContextMenuItem("Random", "RandomNumber")]
        [ContextMenuItem("Reset", "ResetNumber")]
        public int number;
        private void RandomNumber()
        {
            number = Random.Range(0, 100);
        }
        private void ResetNumber()
        {
            number = 0;
        }


        [Header("颜色面板")]
        public Color color1;
        [ColorUsage(false), Tooltip("ColorUsage(false)")] public Color color2;
        [ColorUsage(true, true), Tooltip("ColorUsage(true,true)")] public Color color3;

        public PropertyDrawerExample propertyDrawerExample;
    }
}
