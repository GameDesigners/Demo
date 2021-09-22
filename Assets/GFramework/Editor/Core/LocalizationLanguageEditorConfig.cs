using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class LocalizationLanguageEditorConfig
{
    /// <summary>
    /// 本地化Excel文件路径信息
    /// </summary>
    public string languageLocalizationExcelFilePath;

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
    private Dictionary<string, int> language_index;

    public LocalizationLanguageEditorConfig()
    {
        gameObjectPathsInProject = new List<string>();
        languageList = new List<string>();
        language_index = new Dictionary<string, int>();
    }

    public void UpdateLanguageIndexDic()
    {
        language_index.Clear();
        for (int index = 0; index < languageList.Count; index++)
            language_index.Add(languageList[index], index);
    }

    public int GetLanguageIndex(string language)
    {
        if (language_index.ContainsKey(language))
            return language_index[language];
        else
            return -1;
    }

}
