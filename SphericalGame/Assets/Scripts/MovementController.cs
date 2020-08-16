using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    private TransformSpherical trans;

    private float nextAction;
    public float actionRate = 0.2f;

    void Start()
    {
        trans = GetComponent<TransformSpherical>();
        nextAction = 0;
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time > nextAction)
        {
            nextAction = Time.time + actionRate;
        }
    }

    void FixedUpdate()
    {
        Vector3 move = Vector3.ClampMagnitude(new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("OtherVertical"), -Input.GetAxis("Vertical")), 1f);
        Vector3 turn = new Vector3(15 * Input.GetAxis("Mouse Y"), -15 * Input.GetAxis("Mouse X"), -3 * Input.GetAxis("Spin"));
        trans.localToWorld = trans.localToWorld * Rot4.FromTangent(move, turn, Time.fixedDeltaTime);
    }
}
