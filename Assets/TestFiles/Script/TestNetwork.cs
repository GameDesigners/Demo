using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class TestNetwork : MonoBehaviour
{
    Socket socket;
    // Start is called before the first frame update
    void Start()
    {
        Timer timer = new Timer(SleepTimeout, null, 5000, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SleepTimeout(System.Object state)
    {
        Debug.Log("Finish Timer");
    }
}
