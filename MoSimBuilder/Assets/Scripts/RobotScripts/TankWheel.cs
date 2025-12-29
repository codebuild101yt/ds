using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankWheel : MonoBehaviour
{
    private WheelCollider m_WheelCollider;
    // Start is called before the first frame update
    void Start()
    {
        m_WheelCollider = GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        m_WheelCollider.steerAngle = 0;
        m_WheelCollider.brakeTorque = 0;
        m_WheelCollider.motorTorque = 0.000000000000000000000000000001f;
    }
}