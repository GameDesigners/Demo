using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditorSample
{
    [CustomPropertyDrawer(typeof(PropertyDrawerExample))]
    public class ExamplePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using(new EditorGUI.PropertyScope(position,label,property))
            {
                var minValueProperty = property.FindPropertyRelative("MinValue");
                var maxValueProperty = property.FindPropertyRelative("MaxValue");
                var minMaxSliderRect = new Rect(position)
                {
                    height = position.height * 0.5f,
                };

                var labelRect = new Rect(minMaxSliderRect)
                {
                    x = minMaxSliderRect.x + EditorGUIUtility.labelWidth,
                    y = minMaxSliderRect.y + minMaxSliderRect.height,
                };

                float minValue = minValueProperty.intValue;
                float maxValue = maxValueProperty.intValue;
                EditorGUI.BeginChangeCheck();
                EditorGUI.MinMaxSlider(minMaxSliderRect, label, ref minValue, ref maxValue, 0, 100);
                EditorGUI.LabelField(labelRect, minValue.ToString(), maxValue.ToString());
                if(EditorGUI.EndChangeCheck())
                {
                    minValueProperty.intValue=Mathf.FloorToInt(minValue);
                    maxValueProperty.intValue=Mathf.FloorToInt(maxValue);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 1.5f;
        }
    }
}
