using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BehaviourTreeEditor : EditorWindow
{
    [MenuItem("Custom/BehaviourTreeEditor")]
    static void Init()
    {
        BehaviourTreeEditor window = GetWindow<BehaviourTreeEditor>();
    }
}
