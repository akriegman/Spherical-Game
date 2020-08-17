using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    private TransformSpherical trans;

    private float nextAction;
    public float actionRate = 0.2f;

    public Solid solidType;

    void Start()
    {
        trans = GetComponent<TransformSpherical>();
        nextAction = 0;
        solidType = Solid.White;
    }

    void Update()
    {
        if ( (Input.GetMouseButton(0) || Input.GetMouseButton(1)) && Time.time > nextAction)
        {
            nextAction = Time.time + actionRate;
            RaycastHitSpherical hit;
            if (PhysicsSpherical.Raycast(trans.lookRay, out hit, Mathf.PI))
            {
                Polytope poly = hit.collider.GetComponent<Polytope>();
                if (poly != null)
                {
                    poly.Clicked(Input.GetMouseButton(0), Input.GetMouseButton(1), hit.triangleIndex, solidType);
                }
            }
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKey(KeyCode.Alpha1))
        {
            solidType = (Solid)0;
        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            solidType = (Solid)1;
        }

        if (Input.GetKey(KeyCode.Alpha3))
        {
            solidType = (Solid)2;
        }

        if (Input.GetKey(KeyCode.Alpha4))
        {
            solidType = (Solid)3;
        }
    }

    void FixedUpdate()
    {
        Vector3 move = Vector3.ClampMagnitude(new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("OtherVertical"), -Input.GetAxis("Vertical")), 1f);
        Vector3 turn = new Vector3(15 * Input.GetAxis("Mouse Y"), -15 * Input.GetAxis("Mouse X"), 3 * Input.GetAxis("Spin"));
        trans.localToWorld = trans.localToWorld * Rot4.FromTangent(move, turn, Time.fixedDeltaTime);
    }
}
