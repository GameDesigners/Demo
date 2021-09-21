using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUI : MonoBehaviour
{
    UIBasePage basePage=default;
    // Start is called before the first frame update
    void Start()
    {
        List<List<string>> data;
        /*
        List<string> col1 = new List<string>();
        col1.Add("key");
        col1.Add("Chinese");
        col1.Add("English");
        data.Add(col1);

        List<string> col2 = new List<string>();
        col2.Add("@txt_xxx");
        col2.Add("你好，世界！");
        col2.Add("Hello,World!");
        data.Add(col2);

        ExcelUtil.Write(Application.streamingAssetsPath + "/hello.xlsx",data,"LanguageExcel");
        */

        /*
        data = ExcelUtil.Read(Application.streamingAssetsPath + "/hello.xlsx", "LanguageExcel");

        string log = "";
        foreach (var r in data)
        {
            foreach (var c in r)
                log += c + "\t";
            log += "\n";
        }
        GDebug.Instance.Log(log);
        */
    }

    // Update is called once per frame
    void Update()
    {
        if(GInput.Instance.GetKeyDown(KeyCode.M))
        {
            if(basePage==default)
                basePage = GUIManager.Instance.OpenUIPage("@ui_comp_page");
            else
            {
                if (GUIManager.Instance.CloseUIPage(basePage))
                    basePage = default;
                else
                    GDebug.Instance.Error("Something Wrong...");
            }
        }
    }
}
