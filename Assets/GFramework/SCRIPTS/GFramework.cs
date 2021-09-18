using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.DataManager;
namespace Framework
{
    public class GFramework
    {
        private static string log = "初始化过程：\n";
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static async void Main()
        {
            GDebug.Instance.Log("GFramework启动...");
            
            log += await GUIManager.Instance.Initialize() ? "UIManager初始化成功...\n" : "UIManager初始化失败...\n";
            log += GInput.Instance.Initialize() ? "GInput初始化成功...\n" : "GInput初始化失败...\n";



            GDebug.Instance.Log(log);
            if (log.Contains("失败"))
                QuitFramework();
        }

        static void QuitFramework()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
