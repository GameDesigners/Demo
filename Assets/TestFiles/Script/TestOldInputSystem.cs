using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOldInputSystem : MonoBehaviour
{
    
    private void OnGUI()
    {
        GUI.skin.label.fontSize = 20;
        GUILayout.Label($"LT:          {GInput.Instance.GetAxis(HandleAxis.LT)}");
        GUILayout.Label($"RT:          {GInput.Instance.GetAxis(HandleAxis.RT)}");
        GUILayout.Label($"LSVertical:  {GInput.Instance.GetAxis(HandleAxis.LSVertical)}");
        GUILayout.Label($"LSHorizontal:{GInput.Instance.GetAxis(HandleAxis.LSHorizontal)}");
        GUILayout.Label($"RSVertical:  {GInput.Instance.GetAxis(HandleAxis.RSVertical)}");
        GUILayout.Label($"RSHorizontal:{GInput.Instance.GetAxis(HandleAxis.RSHorizontal)}");
        GUILayout.Label($"Vertical:    {GInput.Instance.GetAxis(HandleAxis.Vertical)}");
        GUILayout.Label($"Horizontal:  {GInput.Instance.GetAxis(HandleAxis.Horizontal)}");
    }
}
