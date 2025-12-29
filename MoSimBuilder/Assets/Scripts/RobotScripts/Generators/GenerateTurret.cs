using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[ExecuteAlways]
public class GenerateTurret : MonoBehaviour
{
    private GameObject _turretDisk;
    private GameObject _turretModel;
    private Rigidbody _rb;
    private HingeJoint _hj;
//start visible section

    [Header("Arm Informatoin")]
    
    [SerializeField] private Units units;

    [SerializeField] private float turretDiameter = 6;
    [SerializeField] private float turretHeight = 1;
    [SerializeField] private float turretWeight = 3;
    
    [Header("Setpoint Settings")]
    
    [SerializeField] private ControlType controlType;
    
    [SerializeField] private float[] setPoints;
    
    [SerializeField] private Buttons[] setPointButtons;
    
    [SerializeField] private float stowAngle = 0;
    
    [Tooltip("leave 0,0 for no limits.")]
    [SerializeField] private Vector2 limits;
    
    [Header("turret target")]
    
    [SerializeField] private bool continuousAim;

    [SerializeField] private Vector3 target;

    [SerializeField] private float angleOffset;
    
    
    // end visible section
    
    private Buttons[] _setPointButtonsBuffer;
    private float[] _setPointBuffer;
    
    private GameObject _mainRobot;
    private Rigidbody _robotRb;
    
    private PlayerInput _playerInput;
    
    private InputActionMap _inputMap;

    private float _activeTarget;
    
    
    
    private int _setpointSequence;

    private bool _sequenceDebounce;

    private JointMotor _jm;

    private float _position;
    
    private Buttons _lastButton;

    private float _multiplier;

    
    
    void Start()
    {
        Startup();
    }

