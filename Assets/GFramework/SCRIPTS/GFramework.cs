using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.DataManager;
namespace Framework
{
    public class GFramework
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Main()
        {
            GDebug.Instance.Log("GFramework启动...");
        }
    }
}
