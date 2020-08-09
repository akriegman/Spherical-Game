using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSpherical : MonoBehaviour
{
    void OnPreRender()
    {
        TransformSpherical trans = GetComponent<TransformSpherical>();
        Shader.SetGlobalMatrix("_View", trans.worldToLocal.Matrix());
    }
}
