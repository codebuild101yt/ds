using UnityEngine;
using Cinemachine;
using Unity.Mathematics;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private Alliance alliance;
    [SerializeField] private bool isSecondaryCam;

    public bool robotCentric;
    private Transform _target;
    private CinemachineVirtualCamera _vcam;
    private Vector3 startVector;
    private quaternion startRotation;

    private void Start()
    {
        startVector = transform.position;
        startRotation = transform.rotation;

        _vcam = GetComponent<CinemachineVirtualCamera>();
        _target = GetEnabledTarget();

        if (robotCentric)
        {
            transform.SetParent(_target);
        }
        else
        {
            _vcam.Follow = _target;
            _vcam.LookAt = _target;
        }
    }

    private Transform GetEnabledTarget()
    {
        if (alliance == Alliance.Blue)
        {
            return isSecondaryCam ? GameObject.FindGameObjectWithTag("Player2").transform : GameObject.FindGameObjectWithTag("Player").transform;
        }

        return isSecondaryCam ? GameObject.FindGameObjectWithTag("RedPlayer2").transform : GameObject.FindGameObjectWithTag("RedPlayer").transform;
    }

    public void Reset()
    {
        transform.position = startVector;
        transform.rotation = startRotation;

        _vcam = GetComponent<CinemachineVirtualCamera>();
        _target = GetEnabledTarget();

        if (robotCentric)
        {
            transform.SetParent(_target);
        }
        else
        {
            _vcam.Follow = _target;
            _vcam.LookAt = _target;
        }
    }
}