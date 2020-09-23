using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSpherical : MonoBehaviour
{
    void OnPreRender()
    {
        TransformSpherical trans = GetComponent<TransformSpherical>();
        Shader.SetGlobalMatrix("_View", trans.worldToLocal.Matrix());

        Camera cam = Camera.main;
        // Note that z_far is negative!
        Shader.SetGlobalMatrix("_Projection", Matrix4x4.Perspective(cam.fieldOfView, cam.aspect, 0.05f, -0.05f));
    }
}
