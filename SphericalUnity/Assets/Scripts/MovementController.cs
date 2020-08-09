using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    private TransformSpherical trans;

    void Start()
    {
        trans = GetComponent<TransformSpherical>();
    }

    void FixedUpdate()
    {
        Vector3 move = new Vector3(-Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 turn = new Vector3(Input.GetAxis("Mouse Y"), -Input.GetAxis("Mouse X"), 0) * 5;
        //Vector3 turn = Vector3.zero;
        trans.localToWorld = trans.localToWorld * Rot4.FromTangent(move, turn, Time.fixedDeltaTime);
    }
}
