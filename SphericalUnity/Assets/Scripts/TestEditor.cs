using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(TestBehaviour))]
public class TestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TestBehaviour behaver = target as TestBehaviour;
        if (GUILayout.Button("Test"))
        {
            behaver.Test();
        }
    }
}
#endif