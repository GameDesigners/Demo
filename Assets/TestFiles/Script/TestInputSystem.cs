using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CurrDevice
{
    public Gamepad pad;
    public Keyboard keyboard;
    public Mouse mouse;
}

[RequireComponent(typeof(PlayerInput))]
public class TestInputSystem : MonoBehaviour
{

    public InputAction walkAction;

    private void Awake()
    {
        walkAction = new InputAction(binding: "*/{leftStick}");
        walkAction.performed += WalkAction_performed;
    }

    private void WalkAction_performed(InputAction.CallbackContext obj)
    {
        Debug.Log("Walk...");
    }

    private void OnEnable()
    {
        walkAction.Enable();
    }

    private void OnDisable()
    {
        walkAction.Disable();
    }

    private void Start()
    {
        FindAllConnectedGamePads();
        OnDeviceChangeEventRegister();
        CreateASimpleFireAction();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            Debug.Log("按下了K键");

        if (GetCurrentUsingGamePad().keyboard.oKey.wasPressedThisFrame)
            Debug.Log("按下了O键");
    }

    private void FixedUpdate()
    {
        var gamePad = Gamepad.current;

        if (gamePad != null) 
        {
            if (gamePad.rightTrigger.wasPressedThisFrame)
            {
                //'Use' code here
            }
            Vector2 move = gamePad.leftStick.ReadDefaultValue();
            //'Move' code here
        }

        


        var keyBoard = Keyboard.current;
        if(keyBoard!=null)
        {
            if (keyBoard.aKey.wasPressedThisFrame)
            {
                Debug.Log("点击了A键");
            }

            if (keyBoard.aKey.wasReleasedThisFrame)
            {
                Debug.Log("释放了A键");
            }

            if(keyBoard.pKey.isPressed)
            {
                Debug.Log("按下P键");
            }


        }
        else
        {
            Debug.Log("键盘设备为空");
        }
    }

    public void MoveAction(InputAction.CallbackContext context)
    {
        Debug.Log("响应了Move");
    }

    public void FireAction(InputAction.CallbackContext context)
    {
        Debug.Log("响应了FireAction");
    }


    private void FindAllConnectedGamePads()
    {
        bool useapi = false;
        string res = "当前连接的GamePads列表:\n";

        if (useapi)
        {
            var allGamePads = Gamepad.all;
            foreach (var pad in allGamePads)
                res += $"{pad}\n";
        }
        else
        {
            var allGamePads = from pad in InputSystem.devices
                              where pad is Gamepad
                              select pad;
            foreach (var pad in allGamePads)
                res += $"{pad}\n";
        }
        Debug.Log(res);
    }

    private CurrDevice GetCurrentUsingGamePad()
    {
        CurrDevice currDevice = new CurrDevice();
        currDevice.pad = Gamepad.current;
        currDevice.keyboard = Keyboard.current;
        currDevice.mouse = Mouse.current;
        return currDevice;
    }

    private void OnDeviceChangeEventRegister()
    {
        InputSystem.onDeviceChange+=(device,change)=>
        {
            switch(change)
            {
                case InputDeviceChange.Added:
                    Debug.Log($"添加了新设备【{device.name}】");
                    break;
                case InputDeviceChange.Disconnected:
                    Debug.Log($"设备【{device.name}】断开连接（设备被拔掉）");
                    break;
                case InputDeviceChange.Removed:
                    Debug.Log($"设备【{device.name}】完全从输入系统中移除");
                    break;
            }
        };
    }

    private void FireEvent() { Debug.Log("Fire Event..."); }
    private void CreateASimpleFireAction()
    {
        var action = new InputAction(binding: "*/{primaryAction}");
        action.performed += _ => FireEvent();
        action.Enable();
    }

}
