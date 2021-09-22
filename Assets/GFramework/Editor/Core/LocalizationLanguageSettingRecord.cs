using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class LocalizationLanguageEditorConfig
{
    /// <summary>
    /// UI游戏对象路径列表
    /// </summary>
    public List<string> gameObjectPathsInProject;
    
    /// <summary>
    /// 语言列表
    /// </summary>
    public List<string> languageList;

    /// <summary>
    /// 语言列表中的索引
    /// </summary>
    public Dictionary<string, int> language_index;

    public LocalizationLanguageEditorConfig()
    {
        gameObjectPathsInProject = new List<string>();
        languageList = new List<string>();
        language_index = new Dictionary<string, int>();
    }

}
