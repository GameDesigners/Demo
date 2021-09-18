using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUI : MonoBehaviour
{
    UIBasePage basePage=default;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(GInput.Instance.GetKeyDown(KeyCode.M))
        {
            if(basePage==default)
                basePage = GUIManager.Instance.OpenUIPage("@gaming_page");
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