    private void Awake()
    {
        Startup();
    }
    // Update is called once per frame
    void Update()
    {
        _multiplier = units switch
        {
            Units.inch => 0.0254f,
            Units.meter => 1,
            Units.centimerter => 0.01f,
            Units.millimeter => 0.001f,
            _ => 0.0254f
        };
        
        if (!EditorApplication.isPlaying)
        {
            if (_mainRobot == null)
            {
                _mainRobot = transform.root.gameObject;
            }

            if (_robotRb == null)
            {
                _robotRb = _mainRobot.GetComponent<Rigidbody>();
            }
            
            if (_turretDisk == null)
            {
                _turretDisk = new GameObject("TurretDisk");
                _turretDisk.transform.parent = transform;
                _turretDisk.transform.localPosition = Vector3.zero;
                _turretDisk.layer = LayerMask.NameToLayer("Robot");
            }

            if (_rb == null)
            {
                _rb = _turretDisk.AddComponent<Rigidbody>();
                _rb.interpolation = RigidbodyInterpolation.Interpolate;
                _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                _rb.excludeLayers = LayerMask.GetMask("Robot");
                _rb.useGravity = true;
                _rb.drag = 0;
            }

            if (_hj == null)
            {
                var t = transform;
                while (t.GetComponent<Rigidbody>() == null)
                {
                    t = t.parent.transform;
                }

                if (t.GetComponent<Rigidbody>() != null)
                {
                    _hj = _turretDisk.AddComponent<HingeJoint>();
                    _hj.axis = new Vector3(0, 1, 0);
                    _hj.useMotor = true;
                    _hj.connectedBody = t.GetComponent<Rigidbody>();
                }
            }
            
            if (limits.x != 0 && limits.y != 0)
            {
                var limit = new JointLimits();
                limit.min = Mathf.Repeat(-limits.x, 360);
                limit.max = Mathf.Repeat(-limits.y, 360);
                _hj.limits = limit;
                _hj.useLimits = true;
            }
            else
            {
                _hj.useLimits = false;
            }
            
            _rb.mass = turretWeight;

            if (_turretModel == null)
            {
                _turretModel = GameObject.CreatePrimitive(PrimitiveType.Cylinder); 
                DestroyImmediate(_turretModel.GetComponent<CapsuleCollider>());
                _turretModel.name = "TurretModel";
                _turretModel.transform.parent = _turretDisk.transform;
                _turretModel.layer = LayerMask.NameToLayer("Robot");
            }
            
            _turretModel.transform.localScale = new Vector3(turretDiameter*_multiplier, (turretHeight/2)*_multiplier, turretDiameter*_multiplier);
            _turretModel.transform.localPosition = new Vector3(0, 0, 0);

            if (setPointButtons != null)
            {
                if (setPointButtons.Length != setPoints.Length)
                {
                    _setPointButtonsBuffer = setPointButtons;

                    setPointButtons = new Buttons[setPoints.Length];
                    for (int i = 0; i < _setPointButtonsBuffer.Length; i++)
                    {
                        if (i < setPoints.Length && i < _setPointButtonsBuffer.Length)
                        {
                            setPointButtons[i] = _setPointButtonsBuffer[i];
                        }
                    }
                }
            }
            else
            {
                setPoints = new float[1];
                setPointButtons = new Buttons[setPoints.Length];
            }
        }
        else
        {
            
            
            for (var i = 0; i < setPoints.Length; i++)
            {
                switch (controlType)
                {
                    case ControlType.toggle:
                    {
                        if (_inputMap.FindAction(setPointButtons[i].ToString()).triggered)
                        {
                            if (Mathf.Approximately(_activeTarget, -setPoints[i]))
                            {
                                _activeTarget = -stowAngle;
                            }
                            else
                            {
                                _activeTarget = -setPoints[i];
                            }
                        }

                        break;
                    }
                    case ControlType.hold:
                    {
                        if (_inputMap.FindAction(setPointButtons[i].ToString()).IsPressed())
                        {
                            _activeTarget = -setPoints[i];
                        }
                        else if (i > 0 )
                        {
                            for (var t = 0; t < setPoints.Length; t++)
                            {
                                if (_inputMap.FindAction(setPointButtons[t].ToString()).IsPressed())
                                {
                                    _activeTarget = -setPoints[t];
                                }
                            }
                        }
                        else
                        {
                            _activeTarget = -stowAngle;
                        }

                        break;
                    }
                    case ControlType.sequence:
                    {
                        if (_inputMap.FindAction(setPointButtons[i].ToString()).triggered && _sequenceDebounce == false)
                        {
                            if (_setpointSequence < setPoints.Length)
                            {
                                if (_lastButton != setPointButtons[i])
                                {
                                    _setpointSequence = 0;
                                    _lastButton = setPointButtons[i];
                                }
                            
                                while (setPointButtons[i] != setPointButtons[_setpointSequence])
                                {
                                    _setpointSequence ++;

                                    if (_setpointSequence+1 > setPoints.Length)
                                    {
                                        _setpointSequence = 0;
                                        _activeTarget = -stowAngle;
                                        return;
                                    }


                                }
                                _activeTarget = -setPoints[_setpointSequence];

                                _setpointSequence += 1;
                            }
                            else
                            {
                                _setpointSequence = 0;
                                _activeTarget = -stowAngle;
                            }
                        
                            _lastButton = setPointButtons[i];
                        }
                        else
                        {

                        }
                    
                        if (_inputMap.FindAction(setPointButtons[i].ToString()).IsPressed())
                        {
                            _sequenceDebounce = true;
                        }
                        else
                        {
                            _sequenceDebounce = false;
                        }

                        break;
                    }
                    case ControlType.lastPressed:
                    {
                        if (_inputMap.FindAction(setPointButtons[i].ToString()).triggered)
                        {
                                _activeTarget = -setPoints[i];
                        }

                        break;
                    }
                }
            }


            if (continuousAim && Mathf.Approximately(_activeTarget, -stowAngle))
            {
                Vector3 aimingPoint;
                
                    aimingPoint = _turretDisk.transform.position;
                
                Quaternion targetShooterRotation;
                targetShooterRotation = Quaternion.LookRotation(target-aimingPoint, Vector3.up);
                float Target = targetShooterRotation.eulerAngles.y - transform.eulerAngles.y;
                if (Target >= 360)
                {
                    Target -= 360;
                }

                if (Target < 0)
                {
                    Target += 360;
                }
                
                _position = Quaternion.Angle(_turretDisk.transform.rotation, transform.rotation);

                if (_turretDisk.transform.localRotation.eulerAngles.y > 180)
                {
                    _position = -_position;
                }
                if (_position < 0)
                {
                    _position += 360;
                }

                _position += angleOffset;
                
                if (_position >= 360)
                {
                    _position -= 360;
                }

                if (_position < 0)
                {
                    _position += 360;
                }
                
                _position = Mathf.Repeat(_position, 360);
                
                float positionError = (Target - _position);

                if (Mathf.Abs(positionError) > 180)
                {
                    positionError = -1 * positionError;
                }
                _jm.force = 90000000000;
                _jm.targetVelocity = Mathf.Clamp(positionError * 8f, -360,360);
                _hj.useMotor = true;
                _hj.useSpring = false;
                _hj.motor = _jm;
            }
            else
            {
                float Target = _activeTarget;
                if (Target >= 360)
                {
                    Target -= 360;
                }

                if (Target < 0)
                {
                    Target += 360;
                }

                _position = Quaternion.Angle(_turretDisk.transform.rotation, transform.rotation);

                if (_turretDisk.transform.localRotation.eulerAngles.y > 180)
                {
                    _position = -_position;
                }
                if (_position < 0)
                {
                    _position += 360;
                }

                _position = Mathf.Repeat(_position, 360);
                
                

                float positionError = (Target - _position);

                if (_position > Mathf.Repeat(-limits.x, 360) && _position+positionError < Mathf.Repeat(-limits.x, 360) && Mathf.Abs(positionError) < 180 && limits.x != 0)
                {
                    positionError = -1 * positionError;
                } else if (_position < Mathf.Repeat(-limits.x, 360) && _position+positionError > Mathf.Repeat(-limits.x, 360) && Mathf.Abs(positionError) < 180 && limits.x != 0)
                {
                    positionError = -1 * positionError;
                } else if (_position > Mathf.Repeat(-limits.y, 360) && _position+positionError < Mathf.Repeat(-limits.y, 360) && Mathf.Abs(positionError) < 180 && limits.y != 0)
                {
                    positionError = -1 * positionError;
                } else if (_position < Mathf.Repeat(-limits.y, 360) && _position+positionError > Mathf.Repeat(-limits.y, 360) && Mathf.Abs(positionError) < 180 && limits.y != 0)
                {
                    positionError = -1 * positionError;
                } else if (Mathf.Abs(positionError) > 180)
                {
                    positionError = -1 * positionError;
                }
                
                _jm.force = 90000000000000;
                _jm.targetVelocity = Mathf.Clamp((positionError * 7f), -360,360);
                _hj.useMotor = true;
                _hj.useSpring = false;
                _hj.motor = _jm;
               
            }

        }
    }

    private void Startup()
    {
        _activeTarget = -stowAngle;
        
        _setpointSequence = 0;
        
        if (_playerInput == null)
        {
            var t = transform;
            while (t.GetComponent<PlayerInput>() == null)
            {
                t = t.parent.transform;
            }
            
            if (t.GetComponent<PlayerInput>() != null)
            {
                _playerInput = t.GetComponent<PlayerInput>();

                _inputMap = _playerInput.currentActionMap;
            }
        }

        if (transform.Find("TurretDisk"))
        {
            _turretDisk = transform.Find("TurretDisk").gameObject;

            if (_turretDisk.transform.Find("TurretModel"))
            {
                _turretModel = _turretDisk.transform.Find("TurretModel").gameObject;
            }

            if (_turretDisk.GetComponent<Rigidbody>())
            {
                _rb = _turretDisk.GetComponent<Rigidbody>();
            }

            if (_turretDisk.GetComponent<HingeJoint>())
            {
                _hj = _turretDisk.GetComponent <HingeJoint>();
            }
        }
    }
}
