using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TestLog : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GDebug.Instance.Log("测试Log系统...");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
