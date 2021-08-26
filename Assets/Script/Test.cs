using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.DataManager;


public class Test : MonoBehaviour
{
    public GameObject templateObj;

    GameObjectPool pool;

    private void Start()
    {
        pool = new GameObjectPool(templateObj);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            pool.GetFreeElem().GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(0.0f,2.0f),-1, Random.Range(0.0f, 2.0f)),ForceMode.Impulse);
    }
}
