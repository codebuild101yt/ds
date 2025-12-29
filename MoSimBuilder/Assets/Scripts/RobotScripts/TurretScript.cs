using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class TurretScript : MonoBehaviour
{
    private GameObject target;
    [Tooltip("Hinge joint around which the turret rotates")]
    public HingeJoint joint;
    private JointMotor jointMotor;
    [Tooltip("Whether the turret can shoot while moving, dont forget to add to player inputs if it can")]
    public bool SWM;
    [Tooltip("the P value in the controll loop. (error * p) = output")]
    public float kP;
    [Tooltip("Max rotational velocity of the turret")]
    public float kMax;

    private Rigidbody rb;
    private DriveController controller;
    private bool onSWM;
    [Tooltip("max speed when using swm and added to playerInputs")]
    [SerializeField] private float maxSWM = 0.32f;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<DriveController>();
        rb = GetComponent<Rigidbody>();
        target = new GameObject();

        target.tag = "ignore";

        jointMotor = new JointMotor();

        jointMotor.force = 5000;

        joint.useMotor = true;

        target.name = "RotationTarget";
        target.transform.SetParent(GameObject.Find("Field").transform);
        target.transform.position = new Vector3(0, 0, 0);

        if (joint.connectedBody == null)
        {
            joint.connectedBody = joint.transform.parent.GetComponentInParent<Rigidbody>().transform.gameObject.GetComponent<Rigidbody>();
        }
        joint.axis = new Vector3(0, 1, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.canRobotMove)
        {
            Vector3 directionToTarget;
            if (SWM)
            {
                directionToTarget = (target.transform.position - (joint.transform.position + rb.velocity)).normalized;
            } else
            {
                directionToTarget = (target.transform.position - (joint.transform.position)).normalized;
            }
            float Target = Quaternion.LookRotation(directionToTarget).eulerAngles.y - (joint.transform.rotation.eulerAngles.y);

            if (Mathf.Abs(Target) > 180)
            {
                Target -= 360 * (Target / Mathf.Abs(Target));
            }

            float TargetRPM = Mathf.Clamp(Target * kP, -kMax, kMax);
            jointMotor.targetVelocity = TargetRPM;
            joint.motor = jointMotor;
        } else
        {
            jointMotor.targetVelocity = 0;
            joint.motor = jointMotor;
        }

        if (onSWM)
        {
            controller.velocityMp = maxSWM;
        } else
        {
            controller.velocityMp = 1;
        }
    }

    public void OnSWM(InputAction.CallbackContext ctx)
    {
        onSWM = ctx.action.triggered;
    }
}
