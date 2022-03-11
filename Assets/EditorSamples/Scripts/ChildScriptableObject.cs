using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditorSample
{
    public class ChildScriptableObject : ScriptableObject
    {
        [SerializeField] string str;

        public ChildScriptableObject()
        {
            //name = "New ChildScriptableObject";
        }
    }

}