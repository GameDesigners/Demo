using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlHuman : BaseHuman
{
    new void Awake()
    {
        base.Awake();
    }

    new void Start()
    {
        base.Start();
    }

    new void Update()
    {
        base.Update();

        if(GInput.Instance.GetMouseButtonDown(MouseButton.LButton))
        {
            Ray ray = Camera.main.ScreenPointToRay(GInput.Instance.GetMousePosition());
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            if (hit.collider.tag == "Terrain")
            {
                MoveTo(hit.point);
                //NetManager.Send("Enter|192.168.3.42,100,200,300,45");
            }
        }
    }
}
