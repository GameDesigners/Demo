using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Text))]
public class GUILocalization : MonoBehaviour
{
    [DisplayOnly] public string hashCode;
    [DisplayOnly] public string key;

    private void Start()
    {
        if (key == null)
            key = gameObject.name;


        Text comp = GetComponent<Text>();
        if (comp != null && key != null)
            comp.text = GUILocalizationManager.Instance.GetLocationValueByKey(key);
    }
}
