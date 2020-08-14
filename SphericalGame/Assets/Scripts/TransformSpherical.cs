using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TransformSpherical : MonoBehaviour
{
    public TransformSpherical()
    {
        localToWorld = Rot4.identity;
    }

    public Rot4 localToWorld;
    public Rot4 worldToLocal
    {
        get => localToWorld.Inverse();
        set => localToWorld = value.Inverse();
    }

    void Update()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend)
        {
            rend.material.SetMatrix("_Model", localToWorld.Matrix());
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TransformSpherical))]
public class TransformSphericalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TransformSpherical trans = (TransformSpherical)target;
        Quaternion pos = trans.localToWorld * Quaternion.identity;
        EditorGUILayout.LabelField("Position:", pos.ToString());
        EditorGUILayout.LabelField("Relative Orientation:", (Rot4.StraightTo(pos).Inverse() * trans.localToWorld * new Vector4(0, 0, 1, 0)).ToString());
        trans.localToWorld = trans.localToWorld * Rot4.identity;
    }

    //public void Awake()
    //{
    //    EditorApplication.update += OnInspectorGUI;
    //}

    //public void OnDestroy()
    //{
    //    EditorApplication.update -= OnInspectorGUI;
    //}
}
#endif