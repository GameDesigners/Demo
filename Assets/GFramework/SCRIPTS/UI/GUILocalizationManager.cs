using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUILocalizationManager
{
    private static GUILocalizationManager _instance;
    public static GUILocalizationManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GUILocalizationManager();
            return _instance;
        }
        private set { }
    }

    public string GetLocationValueByKey(string key) => default;
}
