using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    /// <summary>
    /// 角色预制体
    /// </summary>
    public GameObject humanPrefab;
    
    /// <summary>
    /// 本客户端的人物组件
    /// </summary>
    public BaseHuman myHuman;

    public Dictionary<string, BaseHuman> otherHumans = new Dictionary<string, BaseHuman>();

    private void Start()
    {
        NetManager.AddListener("Enter", OnEnter);
        NetManager.AddListener("List", OnList);
        NetManager.AddListener("Move", OnMove);
        NetManager.AddListener("Attack", OnAttack);
        NetManager.AddListener("Leave", OnLeave);
        NetManager.Connect("192.168.3.42", 8888);

        GameObject obj = GameObject.Instantiate(humanPrefab);
        float x = Random.Range(-5, 5f);
        float z = Random.Range(-5, 5f);
        obj.transform.position = new Vector3(x, 0, z);
        myHuman = obj.AddComponent<CtrlHuman>();
        myHuman.desc = NetManager.GetDesc();

        //发送协议
        Vector3 pos = myHuman.transform.position;
        Vector3 eul = myHuman.transform.eulerAngles;
        string sendStr = "Enter|";
        sendStr += NetManager.GetDesc() + ",";
        sendStr += pos.x + ",";
        sendStr += pos.y + ",";
        sendStr += pos.z + ",";
        sendStr += eul.y + ",";
        sendStr += "100";
        NetManager.Send(sendStr);
    }

    private void Update()
    {
        NetManager.Update();
    }

    private void OnEnter(string msgArgs)
    {
        GDebug.Instance.Log($"[OnEnter]  {msgArgs}");

        //解析参数
        string[] split = msgArgs.Split(',');
        string desc = split[0];
        float x = float.Parse(split[1]);
        float y = float.Parse(split[2]);
        float z = float.Parse(split[3]);
        float eulY = float.Parse(split[4]);
        int hp = int.Parse(split[5]);

        if (desc == NetManager.GetDesc())
        {
            //请求玩家列表
            NetManager.Send("List|");
            return;
        }

        GameObject obj = GameObject.Instantiate(humanPrefab);
        obj.transform.position = new Vector3(x, y, z);
        obj.transform.eulerAngles = new Vector3(0, eulY, 0);
        BaseHuman h = obj.AddComponent<SyncHuman>();
        h.desc = desc;
        Debug.Log($"desc:{desc}");
        otherHumans.Add(desc, h);
    }

    private void OnList(string msgArgs)
    {
        GDebug.Instance.Log($"[OnList]  {msgArgs}");

        //解析参数
        string[] split = msgArgs.Split(',');
        int count = (split.Length - 1) / 6;
        for (int i = 0; i < count; i++)
        {
            string desc = split[i * 6 + 0];
            float x = float.Parse(split[i * 6 + 1]);
            float y = float.Parse(split[i * 6 + 2]);
            float z = float.Parse(split[i * 6 + 3]);
            float eulY = float.Parse(split[i * 6 + 4]);
            int hp = int.Parse(split[i * 6 + 5]);

            //是自己
            if (desc == NetManager.GetDesc())
                continue;

            if (otherHumans.ContainsKey(desc))
            {
                GameObject human = otherHumans[desc].gameObject;
                human.transform.position = new Vector3(x, y, z);
                human.transform.eulerAngles = new Vector3(0, eulY, 0);
            }
            else
            {
                GameObject obj = GameObject.Instantiate(humanPrefab);
                obj.transform.position = new Vector3(x, y, z);
                obj.transform.eulerAngles = new Vector3(0, eulY, 0);
                BaseHuman h = obj.AddComponent<SyncHuman>();
                h.desc = desc;
                otherHumans.Add(desc, h);
            }
        }
    }

    private void OnMove(string msgArgs)
    {
        GDebug.Instance.Log($"[OnMove]  {msgArgs}");

        //解析参数
        string[] split = msgArgs.Split(',');
        string desc = split[0];
        float x = float.Parse(split[1]);
        float y = float.Parse(split[2]);
        float z = float.Parse(split[3]);

        //移动
        if (!otherHumans.ContainsKey(desc))
            return;

        BaseHuman h = otherHumans[desc];
        Vector3 targetPos = new Vector3(x, y, z);
        h.MoveTo(targetPos);
    }

    private void OnAttack(string msgArgs)
    {
        GDebug.Instance.Log($"[OnAttack]  {msgArgs}");

        //解析参数
        string[] split = msgArgs.Split(',');
        string desc = split[0];
        float eulY = float.Parse(split[1]);

        //攻击动作
        if (!otherHumans.ContainsKey(desc))
            return;
        SyncHuman h = otherHumans[desc] as SyncHuman;
        h.SyncAttack(eulY);
    }

    private void OnLeave(string msgArgs)
    {
        GDebug.Instance.Log($"[OnLeave]  {msgArgs}");
        string[] split = msgArgs.Split(',');
        string desc = split[0];

        //删除角色
        if (!otherHumans.ContainsKey(desc))
            return;

        BaseHuman h = otherHumans[desc];
        Destroy(h.gameObject);
        otherHumans.Remove(desc);
    }
}
