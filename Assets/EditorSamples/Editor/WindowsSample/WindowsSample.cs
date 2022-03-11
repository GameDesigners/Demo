using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine.Events;

namespace UnityEditorSample
{
    /// <summary>
    /// 范围控制样式
    /// </summary>
    public class BackgroundColorScope : GUI.Scope
    {
        private readonly Color color;

        public BackgroundColorScope(Color color)
        {
            this.color = GUI.backgroundColor;
            GUI.backgroundColor = color;
        }

        protected override void CloseScope()
        {
            GUI.backgroundColor = color;
        }
    }

    /*
     *   EditorWindow继承自ScriptableObject
     */
    public class WindowsSample : EditorWindow
    {
        //若不允许开发者打开多个此窗口，可利用单例模式
        private static EditorWindow window;

        private static Rect buttonRect = new Rect(100, 100, 300, 100);
        private static Vector2 windowSize = new Vector2(300, 100);



        [MenuItem("Help/EditorExample/Windows/WindowsSamples", false, 1)]
        private static void Open()
        {
            //方法1：直接打开Windows（无需下面的操作）
            //GetWindow<WindowsSample1>();


            //方法2：可设置更多内容
            if (window == null)
                window = CreateInstance<WindowsSample>();


            window.Show();           //普通的显示
                                     //window.ShowUtility();    //始终显示在编辑器窗口的前台（不能被其他窗口覆盖）
                                     //window.ShowPopup();      //没有窗口标题，也没有关闭按钮，不能移动，只能自己实现关闭窗口的过程
                                     //window.ShowAuxWindow();    //和ShowUtility一样，但是如果窗口失焦后，窗口将会被删除
                                     //window.ShowAsDropDown(buttonRect, windowSize);


            //设置Window的最大尺寸
            window.maxSize = window.minSize = new Vector2(800, 600);

            //设置window标题和图标
            var icon1 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/EditorSamples/Editor Default Resources/@unity_icon.png");
            var icon2 = EditorGUIUtility.Load("@unity_icon.png") as Texture2D;
            window.titleContent = new GUIContent("编辑器窗口示例", icon1);

            //搜索所有当前加载的对象的特定对象
            var window_byfind = Resources.FindObjectsOfTypeAll<WindowsSample>();
        }

        private void OnGUI()
        {
            EditorGUISample();
            PupopSample();
        }

        #region PupopContent Sample
        ExamplePupupContent popupContent = new ExamplePupupContent();
        private void PupopSample()
        {
            if (Event.current.keyCode == KeyCode.Escape)
                window.Close();

            if (GUILayout.Button("PopupContent", GUILayout.Width(128)))
            {
                var activatorRect = GUILayoutUtility.GetLastRect();
                PopupWindow.Show(activatorRect, popupContent);
            }
        }
        #endregion

        #region EditorGUI Sample
        private string str1 = "hello world!";

        private Texture2D texture1;
        private AnimFloat animFloat = new AnimFloat(0.0001f);

        private float[] numbers = new float[] { 0, 1, 2 };
        private GUIContent[] contents = new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z") };

        private float angle;

        private bool toggleOn;
        private bool one;
        private bool two;
        private bool three;
        private int selected;

        private void EditorGUISample()
        {
            EditorGUI.BeginChangeCheck();

            //MultiFloatField
            EditorGUI.MultiFloatField(new Rect(0, 0, 200, EditorGUIUtility.singleLineHeight), new GUIContent("Position"), contents, numbers);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight + 30);
            EditorGUILayout.LabelField("Example Label");


            //DisableGroup
            EditorGUI.BeginDisabledGroup(true);
            str1 = EditorGUILayout.TextField("str1", str1);
            EditorGUI.EndDisabledGroup();


            //FadeGroup
            var options = new[] { GUILayout.Width(128), GUILayout.Height(128) };
            bool on = animFloat.value == 1;
            if (GUILayout.Button(on ? "隐藏" : "显示", GUILayout.Width(64)))
            {
                animFloat.target = on ? 0.0001f : 1f;
                animFloat.speed = 5f;

                var env = new UnityEvent();
                env.AddListener(() => Repaint());
                animFloat.valueChanged = env;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginFadeGroup(animFloat.value);
            texture1 = EditorGUILayout.ObjectField(texture1, typeof(Texture2D), false, options) as Texture2D;
            EditorGUILayout.EndFadeGroup();
            texture1 = EditorGUILayout.ObjectField(texture1, typeof(Texture2D), false, options) as Texture2D;
            EditorGUILayout.EndHorizontal();


            //Knob
            angle = EditorGUILayout.Knob(Vector2.one * 64, angle, 0, 360, "度", Color.gray, Color.red, true);


            //Scope
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Button("Button1-HorizontalScope");
                GUILayout.Button("Button2-HorizontalScope");
            }

            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Button("Button1-VerticalScope");
                GUILayout.Button("Button2-VerticalScope");
            }

            using (new BackgroundColorScope(Color.green))
            {
                GUILayout.Button("Button1-Custom-BackgroundColorScope-Green");
            }

            //Button Style
            toggleOn = GUILayout.Toggle(toggleOn, toggleOn ? "on" : "off", "button");

            using (new EditorGUILayout.HorizontalScope())
            {
                one = GUILayout.Toggle(one, "1", EditorStyles.miniButtonLeft);
                two = GUILayout.Toggle(two, "2", EditorStyles.miniButtonMid);
                three = GUILayout.Toggle(three, "3", EditorStyles.miniButtonRight);
            }

            EditorGUILayout.Space(15);
            selected = GUILayout.Toolbar(selected, new string[] { "1", "2", "3" });
            selected = GUILayout.Toolbar(selected, new string[] { "1", "2", "3" }, EditorStyles.toolbarButton);
            selected = GUILayout.SelectionGrid(selected, new string[] { "1", "2", "3" }, 1, "PreferencesKeysElement");

            if (EditorGUI.EndChangeCheck())
            {
                //范围中的GUI发生了变化（数值、状态等）
            }
        }
        #endregion
    }

    public class ExamplePupupContent : PopupWindowContent
    {
        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.LabelField("Label");
        }

        public override void OnOpen()
        {
            Debug.Log("ExamplePupupContent呼出");
        }

        public override void OnClose()
        {
            Debug.Log("ExamplePupupContent关闭");
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 100);
        }
    }
}
