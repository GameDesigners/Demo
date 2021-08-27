using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.DataManager;

[Serializable]
public class Message
{
    public int num;
    public string name;
    public float f;
}

public class SerializerTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Message msg = new Message();
        msg.num = 1;
        msg.name = "hello world :)";
        msg.f = 1.0f;

        Debug.Log(XmlUtil.Serialize(msg));
        string filePath = Application.streamingAssetsPath + "/msg.xml";
        Debug.Log($"Path:{filePath}");
        XmlUtil.Serialize(msg, filePath);

        Message _fromfile = XmlUtil.DeserializeFromFile<Message>(filePath);
        Debug.Log($"num={_fromfile.num}  name={_fromfile.name}  f={_fromfile.f}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
