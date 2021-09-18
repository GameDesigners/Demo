using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TestCortoutines : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(NextFrame());
        //StartCoroutine(WaitForFrames(5));
        //StartCoroutine(WaitForSeconds(5));
        //StartCoroutine(WaitForEndOfFrame());
        //StartCoroutine(WaitForFixedUpdate());

        StartCoroutine(WaitForIsDone());
        Debug.Log("是否执行...");
    }

    private IEnumerator NextFrame()
    {
        yield return null;
        //or yield return 0;
        Debug.Log("下一帧执行于此");
    }

    private IEnumerator WaitForFrames(float waits)
    {
        yield return waits;
        Debug.Log($"等待{waits}帧后执行此函数");
    }

    private IEnumerator WaitForSeconds(float waits)
    {
        yield return new WaitForSeconds(waits);
        Debug.Log($"等待{waits}秒后执行此函数");
    }

    private IEnumerator WaitForEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("在所有的渲染以及GUI程序执行完成后从当前位置继续执行");
    }

    private IEnumerator WaitForFixedUpdate()
    {
        yield return new WaitForFixedUpdate();
        Debug.Log("所有脚本中的FixedUpdate()函数都被执行后从当前位置继续执行");
    }

    private IEnumerator WaitForWWW()
    {
        UnityWebRequest www=new UnityWebRequest("url");
        yield return www;
        Debug.Log("等待一个网络请求完成后从当前位置继续执行");

    }

    private IEnumerator WaitForAnotherCoroutineFinish()
    {
        yield return StartCoroutine(WaitForWWW());
        Debug.Log(":等待一个协程执行完成后从当前位置继续执行");
    }

    private IEnumerator Break()
    {
        Debug.Log("如果使用yield break语句，将会导致协程的执行条件不被满足，不会从当前的位置继续执行程序，而是直接从当前位置跳出函数体，回到函数的根部");
        yield break;
    }

    private IEnumerator WaitForIsDone()
    {
        yield return false;
    }
}
