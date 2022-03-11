using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditorSample
{
    public class WindowsSample3_IHasCustomMenu : EditorWindow, IHasCustomMenu
    {
        [MenuItem("Help/EditorExample/Windows/WindowsSamples 2 (IHashCustomMenuSample)")]
        private static void Open()
        {
            GetWindow<WindowsSample3_IHasCustomMenu>();
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("example"), false, () =>
            {
                Debug.Log("Click Custom Menu Item <example>");
            });

            menu.AddItem(new GUIContent("example2"), false, () =>
            {
                Debug.Log("Click Custom Menu Item <example2>");
            });
        }
    }
}
