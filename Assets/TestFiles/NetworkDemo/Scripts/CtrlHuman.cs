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

        //移动
        if(GInput.Instance.GetMouseButtonDown(MouseButton.LButton))
        {
            Ray ray = Camera.main.ScreenPointToRay(GInput.Instance.GetMousePosition());
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            if (hit.collider.tag == "Terrain")
            {
                MoveTo(hit.point);

                //发送协议
                string sendStr = $"Move|{NetManager.GetDesc()},{hit.point.x},{hit.point.y},{hit.point.z},";
                NetManager.Send(sendStr);
            }
        }

        //攻击
        if(GInput.Instance.GetMouseButtonDown(MouseButton.RButton))
        {
            if (isAttacking) return;
            if (isMoving) return;

            Ray ray = Camera.main.ScreenPointToRay(GInput.Instance.GetMousePosition());
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            transform.LookAt(hit.point);
            Attack();

            //发送协议
            string sendStr = $"Attack|{NetManager.GetDesc()},{transform.eulerAngles.y},";
            NetManager.Send(sendStr);
        }
    }
}
