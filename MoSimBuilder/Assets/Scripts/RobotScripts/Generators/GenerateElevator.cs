using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[ExecuteAlways]
public class GenerateElevator : MonoBehaviour
{
    private GameObject _mainRobot;
    private GameObject _elevator;
    private GameObject _stationary;

    private GameObject[] _stage;
    private GameObject[] _stageLeftModel;
    private GameObject[] _stageRightModel;
    private Rigidbody[] _rbs;
    private ConfigurableJoint[] _cgs;
    
    private GameObject[] _stageBuffer;
    private GameObject[] _stageLeftModelBuffer;
    private GameObject[] _stageRightModelBuffer;
    private Rigidbody[] _rbsBuffer;
    private ConfigurableJoint[] _cgsBuffer;
    
    private GameObject _stationaryLeftModel;
    private GameObject _stationaryRightModel;

    private Rigidbody _robotRb;
    private Rigidbody _rb;
    private FixedJoint _stationaryJoint;
    private JointDrive _stage1Drive;

    private bool _isStowed;

    //start visible stuff
    [Header("Elevator Information")]
    [SerializeField] private Units units;
    [SerializeField] private TubeType tubeType;
    [SerializeField] private int numOfStages = 1;
    [SerializeField] private float elevatorHeight = 16;
    [SerializeField] private float elevatorWidth = 13;

    [Header("Weights")]
    [SerializeField] private float stationaryWeight = 15;
    [SerializeField] private float[] stageWeights;
    

    [Header("Setpoints")]
    [SerializeField] private ControlType controlType;
    [Tooltip("Height in Units to go to")]
    [SerializeField] private float[] setpoints;
    [Tooltip("Button correlating to setpoint to go to")]
    [SerializeField] private Buttons[] setpointButton;
    
    [SerializeField] private float stowHieght;

    [SerializeField] private bool offsetSetpointsByStowHeight;
    
    
    //end visible stuff
    
    private float _multiplier;
    
    private float[] _stageWeightBuffer;
    
    private Buttons[] _setpointBuffer;
    
    private DriveController _driveController;
    
    private LayerMask _layer = new LayerMask();
    
    private PlayerInput _playerInput;
    
    private InputActionMap _inputMap;

    private float _activeTarget;

    private int _setpointSequence;
    
    private Buttons _lastButton;

    private bool _sequenceDebounce;
    
    private float _tubeHeight;
    private float _tubeWidth;
    // Start is called before the first frame update
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
        
        _tubeHeight = tubeType switch
        {
            TubeType.oneXTwo => 2,
            TubeType.oneXOne => 1,
            TubeType.oneXThree => 3,
            TubeType.twoXTwo => 2,
            _ => 2
        };
        
        _tubeWidth = tubeType switch
        {
            TubeType.oneXTwo => 1,
            TubeType.oneXOne => 1,
            TubeType.oneXThree => 1,
            TubeType.twoXTwo => 2,
            _ => 2
        };
        
        if (_mainRobot == null)
        {
            _mainRobot = transform.root.gameObject;
        }

        if (_robotRb == null)
        {
            _robotRb = _mainRobot.GetComponent<Rigidbody>();
        }
        
