using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToTarget : MonoBehaviour
{
    private GameObject target;
    public float kP;
    public float kMax;
    private DriveController driveController;
    // Start is called before the first frame update
    void Start()
    {
        target = new GameObject();

        target.tag = "ignore";

        driveController = GetComponent<DriveController>();

        target.name = "RotationTarget";
        target.transform.SetParent(GameObject.Find("Field").transform);
        target.transform.position = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.canRobotMove)
        {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized; ;
            float Target = Quaternion.LookRotation(directionToTarget).eulerAngles.y - (transform.rotation.eulerAngles.y);

            if (Mathf.Abs(Target) > 180)
            {
                Target -= 360 * (Target / Mathf.Abs(Target));
            }

            driveController.validVision = true;
            driveController.targetOffset = Target;
            driveController.visionMultiplyer = kP;
            driveController.visionMax = kMax;
        }
    }
}
