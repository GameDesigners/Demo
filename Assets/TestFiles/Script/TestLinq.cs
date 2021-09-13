using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.DataManager;
using System;
using System.Linq;
using System.Xml.Linq;

[System.Serializable]
public class Player : IFormattable,IComparable<Player>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Score { get; set; }
    public float FinishedTime { get; set; }

    private List<string> props;
    public List<string> Props
    {
        get
        {
            if (props == null)
                props = new List<string>();
            return props;

        }
        set
        {
            props = value;
        }
    }

    public Player() { }
    public Player(int _id,string _name,int _score,float _finishTime)
    {
        Id = _id;
        Name = _name;
        Score = _score;
        FinishedTime = _finishTime;
    }

    public string ToString(string format,IFormatProvider formatProvider)
    {
        switch(format)
        {
            case "ID":
                return $"{Id}";
            case "N":
                return $"{Name}";
            case "S":
                return $"{Score}";
            case "FT":
                return $"{FinishedTime}";
            default:
                return $"{Id} {string.Format("{0,-10}", Name)} {string.Format("{0,-6}", Score)} {string.Format("{0,-16}", FinishedTime)}";
        }
    }

    public int CompareTo(Player other) => Id.CompareTo(other?.Id);
}

public static class MyExtensions
{
    /// <summary>
    /// 拓展方法
    /// </summary>
    /// <param name="p"></param>
    /// <param name="standard"></param>
    public static bool IsHaveProp(this Player p, string propsStr)
    {
        var query = from s in p.Props
                    where s == propsStr
                    select s;
        return query.ToList().Count != 0;
    }
}

public class TestLinq : MonoBehaviour
{
    [Header("道具列表")] public List<string> props;
    private List<Player> players = new List<Player>();
    private string xmlFilePath;


    private void Start()
    {
        xmlFilePath = Application.streamingAssetsPath + "/players.xml";
        //GetRandomData(100);
        players = XmlUtil.DeserializeFromFile<List<Player>>(xmlFilePath);

        LinqQueryExample_1();
        LinqQueryExample_XmlQuery();
        
        LinqOps_Fliter();
        Linq_FliterByExtensionMethod();
        Linq_FilterByType();




        //players[0].IsHaveProp("飓风之刃");
    }

    private void DebugPlayerList(List<Player> list,string caption="")
    {
        string liststr = $"{caption}\n";
        foreach (Player p in list)
            liststr += $"{p}\n";
        Debug.Log(liststr);
    }


    /// <summary>
    /// LINQ例子：Linq To Object
    /// </summary>
    private void LinqQueryExample_1()
    {
        var query = from r in players
                    where r.Score >= 60
                    orderby r.Score descending
                    select r;
        DebugPlayerList(query.ToList());
    }  

    /// <summary>
    /// LINQ例子：Linq To Xml 
    /// </summary>
    private void LinqQueryExample_XmlQuery()
    {
        XElement xmlDoc = XElement.Load(xmlFilePath);
        var query = from r in xmlDoc.Elements("Player")
                    where int.Parse(r.Element("Score").Value) >= 60
                    orderby int.Parse(r.Element("Score").Value) descending
                    select r;
        string res = "";
        foreach(var v in query)
        {
            res += v.Element("Name").Value+"\n";
        }
        Debug.Log(res);
    }


    #region LINQ Ops Examples
    private void LinqOps_Fliter()
    {
        var res = from p in players
                  where (p.Score >= 60 && p.FinishedTime <= 60)
                  orderby p.Score descending, p.FinishedTime ascending
                  select p;

        DebugPlayerList(res.ToList(), "及格且在60s内完成的玩家");
    }

    private void Linq_FliterByExtensionMethod()
    {
        var res = players.Where((r, index) => r.Score >= 60 && r.FinishedTime <= 60 && index < 30).OrderBy(r => r.FinishedTime).OrderByDescending(r => r.Score);
        DebugPlayerList(res.ToList(), "前三十位玩家中，及格且在60s内完成的玩家");
    }

    private void Linq_FilterByType()
    {
        object[] data = { "one", 2, "2.5str", 3, 4.5f, "five", 'c' };
        var res = data.OfType<string>();
        string str = "object数组在的属于字符串类型的有:\n";
        foreach (var s in res)
            str += s + "\n";
        Debug.Log(str);

    }

    private void Linq_ComplexFrom()
    {
        //var res=from 
    }




    #endregion




















    private string RandomString()
    {
        string res = "Xiao";
        string charTable = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = 0; i < 3; i++)
        {
            res += charTable.Substring(UnityEngine.Random.Range(0, charTable.Length - 1), 1);
        }
        return res;
    }

    private void GetRandomData(int num)
    {
        List<int> alreadRegisterId = new List<int>();
        for (int i = 0; i < num; i++)
        {
            int randomId;
            do
            {
                randomId = UnityEngine.Random.Range(1000000, 9999999);
            }
            while (alreadRegisterId.Contains(randomId));

            Player p = new Player(randomId, RandomString(), UnityEngine.Random.Range(0, 100), UnityEngine.Random.Range(0.0f, 100.0f));
            int propsNum = UnityEngine.Random.Range(0, props.Count - 1);
            for(int index=0;index<propsNum;)
            {
                int propsIndex= UnityEngine.Random.Range(0, props.Count - 1);
                string propsStr = props[propsIndex];
                if(!p.Props.Contains(propsStr))
                {
                    p.Props.Add(propsStr);
                    index++;
                }
            }
            players.Add(p);
        }

        XmlUtil.Serialize(players,xmlFilePath);
    }
}




