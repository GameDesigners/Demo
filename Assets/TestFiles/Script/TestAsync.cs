using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TestAsync : MonoBehaviour
{
    public Text text;
    private Button[] btns;
    private string result="点击上方按钮测试同步和异步的区别...";
    private CancellationTokenSource cst = new CancellationTokenSource();

    private void Start()
    {
        btns = GetComponentsInChildren<Button>();
        if (btns.Length >= 4)
        {
            btns[0].onClick.AddListener(() =>
            {
                result = $"Call with Sync Function--> {Greeting("XiaoMing")}";
                result += "\n同步模式的调用会照成主线程的堵塞...(动画不播放)";
                text.text = result;
            });

            btns[1].onClick.AddListener(() =>
            {
                Task<string> t1 = GreetingAsync("XiaoPeng");
                t1.ContinueWith(t =>
                {
                    result = $"Call with Sync Function--> {t.Result}";
                    result += "\nTask have been finished...";
                    result += "\n主线程依旧在运行...";
                });
            });

            btns[2].onClick.AddListener(() =>
            {
                Task<string[]> t = MultiGreetingAsync(new List<string> { "XiaoMing", "XiaoWang", "XiaoFang", "XiaoYan" });
                t.ContinueWith(res =>
                {
                    result = "调用了执行多个Task的函数：\n";
                    foreach(var str in res.Result)
                    {
                        result += str + "\n";
                    }
                });
            });

            btns[3].onClick.AddListener(() =>
            {
                cst.Cancel();
                GDebug.Instance.Log("取消人物");
            });
        }
    }

    private void Update()
    {
        text.text = result;
    }

    private string Greeting(string name)
    {
        Task.Delay(3000).Wait();
        return $"hello,{name}";
    }

    private Task<string> GreetingAsync(string name)
    {
        return Task.Run(() =>
        {
            return Greeting(name);
        }, cst.Token);
    }

    private Task<string[]> MultiGreetingAsync(List<string> names)
    {
        List<Task<string>> tasks = new List<Task<string>>();
        foreach (var n in names)
            tasks.Add(GreetingAsync(n));
        IEnumerable<Task<string>> taskEnumerables = tasks.AsEnumerable();
        return Task.WhenAll(taskEnumerables);
    }
}
