using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class GenerateDriveTrain : MonoBehaviour
{
    private GameObject _frontRail;
    private GameObject _backRail;
    private GameObject _leftRail;
    private GameObject _rightRail;
    private GameObject _driveTrain;
    private GameObject _frame;

    private GameObject _wheelChild;
    private GameObject _lfWheel;
    private GameObject _rfWheel;
    private GameObject _rrWheel;
    private GameObject _lrWheel;

    private GameObject _raycastChild;
    private GameObject _lf;
    private GameObject _rf;
    private GameObject _rr;
    private GameObject _lr;
    
    private Rigidbody _rb;
    
    private float _multiplier;

    private float _velMp;

    private float _maxMp;
    
    [HideInInspector] public InputActionAsset inputAsset;
    
    private DriveController _driveController;
    
    private PlayerInput _playerInput;
    
//begin visible stack
    [Header("Drive Settings")]
    
    [SerializeField] private SpeedUnit speedUnits;
    
    [Tooltip("Max Drive Free Speed in SpeedUnits/s")]
    [SerializeField] private float driveSpeed = 16;

    [FormerlySerializedAs("driveForce")]
    [Tooltip("max acceleration in SpeedUnits/s/s")]
    [SerializeField] private float driveAcceleration = 18;

    [Header("Frame Information")] [SerializeField]
    private TubeType frameTubing;
    
    [SerializeField] private Units units;

    [Tooltip("Frame Width in Units")]
    [SerializeField] private float driveTrainWidth = 28;

    [Tooltip("Frame Length in Units")] [SerializeField]
    private float driveTrainLength = 28;
    
    [Tooltip("Frame Weight in lbs")]
    [SerializeField] private float drivetrainWeight = 65;
    
    [Header("Wheel Information")]
    
    [Tooltip("Wheel Width in Units")]
    [SerializeField] private float wheelWidth = 1.5f;
    
    [Tooltip("wheel Diameter in Units")]
    [SerializeField] private float wheelDiameter = 4;

    [Header("Bumper Settings")]

    [Tooltip("Height of the bumper relative to the center of the frame in Units")]
    [SerializeField] private float bumperHeight = 2.5f;

    [FormerlySerializedAs("bumperGap")]
    [Tooltip("frame gap for bumper X:Front,Y:Right,Z:Left,W:Rear")]
    [SerializeField] private Vector4 bumperFrameOffset;
  //end visible stack  
    

    [HideInInspector] public GameObject bumperPart;
    
    private GameObject[] _bumperParts = new GameObject[4];

    private GameObject _bumper;
    
    private GameObject[] _bumpRend = new GameObject[4];

    private float _frameHeight;
    
    private float _frameWidth;
    
    
    // Start is called before the first frame update

    void Awake()
    {
       Startup();
    }

    private void Start()
    {
        Startup();
    }

    // Update is called once per frame
    void Update()
    {
        if (EditorApplication.isPlaying) return;
        //create frame model
        
        _velMp = speedUnits switch
        {
            SpeedUnit.M => (1/0.3048f)*(1/0.3048f),
            SpeedUnit.ft => 1*(1/0.3048f),
            _ => 1*(1/0.3048f)
        };
        
        _maxMp = speedUnits switch
        {
            SpeedUnit.M => 1/0.3048f,
            SpeedUnit.ft => 1,
            _ => 1
        };
        
        _multiplier = units switch
        {
            Units.inch => 0.0254f,
            Units.meter => 1,
            Units.centimerter => 0.01f,
            Units.millimeter => 0.001f,
            _ => 0.0254f
        };
        
        _frameHeight = frameTubing switch
        {
            TubeType.oneXTwo => 2,
            TubeType.oneXOne => 1,
            TubeType.oneXThree => 3,
            TubeType.twoXTwo => 2,
            _ => 2
        };
        
        _frameWidth = frameTubing switch
        {
            TubeType.oneXTwo => 1,
            TubeType.oneXOne => 1,
            TubeType.oneXThree => 1,
            TubeType.twoXTwo => 2,
            _ => 2
        };
        
        if (_driveTrain == null)
        {
            _driveTrain = new GameObject("DriveTrain");
            _driveTrain.transform.parent = transform;
            _driveTrain.layer = LayerMask.NameToLayer("Robot");
        }

        if (_frame == null)
        {
            _frame = new GameObject("Frame");
            _frame.transform.parent = _driveTrain.transform;
            _frame.layer = LayerMask.NameToLayer("Robot");
        }

        if (_frontRail == null)
        {
            _frontRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _frontRail.name = "FrontRail";
            _frontRail.transform.parent = _frame.transform;
            _frontRail.layer = LayerMask.NameToLayer("Robot");
        }

        if (_backRail == null)
        {
            _backRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _backRail.name = "BackRail";
            _backRail.transform.parent = _frame.transform;
            _backRail.layer = LayerMask.NameToLayer("Robot");
        }

        if (_leftRail == null)
        {
            _leftRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _leftRail.name = "LeftRail";
            _leftRail.transform.parent = _frame.transform;
            _leftRail.layer = LayerMask.NameToLayer("Robot");
        }

        if (_rightRail == null)
        {
            _rightRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _rightRail.name = "RightRail";
            _rightRail.transform.parent = _frame.transform;
            _rightRail.layer = LayerMask.NameToLayer("Robot");
        }

        _frontRail.transform.localPosition = new Vector3(0, (2-_frameHeight) * 0.0254f *0.5f,((driveTrainLength * _multiplier)-(_frameWidth*0.0254f)) / (2.0f));
        _backRail.transform.localPosition = new Vector3(0, (2-_frameHeight) * 0.0254f * 0.5f, -((driveTrainLength * _multiplier)-(_frameWidth*0.0254f)) / (2.0f));
        _leftRail.transform.localPosition = new Vector3(-((driveTrainWidth * _multiplier)-(_frameWidth*0.0254f)) / (2.0f), (2-_frameHeight) * 0.0254f * 0.5f, 0);
        _rightRail.transform.localPosition = new Vector3(((driveTrainWidth * _multiplier)-(_frameWidth*0.0254f)) / (2.0f), (2-_frameHeight) * 0.0254f * 0.5f, 0);

        _frontRail.transform.localScale = new Vector3(driveTrainWidth * _multiplier, 0.0254f * _frameHeight, 0.0254f * _frameWidth);
        _backRail.transform.localScale = new Vector3(driveTrainWidth * _multiplier, 0.0254f * _frameHeight, 0.0254f * _frameWidth);
        _leftRail.transform.localScale = new Vector3(0.0254f * _frameWidth, 0.0254f * _frameHeight, driveTrainLength * _multiplier);
        _rightRail.transform.localScale = new Vector3(0.0254f * _frameWidth, 0.0254f * _frameHeight, driveTrainLength * _multiplier);

        //create wheels
        if (_wheelChild == null)
        {
            _wheelChild = new GameObject("Wheels");
            _wheelChild.transform.parent = _driveTrain.transform;
        }

        WheelCollider leftFrontWheel;
        if (_lfWheel == null)
        {
            _lfWheel = new GameObject();
            _lfWheel.name = "Lf";
            _lfWheel.transform.parent = _wheelChild.transform;
            leftFrontWheel = _lfWheel.AddComponent<WheelCollider>();
            _lfWheel.AddComponent<SwerveWheel>();
            _lfWheel.layer = LayerMask.NameToLayer("Robot");
        }
        else
        {
            leftFrontWheel = _lfWheel.GetComponent<WheelCollider>();
        }

        _lfWheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
        _lfWheel.transform.localScale =
            new Vector3(wheelDiameter * _multiplier, wheelWidth / 2 * _multiplier, wheelDiameter * _multiplier);
        _lfWheel.transform.localPosition =
            new Vector3(-driveTrainWidth / 2 * _multiplier + wheelDiameter / 2 * _multiplier + 1.5f * _multiplier, -_multiplier * 1,
                driveTrainLength / 2 * _multiplier - wheelDiameter / 2 * _multiplier - 1.5f * _multiplier);
        leftFrontWheel.radius = wheelDiameter / wheelWidth;

        WheelCollider rightFrontWheel;
        if (_rfWheel == null)
        {
            _rfWheel = new GameObject();
            _rfWheel.name = "Rf";
            _rfWheel.transform.parent = _wheelChild.transform;
            rightFrontWheel = _rfWheel.AddComponent<WheelCollider>();
            _rfWheel.AddComponent<SwerveWheel>();
            _rfWheel.layer = LayerMask.NameToLayer("Robot");
        }
        else
        {
            rightFrontWheel = _rfWheel.GetComponent<WheelCollider>();
        }

        _rfWheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
        _rfWheel.transform.localScale =
            new Vector3(wheelDiameter * _multiplier, wheelWidth / 2 * _multiplier, wheelDiameter * _multiplier);
        _rfWheel.transform.localPosition =
            new Vector3(driveTrainWidth / 2 * _multiplier - wheelDiameter / 2 * _multiplier - 1.5f * _multiplier, -_multiplier * 1,
                driveTrainLength / 2 * _multiplier - wheelDiameter / 2 * _multiplier - 1.5f * _multiplier);
        rightFrontWheel.radius = wheelDiameter / wheelWidth;

        WheelCollider leftRearWheel;
        if (_lrWheel == null)
        {
            _lrWheel = new GameObject();
            _lrWheel.name = "Lr";
            _lrWheel.transform.parent = _wheelChild.transform;
            leftRearWheel = _lrWheel.AddComponent<WheelCollider>();
            _lrWheel.AddComponent<SwerveWheel>();
            _lrWheel.layer = LayerMask.NameToLayer("Robot");
        }
        else
        {
            leftRearWheel = _lrWheel.GetComponent<WheelCollider>();
        }

        _lrWheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
        _lrWheel.transform.localScale =
            new Vector3(wheelDiameter * _multiplier, wheelWidth / 2 * _multiplier, wheelDiameter * _multiplier);
        _lrWheel.transform.localPosition =
            new Vector3(-driveTrainWidth / 2 * _multiplier + wheelDiameter / 2 * _multiplier + 1.5f * _multiplier, -_multiplier * 1,
                -driveTrainLength / 2 * _multiplier + wheelDiameter / 2 * _multiplier + 1.5f * _multiplier);
        leftRearWheel.radius = wheelDiameter / wheelWidth;

        WheelCollider rightRearWheel;
        if (_rrWheel == null)
        {
            _rrWheel = new GameObject();
            _rrWheel.name = "Rr";
            _rrWheel.transform.parent = _wheelChild.transform;
            rightRearWheel = _rrWheel.AddComponent<WheelCollider>();
            _rrWheel.AddComponent<SwerveWheel>();
            _rrWheel.layer = LayerMask.NameToLayer("Robot");
        }
        else
        {
            rightRearWheel = _rrWheel.GetComponent<WheelCollider>();
        }

        _rrWheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
        _rrWheel.transform.localScale =
            new Vector3(wheelDiameter * _multiplier, wheelWidth / 2 * _multiplier, wheelDiameter * _multiplier);
        _rrWheel.transform.localPosition =
            new Vector3(driveTrainWidth / 2 * _multiplier - wheelDiameter / 2 * _multiplier - 1.5f * _multiplier, -_multiplier * 1,
                -driveTrainLength / 2 * _multiplier + wheelDiameter / 2 * _multiplier + 1.5f * _multiplier);
        rightRearWheel.radius = wheelDiameter / wheelWidth;

        //create raycastObjects
        if (_raycastChild == null)
        {
            _raycastChild = new GameObject("Raycast");
            _raycastChild.transform.parent = _driveTrain.transform;
            _raycastChild.layer = LayerMask.NameToLayer("Robot");
        }

        
        if (_lf == null)
        {
            _lf = new GameObject("Lf");
            _lf.transform.parent = _raycastChild.transform;
            var lfModel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            DestroyImmediate(lfModel.GetComponent<CapsuleCollider>());
            lfModel.name = "LfModel";
            lfModel.transform.parent = _lf.transform;
            lfModel.transform.localPosition = new Vector3(0, 0, 0);
            lfModel.transform.localRotation = Quaternion.Euler(0, 0, 90);
            _lf.layer = LayerMask.NameToLayer("Robot");
        }

        _lf.transform.localPosition = new Vector3(
            -driveTrainWidth / 2 * _multiplier + wheelDiameter / 2 * _multiplier + 1.5f * _multiplier,
            -_multiplier * 1 + (-_multiplier * wheelDiameter / 2) + (wheelDiameter * _multiplier * 0.1f),
            driveTrainLength / 2 * _multiplier - wheelDiameter / 2 * _multiplier - 1.5f * _multiplier);
        _lf.transform.localScale = new Vector3(wheelWidth / 2 * _multiplier, wheelDiameter * _multiplier, wheelDiameter * _multiplier);
        _lf.transform.Find("LfModel").transform.localPosition = new Vector3(0,
            (_multiplier * 1 - (_multiplier * -wheelDiameter / 2) - (wheelDiameter * _multiplier * 0.1f) -_multiplier * 1)/(wheelDiameter * _multiplier), 0);

        if (_rf == null)
        {
            _rf = new GameObject("Rf");
            _rf.transform.parent = _raycastChild.transform;
            var rfModel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            DestroyImmediate(rfModel.GetComponent<CapsuleCollider>());
            rfModel.name = "RfModel";
            rfModel.transform.parent = _rf.transform;
            rfModel.transform.localPosition = new Vector3(0, 0, 0);
            rfModel.transform.localRotation = Quaternion.Euler(0, 0, 90);
            _rf.layer = LayerMask.NameToLayer("Robot");
        }

        _rf.transform.localPosition = new Vector3(
            driveTrainWidth / 2 * _multiplier - wheelDiameter / 2 * _multiplier - 1.5f * _multiplier,
            -_multiplier * 1 + (-_multiplier * wheelDiameter / 2) + (wheelDiameter * _multiplier * 0.1f),
            driveTrainLength / 2 * _multiplier - wheelDiameter / 2 * _multiplier - 1.5f * _multiplier);
        _rf.transform.localScale = new Vector3(wheelWidth / 2 * _multiplier, wheelDiameter * _multiplier, wheelDiameter * _multiplier);
        _rf.transform.Find("RfModel").transform.localPosition = new Vector3(0,
            (_multiplier * 1 - (-_multiplier * wheelDiameter / 2) - (wheelDiameter * _multiplier * 0.1f) -_multiplier * 1)/(wheelDiameter * _multiplier), 0);

        if (_lr == null)
        {
            _lr = new GameObject("Lr");
            _lr.transform.parent = _raycastChild.transform;
            var lrModel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            DestroyImmediate(lrModel.GetComponent<CapsuleCollider>());
            lrModel.name = "LrModel";
            lrModel.transform.parent = _lr.transform;
            lrModel.transform.localPosition = new Vector3(0, 0, 0);
            lrModel.transform.localRotation = Quaternion.Euler(0, 0, 90);
            _lr.layer = LayerMask.NameToLayer("Robot");
        }

        _lr.transform.localPosition = new Vector3(
            -driveTrainWidth / 2 * _multiplier + wheelDiameter / 2 * _multiplier + 1.5f * _multiplier,
            -_multiplier * 1 + (-_multiplier * wheelDiameter / 2) + (wheelDiameter * _multiplier * 0.1f),
            -driveTrainLength / 2 * _multiplier + wheelDiameter / 2 * _multiplier + 1.5f * _multiplier);
        _lr.transform.localScale = new Vector3(wheelWidth / 2 * _multiplier, wheelDiameter * _multiplier, wheelDiameter * _multiplier);
        _lr.transform.Find("LrModel").transform.localPosition = new Vector3(0,
            (_multiplier * 1 - (-_multiplier * wheelDiameter / 2) - (wheelDiameter * _multiplier * 0.1f) -_multiplier * 1)/(wheelDiameter * _multiplier), 0);

        if (_rr == null)
        {
            _rr = new GameObject("Rr");
            _rr.transform.parent = _raycastChild.transform;
            var rrModel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            DestroyImmediate(rrModel.GetComponent<CapsuleCollider>());
            rrModel.name = "RrModel";
            rrModel.transform.parent = _rr.transform;
            rrModel.transform.localPosition = new Vector3(0, 0, 0);
            rrModel.transform.localRotation = Quaternion.Euler(0, 0, 90);
            _rr.layer = LayerMask.NameToLayer("Robot");
        }

        _rr.transform.localPosition = new Vector3(
            driveTrainWidth / 2 * _multiplier - wheelDiameter / 2 * _multiplier - 1.5f * _multiplier,
            -_multiplier * 1 + (-_multiplier * wheelDiameter / 2) + (wheelDiameter * _multiplier * 0.1f),
            -driveTrainLength / 2 * _multiplier + wheelDiameter / 2 * _multiplier + 1.5f * _multiplier);
        _rr.transform.localScale = new Vector3(wheelWidth / 2 * _multiplier, wheelDiameter * _multiplier, wheelDiameter * _multiplier);
        _rr.transform.Find("RrModel").transform.localPosition = new Vector3(0,
            (_multiplier * 1 - (-_multiplier * wheelDiameter / 2) - (wheelDiameter * _multiplier * 0.1f) -_multiplier * 1)/(wheelDiameter * _multiplier), 0);

        //setup Rb drivecontroller, and player input.

        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody>();
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rb.useGravity = true;
            _rb.drag = 3;
            _rb.angularDrag = 3;
        }

        _rb.mass = drivetrainWeight;


        if (_driveController == null)
        {
            _driveController = gameObject.AddComponent<DriveController>();
            _driveController.driveTrainParent = _driveTrain;
            _driveController.driveTrain = global::DriveTrain.Swerve;
            _driveController.rayCastDistance = 0.25f;
        }

        _driveController.maxSpeed = driveSpeed * _maxMp;

        _driveController.accelerationSpeed = driveAcceleration * _velMp;

        if (_playerInput == null)
        {
            _playerInput = gameObject.AddComponent<PlayerInput>();
            _playerInput.actions = inputAsset;
            _playerInput.defaultControlScheme = "Controls 1";
            _playerInput.neverAutoSwitchControlSchemes = true;
            _playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
        }

        if (_bumper == null)
        {
            _bumper = new GameObject("Bumper");
            _bumper.transform.parent = _driveTrain.transform;
            _bumper.layer = LayerMask.NameToLayer("Robot");
        }
        
        if (_bumperParts == null)
        {
            _bumperParts = new GameObject[4];
        }
        else
        {
            for (int i = 0; i < _bumperParts.Length; i++)
            {
                if (_bumperParts[i] == null)
                {
                    foreach (var t in _bumperParts)
                    {
                        if (t != null)
                        {
                            DestroyImmediate(t);
                        }
                    }

                    _bumperParts = new GameObject[4];
                    for (int t = 0; t < 4; t++)
                    {
                        _bumperParts[t] = Instantiate(bumperPart, _bumper.transform.position, _bumper.transform.rotation, _bumper.transform);
                        _bumperParts[t].layer = LayerMask.NameToLayer("Robot");
                    }
                }
            }

            if (_bumperParts != null)
            {
                _bumperParts[0].transform.localPosition =
                    new Vector3(((bumperFrameOffset.y-bumperFrameOffset.z)/2)*_multiplier, (bumperHeight-2.5f)*_multiplier, (((driveTrainLength+(bumperFrameOffset.x*2)) / 2) * _multiplier));
                _bumperParts[1].transform.localPosition =
                    new Vector3(((bumperFrameOffset.y-bumperFrameOffset.z)/2)*_multiplier, (bumperHeight-2.5f)*_multiplier, -(((driveTrainLength+(bumperFrameOffset.w*2)) / 2) * _multiplier));
                _bumperParts[2].transform.localPosition =
                    new Vector3((((driveTrainWidth+(bumperFrameOffset.y*2)) / 2) * _multiplier), (bumperHeight-2.5f)*_multiplier, ((bumperFrameOffset.x-bumperFrameOffset.w)/2)*_multiplier);
                _bumperParts[3].transform.localPosition =
                    new Vector3(-(((driveTrainWidth+(bumperFrameOffset.z*2)) / 2)* _multiplier), (bumperHeight-2.5f)*_multiplier, ((bumperFrameOffset.x-bumperFrameOffset.w)/2)*_multiplier);

                _bumperParts[0].transform.localScale =
                    new Vector3((driveTrainWidth+bumperFrameOffset.y+bumperFrameOffset.z) * _multiplier, 1, 1);
                _bumperParts[1].transform.localScale =
                    new Vector3((driveTrainWidth+bumperFrameOffset.z+bumperFrameOffset.y) * _multiplier, 1, 1);
                _bumperParts[2].transform.localScale =
                    new Vector3(((driveTrainLength+bumperFrameOffset.x+bumperFrameOffset.w) * _multiplier)+((2.5f+0.75f)*2f*0.0254f), 1, 1);
                _bumperParts[3].transform.localScale =
                    new Vector3(((driveTrainLength+bumperFrameOffset.x+bumperFrameOffset.w) * _multiplier)+((2.5f+0.75f)*2f*0.0254f), 1, 1);

                _bumperParts[0].transform.localRotation = Quaternion.Euler(0, 0, 0);
                _bumperParts[1].transform.localRotation = Quaternion.Euler(0, 180, 0);
                _bumperParts[2].transform.localRotation = Quaternion.Euler(0, 90, 0);
                _bumperParts[3].transform.localRotation = Quaternion.Euler(0, 270, 0);
            }
        }
    }

    private void Startup()
    {
        _multiplier = units switch
        {
            Units.inch => 0.0254f,
            Units.meter => 1,
            Units.centimerter => 0.01f,
            Units.millimeter => 0.001f,
            _ => 0.0254f
        };

        if (transform.Find("DriveTrain") == null) return;
        _driveTrain = transform.Find("DriveTrain").gameObject;

        if (transform.Find("DriveTrain").transform.Find("Frame") != null)
        {
            _frame = transform.Find("DriveTrain").transform.Find("Frame").gameObject;

            if (transform.Find("DriveTrain").transform.Find("Frame").transform.Find("FrontRail") != null)
            {
                _frontRail = transform.Find("DriveTrain").transform.Find("Frame").transform.Find("FrontRail")
                    .gameObject;
            }

            if (transform.Find("DriveTrain").transform.Find("Frame").transform.Find("BackRail") != null)
            {
                _backRail = transform.Find("DriveTrain").transform.Find("Frame").transform.Find("BackRail")
                    .gameObject;
            }

            if (transform.Find("DriveTrain").transform.Find("Frame").transform.Find("LeftRail") != null)
            {
                _leftRail = transform.Find("DriveTrain").transform.Find("Frame").transform.Find("LeftRail")
                    .gameObject;
            }

            if (transform.Find("DriveTrain").transform.Find("Frame").transform.Find("RightRail") != null)
            {
                _rightRail = transform.Find("DriveTrain").transform.Find("Frame").transform.Find("RightRail")
                    .gameObject;
            }
        }

        if (transform.Find("DriveTrain").transform.Find("Wheels") != null)
        {
            _wheelChild = transform.Find("DriveTrain").transform.Find("Wheels").gameObject;


            if (transform.Find("DriveTrain").transform.Find("Wheels").transform.Find("Lf"))
            {
                _lfWheel = transform.Find("DriveTrain").transform.Find("Wheels").transform.Find("Lf").gameObject;
            }

            if (transform.Find("DriveTrain").transform.Find("Wheels").transform.Find("Rf"))
            {
                _rfWheel = transform.Find("DriveTrain").transform.Find("Wheels").transform.Find("Rf").gameObject;
            }

            if (transform.Find("DriveTrain").transform.Find("Wheels").transform.Find("Lr"))
            {
                _lrWheel = transform.Find("DriveTrain").transform.Find("Wheels").transform.Find("Lr").gameObject;
            }

            if (transform.Find("DriveTrain").transform.Find("Wheels").transform.Find("Rr"))
            {
                _rrWheel = transform.Find("DriveTrain").transform.Find("Wheels").transform.Find("Rr").gameObject;
            }
        }

        if (transform.Find("DriveTrain").transform.Find("Raycast") != null)
        {
            _raycastChild = transform.Find("DriveTrain").transform.Find("Raycast").gameObject;


            if (transform.Find("DriveTrain").transform.Find("Raycast").transform.Find("Lf"))
            {
                _lf = transform.Find("DriveTrain").transform.Find("Raycast").transform.Find("Lf").gameObject;
            }

            if (transform.Find("DriveTrain").transform.Find("Raycast").transform.Find("Rf"))
            {
                _rf = transform.Find("DriveTrain").transform.Find("Raycast").transform.Find("Rf").gameObject;
            }

            if (transform.Find("DriveTrain").transform.Find("Raycast").transform.Find("Lr"))
            {
                _lr = transform.Find("DriveTrain").transform.Find("Raycast").transform.Find("Lr").gameObject;
            }

            if (transform.Find("DriveTrain").transform.Find("Raycast").transform.Find("Rr"))
            {
                _rr = transform.Find("DriveTrain").transform.Find("Raycast").transform.Find("Rr").gameObject;
            }
        }

        if (transform.Find("DriveTrain").transform.Find("Bumper") != null)
        {
            _bumper = transform.Find("DriveTrain").transform.Find("Bumper").gameObject;
            
            if (_bumper.transform.childCount != 0)
            {
                for (int i = 0; i < _bumper.transform.childCount; i++)
                {
                    _bumperParts[i] = _bumper.transform.GetChild(i).gameObject;
                }
            }
        }

        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            _rb = gameObject.GetComponent<Rigidbody>();
        }

        if (gameObject.GetComponent<DriveController>() != null)
        {
            _driveController = gameObject.GetComponent<DriveController>();
        }

        if (gameObject.GetComponent<PlayerInput>() != null)
        {
            _playerInput = gameObject.GetComponent<PlayerInput>();
        }

        if (_bumperParts[0] != null)
        {

            for (int i = 0; i < _bumperParts.Length; i++)
            {
                _bumpRend[i] = _bumperParts[i].transform.Find("Model").gameObject;
            }
            
            _driveController.bumper = _bumpRend;
        }
    }
}
