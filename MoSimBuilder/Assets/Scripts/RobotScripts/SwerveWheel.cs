using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SwerveWheel : MonoBehaviour
{
    // Start is called before the first frame update
    private WheelCollider m_WheelCollider;
    public float Wa;
    private JointSpring suspensionSpring;

    private WheelFrictionCurve forwardFriction;
    private WheelFrictionCurve sidewaysFriction;
    // Start is called before the first frame update

    private void Awake()
    {
        suspensionSpring = new JointSpring();
        
        suspensionSpring.spring = 80000;
        suspensionSpring.damper = 4000;

        forwardFriction = new WheelFrictionCurve();

        forwardFriction.stiffness = 1;
        forwardFriction.asymptoteSlip = 0.5f;
        forwardFriction.asymptoteValue = 1;
        forwardFriction.extremumSlip = 0.6f;
        forwardFriction.extremumValue = 1;
        
        sidewaysFriction = new WheelFrictionCurve();

        sidewaysFriction.stiffness = 1;
        sidewaysFriction.asymptoteSlip = 0.5f;
        sidewaysFriction.asymptoteValue = 0.75f;
        sidewaysFriction.extremumSlip = 0.6f;
        sidewaysFriction.extremumValue = 0.75f;
        
        m_WheelCollider = GetComponent<WheelCollider>();

        m_WheelCollider.suspensionDistance = 0f;
        m_WheelCollider.wheelDampingRate = 1;
        m_WheelCollider.suspensionSpring = suspensionSpring;
        m_WheelCollider.forwardFriction = forwardFriction;
        m_WheelCollider.sidewaysFriction = sidewaysFriction;
        m_WheelCollider.mass = 1f;
    }

    void Start()
    {
        suspensionSpring = new JointSpring();
        
        suspensionSpring.spring = 80000;
        suspensionSpring.damper = 4000;

        forwardFriction = new WheelFrictionCurve();

        forwardFriction.stiffness = 1;
        forwardFriction.asymptoteSlip = 0.5f;
        forwardFriction.asymptoteValue = 1;
        forwardFriction.extremumSlip = 0.6f;
        forwardFriction.extremumValue = 1;
        
        sidewaysFriction = new WheelFrictionCurve();

        sidewaysFriction.stiffness = 2;
        sidewaysFriction.asymptoteSlip = 0.5f;
        sidewaysFriction.asymptoteValue = 0.75f;
        sidewaysFriction.extremumSlip = 0.6f;
        sidewaysFriction.extremumValue = 0.75f;
        
        m_WheelCollider = GetComponent<WheelCollider>();

        m_WheelCollider.suspensionDistance = 0f;
        m_WheelCollider.wheelDampingRate = 1;
        m_WheelCollider.suspensionSpring = suspensionSpring;
        m_WheelCollider.forwardFriction = forwardFriction;
        m_WheelCollider.sidewaysFriction = sidewaysFriction;
        m_WheelCollider.mass = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Wa < 0)
        {
            Wa = -Wa + 180;
        }

        if (Wa < 0)
        {
            Wa = -Wa + 180;
        }

        if (Wa > 360)
        {
            Wa -= 360;
        }

        if (Wa > 360)
        {
            Wa -= 360;
        }

        m_WheelCollider.steerAngle = Wa;
        m_WheelCollider.brakeTorque = 0;
        m_WheelCollider.motorTorque = 0.000000000000000000000000000001f;
    }
}