        _layer = LayerMask.GetMask("Robot");
        //create/update values
        if (!EditorApplication.isPlaying)
        {
            if (stageWeights != null)
            {
                if (stageWeights.Length != numOfStages)
                {
                    if (stageWeights != null)
                    {
                        _stageWeightBuffer = stageWeights;
                    }

                    stageWeights = new float[numOfStages];

                    for (int i = 0; i < numOfStages; i++)
                    {
                        stageWeights[i] = 3;
                    }

                    for (int i = 0; i < _stageWeightBuffer.Length; i++)
                    {
                        if (i < stageWeights.Length && i < _stageWeightBuffer.Length)
                        {
                            stageWeights[i] = _stageWeightBuffer[i];
                        }
                    }
                }
            }
            else
            {
                stageWeights = new float[numOfStages];

                for (int i = 0; i < numOfStages; i++)
                {
                    stageWeights[i] = 3;
                }
            }

            if (_elevator == null)
            {
                _elevator = gameObject;
            }

            if (_stationary == null)
            {
                _stationary = new GameObject();
                _stationary.transform.gameObject.layer = LayerMask.NameToLayer("Robot");
                _stationary.name = "Stationary";
                _stationary.transform.parent = _elevator.transform;
                _stationary.transform.localPosition = Vector3.zero;
                _stationary.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            
            if (_rb == null)
            {
                _rb = _stationary.AddComponent<Rigidbody>();
                _rb.drag = 0;
                
            }
            
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rb.useGravity = false;
            _rb.mass = stationaryWeight;
            _rb.excludeLayers = _layer;

            if (_stationaryJoint == null)
            {
                var t = transform;
                while (t.GetComponent<Rigidbody>() == null)
                {
                    t = t.parent.transform;
                }

                if (t.GetComponent<Rigidbody>() != null)
                {
                    _stationaryJoint = _stationary.AddComponent<FixedJoint>();
                    _stationaryJoint.connectedBody = t.GetComponent<Rigidbody>();
                }
            }

            if (_stationaryLeftModel == null)
            {
                _stationaryLeftModel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _stationaryLeftModel.name = "StationaryLeftModel";
                _stationaryLeftModel.transform.parent = _stationary.transform;
                _stationaryLeftModel.layer = LayerMask.NameToLayer("Robot");
                _stationaryLeftModel.transform.localRotation = Quaternion.Euler(Vector3.zero);
                
            }
            
            _stationaryLeftModel.transform.localPosition = new Vector3(-elevatorWidth/2 * _multiplier+((_tubeWidth/2)*0.0254f), elevatorHeight/2 * _multiplier, 0);
            _stationaryLeftModel.transform.localScale = new Vector3(_tubeWidth*0.0254f, elevatorHeight*_multiplier, _tubeHeight*0.0254f);
            
            if (_stationaryRightModel == null)
            {
                _stationaryRightModel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _stationaryRightModel.name = "StationaryRightModel";
                _stationaryRightModel.transform.parent = _stationary.transform;
                _stationaryRightModel.layer = LayerMask.NameToLayer("Robot");
                _stationaryRightModel.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            
            _stationaryRightModel.transform.localPosition = new Vector3(elevatorWidth/2 * _multiplier-((_tubeWidth/2)*0.0254f), elevatorHeight/2 * _multiplier, 0);
            _stationaryRightModel.transform.localScale = new Vector3(_tubeWidth*0.0254f, elevatorHeight*_multiplier, _tubeHeight*0.0254f);
            if (_stage != null)
            {
                if (numOfStages != _stage.Length)
                {
                    _stageBuffer = _stage;
                    _stageLeftModelBuffer = _stageLeftModel;
                    _stageRightModelBuffer = _stageRightModel;
                    _rbsBuffer = _rbs;
                    _cgsBuffer = _cgs;

                    _stage = new GameObject[numOfStages];
                    _stageLeftModel = new GameObject[numOfStages];
                    _stageRightModel = new GameObject[numOfStages];
                    _rbs = new Rigidbody[numOfStages];
                    _cgs = new ConfigurableJoint[numOfStages];

                    for (int i = 0; i < _stageBuffer.Length; i++)
                    {
                        if (i < _stage.Length && i < _stageBuffer.Length)
                        {
                            _stage[i] = _stageBuffer[i];
                            _stageLeftModel[i] = _stageLeftModelBuffer[i];
                            _stageRightModel[i] = _stageRightModelBuffer[i];
                            _rbs[i] = _rbsBuffer[i];
                            _cgs[i] = _cgsBuffer[i];
                        }
                        else
                        {
                            DestroyImmediate(_stageBuffer[i]);
                            DestroyImmediate(_stageLeftModelBuffer[i]);
                            DestroyImmediate(_stageRightModelBuffer[i]);
                        }
                    }
                }
            }
            else
            {
                _stage = new GameObject[numOfStages];
                _stageLeftModel = new GameObject[numOfStages];
                _stageRightModel = new GameObject[numOfStages];
                _rbs = new Rigidbody[numOfStages];
                _cgs = new ConfigurableJoint[numOfStages];
            }

            for (int i = 0; i < numOfStages; i++)
            {
                    
                if (_stage[i] == null)
                {
                    _stage[i] = new GameObject("Stage"+(i+1));
                    _stage[i].transform.gameObject.layer = LayerMask.NameToLayer("Robot");
                    _stage[i].transform.parent = _stationary.transform;
                    _stage[i].transform.localPosition = Vector3.zero;
                    _stage[i].transform.localRotation = Quaternion.Euler(Vector3.zero);
                }

                if (_rbs[i] == null)
                {
                    _rbs[i] = _stage[i].AddComponent<Rigidbody>();
                    _rbs[i].drag = 0;
                    _rbs[i].angularDrag = 1;
                }
            
                _rbs[i].interpolation = RigidbodyInterpolation.Interpolate;
                _rbs[i].collisionDetectionMode = CollisionDetectionMode.Continuous;
                _rbs[i].useGravity = true;
                _rbs[i].mass = stageWeights[i];
                _rbs[i].excludeLayers = _layer;
            
                if (_stageLeftModel[i] == null)
                {
                    _stageLeftModel[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    _stageLeftModel[i].transform.gameObject.layer = LayerMask.NameToLayer("Robot");
                    _stageLeftModel[i].name = "Stage"+(i+1)+"LeftModel";
                    _stageLeftModel[i].transform.parent = _stage[i].transform;
                    _stageLeftModel[i].transform.localRotation = Quaternion.Euler(Vector3.zero);
                }

                if (i + 1 == numOfStages)
                {
                    _stageLeftModel[i].transform.localPosition = new Vector3(-elevatorWidth/4 * _multiplier+(((i+1)*(_tubeWidth/2))*0.0254f+(((i)*0.05f)*0.0254f)), elevatorHeight/2 * _multiplier,0);
                    _stageLeftModel[i].transform.localScale = new Vector3((elevatorWidth/2*_multiplier) - ((i + 1)*0.0254f), _tubeWidth*0.0254f, _tubeHeight*0.0254f);
                }
                else
                {
                    _stageLeftModel[i].transform.localPosition = new Vector3( -elevatorWidth/2 * _multiplier+((i +1 +(_tubeWidth/2) +((i+1)*0.05f))*0.0254f), elevatorHeight/2 * _multiplier,0);
                    _stageLeftModel[i].transform.localScale = new Vector3(_tubeWidth*0.0254f, elevatorHeight*_multiplier, _tubeHeight*0.0254f);
                }
                 
            
                if (_stageRightModel[i] == null)
                {
                    _stageRightModel[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    _stageRightModel[i].name = "Stage"+(i+1)+"RightModel";
                    _stageRightModel[i].transform.gameObject.layer = LayerMask.NameToLayer("Robot");
                    _stageRightModel[i].transform.parent = _stage[i].transform;
                    _stageRightModel[i].transform.localRotation = Quaternion.Euler(Vector3.zero);
                }

                if (i + 1 == numOfStages)
                {
                    _stageRightModel[i].transform.localPosition = new Vector3(elevatorWidth/4 * _multiplier-(((i+1)*(_tubeWidth/2))*0.0254f-(((i)*0.05f)*0.0254f)), elevatorHeight/2 * _multiplier,0);
                    _stageRightModel[i].transform.localScale = new Vector3((elevatorWidth/2*_multiplier) - ((i + 1)*0.0254f), _tubeWidth*0.0254f, _tubeHeight*0.0254f);
                }
                else
                {
                    _stageRightModel[i].transform.localPosition = new Vector3(elevatorWidth/2 * _multiplier-((i +1 +(_tubeWidth/2) +((i+1)*0.05f))*0.0254f), elevatorHeight / 2 * _multiplier, 0);
                    _stageRightModel[i].transform.localScale = new Vector3(_tubeWidth * 0.0254f, elevatorHeight *_multiplier, _tubeHeight * 0.0254f);
                }

                if (_cgs[i] == null)
                {
                    _cgs[i] = _stage[i].AddComponent<ConfigurableJoint>();
                    _cgs[i].connectedBody = _rb;
                    _cgs[i].enablePreprocessing = true;
                }

                _cgs[i].xMotion = ConfigurableJointMotion.Locked;
                _cgs[i].zMotion = ConfigurableJointMotion.Locked;
                _cgs[i].angularXMotion = ConfigurableJointMotion.Locked;
                _cgs[i].angularYMotion = ConfigurableJointMotion.Locked;
                _cgs[i].angularZMotion = ConfigurableJointMotion.Locked;

                _stage1Drive.maximumForce = 900000000000f;
                _stage1Drive.positionDamper = 900000000000f;
                _stage1Drive.positionSpring = 0;

                _cgs[i].yDrive = _stage1Drive;
            }

            if (setpointButton != null)
            {
                if (setpointButton.Length != setpoints.Length)
                {
                    _setpointBuffer = setpointButton;

                    setpointButton = new Buttons[setpoints.Length];
                    for (int i = 0; i < _setpointBuffer.Length; i++)
                    {
                        if (i < setpoints.Length && i < _setpointBuffer.Length)
                        {
                            setpointButton[i] = _setpointBuffer[i];
                        }
                    }
                }
            }
            else
            {
                setpoints = new float[1];
                setpointButton = new Buttons[setpoints.Length];
            }

        }
        else //running logic
        {
            for (var i = 0; i < setpoints.Length; i++)
            {
                switch (controlType)
                {
                    case ControlType.toggle:
                    {
                        if (_inputMap.FindAction(setpointButton[i].ToString()).triggered)
                        {
                            if (Mathf.Approximately(_activeTarget, -setpoints[i]))
                            {
                                _activeTarget = -stowHieght + _tubeWidth;
                                _isStowed = true;
                            }
                            else
                            {
                                _activeTarget = -setpoints[i];
                                _isStowed = false;
                            }

                        }

                        break;
                    }
                    case ControlType.hold:
                    {
                        if (_inputMap.FindAction(setpointButton[i].ToString()).IsPressed())
                        {
                            _activeTarget = -setpoints[i];
                            _isStowed = false;
                        }
                        else if (i > 0)
                        {
                            for (var t = 0; t < setpoints.Length; t++)
                            {
                                if (_inputMap.FindAction(setpointButton[t].ToString()).IsPressed())
                                {
                                    _activeTarget = -setpoints[t];
                                    _isStowed = false;
                                }
                            }
                        }
                        else
                        {
                            _activeTarget = -stowHieght + _tubeWidth;
                            _isStowed = true;
                        }
                        
                        break;
                    }
                    case ControlType.sequence:
                    {
                        if (_inputMap.FindAction(setpointButton[i].ToString()).triggered && _sequenceDebounce == false)
                        {
                        
                            if (_setpointSequence < setpoints.Length)
                            {
                                if (_lastButton != setpointButton[i])
                                {
                                    _setpointSequence = 0;
                                    _lastButton = setpointButton[i];
                                    _isStowed = false;
                                }
                            
                                while (setpointButton[i] != setpointButton[_setpointSequence])
                                {
                                    _setpointSequence ++;

                                    if (_setpointSequence+1 > setpoints.Length)
                                    {
                                        _setpointSequence = 0;
                                        _activeTarget = -stowHieght + _tubeWidth;
                                        _isStowed = true;
                                        return;
                                    }


                                }
                                _activeTarget = -setpoints[_setpointSequence];
                                _isStowed = false;
                                _setpointSequence += 1;
                            }
                            else
                            {
                                _setpointSequence = 0;
                                _activeTarget = -stowHieght + _tubeWidth;
                                _isStowed = true;
                            }
                        
                            _lastButton = setpointButton[i];
                        }
                        else
                        {
                       
                        }

                        if (_inputMap.FindAction(setpointButton[i].ToString()).IsPressed())
                        {
                            _sequenceDebounce = true;
                        }
                        else
                        {
                            _sequenceDebounce = false;
                        }

                        break;
                    } case ControlType.lastPressed:
                    {
                        if (_inputMap.FindAction(setpointButton[i].ToString()).triggered)
                        {
                            _activeTarget = -setpoints[i];
                            _isStowed = false;
                        }

                        break;
                    }
                }
            }

            for (var i = 0; i < _stage.Length; i++)
            {
                
                if (i + 1 == numOfStages)
                {
                    float stowOffset;
                    
                    if (offsetSetpointsByStowHeight && !_isStowed)
                    {
                        stowOffset = ((elevatorHeight / 2)) - stowHieght + (_tubeWidth/2);
                    }
                    else
                    {
                        stowOffset = (elevatorHeight / 2) - (_tubeWidth/2);
                    }
                    
                    _cgs[i].targetVelocity = new Vector3(0, Mathf.Clamp((-_stage[i].transform.localPosition.y - ((_activeTarget+stowOffset) * _multiplier)) * -8f, -5, 5), 0);
                }
                else
                {
                    float stowOffset;
                    
                    if (offsetSetpointsByStowHeight && !_isStowed)
                    {
                        stowOffset = stowHieght -_tubeWidth/2;
                    }
                    else
                    {
                        stowOffset = -_tubeWidth/2;
                    }
                    
                    if (-_activeTarget + stowOffset >= ((numOfStages-i-1) * (elevatorHeight)))
                    {
                        _cgs[i].targetVelocity = new Vector3(0, Mathf.Clamp((-_stage[i].transform.localPosition.y - (_activeTarget - (stowOffset) - (_tubeWidth/2) + (numOfStages-i-1) * elevatorHeight) * _multiplier ) * -8f, -5, 5), 0);
                    }
                    else
                    {
                        _cgs[i].targetVelocity = new Vector3(0, Mathf.Clamp((-_stage[i].transform.localPosition.y -(0 * _multiplier)) * -8f, -3, 3), 0);
                    }
                    
                }
            }
        }
    }

    private void Startup()
    {
        
        _activeTarget = -stowHieght + _tubeWidth;
        
        _isStowed = true;
        
        _setpointSequence = 0;
        
        _multiplier = units switch
        {
            Units.inch => 0.0254f,
            Units.meter => 1,
            Units.centimerter => 0.01f,
            Units.millimeter => 0.001f,
            _ => 0.0254f
        };
        
        _tubeHeight = tubeType switch
        {
            TubeType.oneXTwo => 2,
            TubeType.oneXOne => 1,
            TubeType.oneXThree => 3,
            TubeType.twoXTwo => 2,
            _ => 2
        };
        
        _tubeWidth = tubeType switch
        {
            TubeType.oneXTwo => 1,
            TubeType.oneXOne => 1,
            TubeType.oneXThree => 1,
            TubeType.twoXTwo => 2,
            _ => 2
        };
        
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
        
        if (_elevator == null)
        {
            _elevator = gameObject;
        }

        if (_elevator.transform.Find("Stationary"))
        {
            _stationary = _elevator.transform.Find("Stationary").gameObject;

            if (_stationary.GetComponent<Rigidbody>() != null)
            {
                _rb = _stationary.GetComponent<Rigidbody>();
            }

            if (_stationary.GetComponent<FixedJoint>() != null)
            {
                _stationaryJoint = _stationary.GetComponent<FixedJoint>();
            }

            if (_stationary.transform.Find("StationaryLeftModel"))
            {
                _stationaryLeftModel = _stationary.transform.Find("StationaryLeftModel").gameObject;
            }

            if (_stationary.transform.Find("StationaryRightModel"))
            {
                _stationaryRightModel = _stationary.transform.Find("StationaryRightModel").gameObject;
            }
            
            _stage = new GameObject[numOfStages];
            _stageLeftModel = new GameObject[numOfStages];
            _stageRightModel = new GameObject[numOfStages];
            _rbs = new Rigidbody[numOfStages];
            _cgs = new ConfigurableJoint[numOfStages];
            
            for (var i = 0; i < numOfStages; i++)
            {
                
                if (_stationary.transform.Find("Stage" + (i+1)) == null) return;
                _stage[i] = _stationary.transform.Find("Stage"+(i+1)).gameObject;

                if (_stage[i].GetComponent<Rigidbody>() != null)
                {
                    _rbs[i] = _stage[i].GetComponent<Rigidbody>();
                }

                if (_stage[i].transform.Find("Stage"+(i+1)+"LeftModel"))
                {
                    _stageLeftModel[i] = _stage[i].transform.Find("Stage"+(i+1)+"LeftModel").gameObject;
                }

                if (_stage[i].transform.Find("Stage"+(i+1)+"RightModel"))
                {
                    _stageRightModel[i] = _stage[i].transform.Find("Stage"+(i+1)+"RightModel").gameObject;
                }

                if (_stage[i].GetComponent<ConfigurableJoint>() != null)
                {
                    _cgs[i] = _stage[i].GetComponent<ConfigurableJoint>();
                }
            }

        }
    }
}
