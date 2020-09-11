using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axes : MonoBehaviour
{
    void Start()
    {
        Renderer[] rs = GetComponentsInChildren<Renderer>();
        rs[0].material.color = new Color(0.2f, 0.2f, 0.2f); // 1
        rs[1].material.color = Color.red; // i
        rs[2].material.color = Color.yellow; // j
        rs[3].material.color = Color.blue; // k
        rs[4].material.color = Color.magenta; // -1
        rs[5].material.color = new Color(1f, 0.5f, 0f, 1f); // -i
        rs[6].material.color = Color.white; // -j
        rs[7].material.color = Color.green; // -k
        rs[0].GetComponent<TransformSpherical>().localToWorld = Rot4.StraightTo(new Quaternion(0, 0, 0, 1));
        rs[1].GetComponent<TransformSpherical>().localToWorld = Rot4.StraightTo(new Quaternion(1, 0, 0, 0));
        rs[2].GetComponent<TransformSpherical>().localToWorld = Rot4.StraightTo(new Quaternion(0, 1, 0, 0));
        rs[3].GetComponent<TransformSpherical>().localToWorld = Rot4.StraightTo(new Quaternion(0, 0, 1, 0));
        rs[4].GetComponent<TransformSpherical>().localToWorld = new Rot4(new R4(0, 0, 0, -1), new R4(0, 0, 0, 1));
        rs[5].GetComponent<TransformSpherical>().localToWorld = Rot4.StraightTo(new Quaternion(-1, 0, 0, 0));
        rs[6].GetComponent<TransformSpherical>().localToWorld = Rot4.StraightTo(new Quaternion(0, -1, 0, 0));
        rs[7].GetComponent<TransformSpherical>().localToWorld = Rot4.StraightTo(new Quaternion(0, 0, -1, 0));
    }
}
