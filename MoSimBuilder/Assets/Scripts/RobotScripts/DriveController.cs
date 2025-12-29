using System;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class DriveController : MonoBehaviour
{
    [HideInInspector]
    public DriveTrain driveTrain;

    [Tooltip("add bumper number objects to list for custom bumper num")]
    private TMP_Text[] bumperNumbers;

    [Tooltip("reverses the text")]
    private bool reverseBumperAllianceText = false;
    
    [HideInInspector][FormerlySerializedAs("DriveTrainParent")] [Tooltip("Game Object with the Wheels and Raycast children")]
    public GameObject driveTrainParent;

    [HideInInspector][Tooltip("distance to ground to look for before no longer touching the ground")]
    [SerializeField] public float rayCastDistance;

    [HideInInspector]
    private bool flipRayCastDir = false;

    private Collider _field;

    private bool _isReseting = false;

    //Handles climbing logic
    [HideInInspector]
    public bool isGrounded = true;

    [HideInInspector]
    public bool isTouchingGround = true;

    [HideInInspector][Tooltip("sets Max Speed in ft/s")]
    public float maxSpeed = 5;

    [HideInInspector][FormerlySerializedAs("AccelerationSpeed")] [Tooltip("sets constant acceleration")]
    public float accelerationSpeed = 20f;

    [HideInInspector][FormerlySerializedAs("SteerMultiplyer")] [Tooltip("sets modified base steering")]
    public float steerMultiplyer = 1.0f;

    [HideInInspector][Tooltip("displays current velocity. Does not Set velocity")]
    public float beforeVelocity;

    [Tooltip("Sets the robots CG")]
     private Vector3 centerOfMass;

    [HideInInspector]
    public bool isRedRobot = false;

    [HideInInspector]
    public bool areRobotsTouching;

    [FormerlySerializedAs("DsMove")] [HideInInspector]
    public Vector2 dsMove;

    [HideInInspector]
    public static bool CanBlueRotate;

    [HideInInspector]
    public static bool CanRedRotate;

    [HideInInspector]
    public static bool IsTouchingWallColliderRed = false;

    [HideInInspector]
    public static bool IsTouchingWallColliderBlue = false;

    [HideInInspector]
    public Vector3 velocity { get; set; }

    [HideInInspector]
    public bool startingReversed;

    public static bool RobotsTouching;
    public static bool IsPinningRed = false;
    public static bool IsPinningBlue = false;
    public static bool IsAmped = false;
    public static bool IsRedAmped = false;

    private Rigidbody _rb;
    private Vector2 _translateValue;
    private float _rotateValue;
    [HideInInspector]
    public float intakeValue = 0f;
    private bool _dontPlayDriveSounds = false;
    private bool _useSwerveSounds;
    private bool _useIntakeSounds;

    [HideInInspector]
    public Material[] materialPrefab = new Material[2];

    [Tooltip("Game Objects for bumper recoloring")]
    [HideInInspector] public GameObject[] bumper;

    private Material _bumperMat;

    private Color _defaultBumperColor;

    private bool _dontUpdateBeforeVelocity = false;
    [HideInInspector]
    public bool isFieldCentric = false;

    private GameManager _gameManager;

    private Vector3 _startingPos;
    private Quaternion _startingRot;

    [HideInInspector]
    public bool atTargetPos = false;
    [HideInInspector]
    public bool atTargetRot = false;
    [HideInInspector]
    public float targetOffset;
    [FormerlySerializedAs("VisionMultiplyer")] [HideInInspector]
    public float visionMultiplyer;
    [FormerlySerializedAs("VisionMax")] [HideInInspector]
    public float visionMax;
    [HideInInspector]
    public bool validVision = false;

    private bool _onAlign;

    private float _steer2;
    [FormerlySerializedAs("VelocityMP")] [HideInInspector]
    public float velocityMp;


    private Transform _raycastChild;

    private Transform _wheelChild;

    private bool _driveTrainAssigned;

    private bool _driveTrainError;

    private bool _flag;

    private Transform LeftFront = null, RightFront = null, LeftRear = null, RightRear = null, LeftCenter = null, RightCenter = null, CenterWheel = null;
    private SwerveWheel LeftFrontW = null, RightFrontW = null, LeftRearW = null, RightRearW = null;


    private AudioSource _treadPlayer;
    private AudioSource _gearPlayer;
    private AudioResource _swerveSound;
    private AudioResource _gearSound;

    private AudioSource _player;
    private AudioResource[] _hitSounds;

    private bool _triggerSound;
    private bool _useSounds;
    
    private PlayerInput _playerInput;
    private InputActionMap _inputActionMap;
    
    private InputAction _translateAction;
    private InputAction _rotateAction;
    
    [HideInInspector] public InputActionAsset _inputActionAsset;

    private void Start()
    {
        rayCastDistance = 0.75f*0.0254f;

        _playerInput = gameObject.GetComponent<PlayerInput>();
        _playerInput.actions = _inputActionAsset;
        _playerInput.actions.Enable();
        
        _inputActionMap = _playerInput.currentActionMap;
        _inputActionMap.Enable();
        
        _translateAction = _inputActionMap.FindAction("LeftStick");
        _rotateAction = _inputActionMap.FindAction("RightStick");
        _translateAction.Enable();
        _rotateAction.Enable();
        
        if (PlayerPrefs.GetInt("quality") == 0)
        {
            Graphics.activeTier = GraphicsTier.Tier1;
        } else if (PlayerPrefs.GetInt("quality") == 1)
        {
            Graphics.activeTier = GraphicsTier.Tier2;
        }
        else
        {
            Graphics.activeTier = GraphicsTier.Tier3;
        }
        
        gameObject.layer = 7;
        _flag = false; 
        _driveTrainAssigned = false;

        _driveTrainError = false;

        _treadPlayer = transform.gameObject.AddComponent<AudioSource>();
        _gearPlayer = transform.gameObject.AddComponent<AudioSource>();

        _gearSound = Resources.Load("Misc/ShooterSounds/swervegears", typeof(AudioResource)) as AudioResource;
        _swerveSound = Resources.Load("Misc/ShooterSounds/swervesounds", typeof(AudioResource)) as AudioResource;

        _treadPlayer.resource = _swerveSound;
        _treadPlayer.loop = true;

        _gearPlayer.resource = _gearSound;
        _gearPlayer.loop = true;

        _useSounds = PlayerPrefs.GetInt("bumpSounds") == 1;

        _player = transform.gameObject.AddComponent<AudioSource>();

        _hitSounds = new AudioResource[14];

        _hitSounds[0] = Resources.Load("Misc/BumpSounds/bump", typeof(AudioResource)) as AudioResource;
        _hitSounds[1] = Resources.Load("Misc/BumpSounds/bump2", typeof(AudioResource)) as AudioResource;
        _hitSounds[2] = Resources.Load("Misc/BumpSounds/bump3", typeof(AudioResource)) as AudioResource;
        _hitSounds[3] = Resources.Load("Misc/BumpSounds/bump4", typeof(AudioResource)) as AudioResource;
        _hitSounds[4] = Resources.Load("Misc/BumpSounds/bump5", typeof(AudioResource)) as AudioResource;
        _hitSounds[5] = Resources.Load("Misc/BumpSounds/bump6", typeof(AudioResource)) as AudioResource;
        _hitSounds[6] = Resources.Load("Misc/BumpSounds/bump7", typeof(AudioResource)) as AudioResource;
        _hitSounds[7] = Resources.Load("Misc/BumpSounds/bump8", typeof(AudioResource)) as AudioResource;
        _hitSounds[8] = Resources.Load("Misc/BumpSounds/bump9", typeof(AudioResource)) as AudioResource;
        _hitSounds[9] = Resources.Load("Misc/BumpSounds/bump10", typeof(AudioResource)) as AudioResource;
        _hitSounds[10] = Resources.Load("Misc/BumpSounds/loudbump", typeof(AudioResource)) as AudioResource;
        _hitSounds[11] = Resources.Load("Misc/BumpSounds/loudbump2", typeof(AudioResource)) as AudioResource;
        _hitSounds[12] = Resources.Load("Misc/BumpSounds/quietbump", typeof(AudioResource)) as AudioResource;
        _hitSounds[13] = Resources.Load("Misc/BumpSounds/quietbump2", typeof(AudioResource)) as AudioResource;


        if (driveTrainParent == null)
        {
            print("Please add a Drive Train Parent");
            _flag = true;
        }

        if (driveTrainParent.transform.Find("Raycast") != null)
        {
            _raycastChild = driveTrainParent.transform.Find("Raycast");
        } else
        {
            print("Please add a Raycast Child to DriveTrain Parent");
            _flag = true;
        }

        if (driveTrainParent.transform.Find("Wheels") != null)
        {
            _wheelChild = driveTrainParent.transform.Find("Wheels");
        }
        else
        {
            print("Please add a Wheels Child to DriveTrain Parent");
            _flag = true;
        }

        if (bumperNumbers != null)
        {
            foreach (var t in bumperNumbers)
            {
                if (t == null)
                {
                    print("Missing bumper Number Assignment");
                }
            }
        }

        if (bumper != null)
        {
            foreach (var t in bumper)
            {
                if (t == null)
                {
                    print("Missing bumper model assignment");
                }
            }
        }

        materialPrefab[0] = Resources.Load("Misc/BumperColor/RedBumper", typeof(Material)) as Material;
        if (materialPrefab[0] == null)
        {
            print("Bad material");
            _flag = true;
        }
        materialPrefab[1] = Resources.Load("Misc/BumperColor/Blue", typeof(Material)) as Material;
        if (materialPrefab[1] == null)
        {
            print("Bad material");
            _flag = true;
        }
        velocityMp = 1f;

        _startingPos = transform.position;
        _startingRot = transform.rotation;

        if (materialPrefab != null)
        {
            if (isRedRobot)
            {
                _bumperMat = Instantiate(materialPrefab[0]);
            }
            else
            {
                _bumperMat = Instantiate(materialPrefab[1]);
            }

            if (bumper != null)
            {
                foreach (var t in bumper)
                {
                    foreach (Material mat in t.GetComponent<Renderer>().materials)
                    {
                        mat.color = _bumperMat.color;
                    }
                }
            }



            _defaultBumperColor = _bumperMat.color;
        }
        else 
        { 
            Debug.LogError("Material prefab is not assigned!");
            _flag = true;
        }

        if (!reverseBumperAllianceText)
        {
            switch (isRedRobot)
            {
                case true when PlayerPrefs.GetString("redName") != "":
                {
                    foreach (TMP_Text bumperNumber in bumperNumbers)
                    {
                        bumperNumber.text = PlayerPrefs.GetString("redName");
                    }

                    break;
                }
                case false when PlayerPrefs.GetString("blueName") != "":
                {
                    foreach (TMP_Text bumperNumber in bumperNumbers)
                    {
                        bumperNumber.text = PlayerPrefs.GetString("blueName");
                    }

                    break;
                }
            }
        }
        else
        {
            switch (isRedRobot)
            {
                case true when PlayerPrefs.GetString("blueName") != "":
                {
                    foreach (TMP_Text bumperNumber in bumperNumbers)
                    {
                        bumperNumber.text = PlayerPrefs.GetString("blueName");
                    }

                    break;
                }
                case false when PlayerPrefs.GetString("redName") != "":
                {
                    foreach (TMP_Text bumperNumber in bumperNumbers)
                    {
                        bumperNumber.text = PlayerPrefs.GetString("redName");
                    }

                    break;
                }
            }
        }


        _useSwerveSounds = PlayerPrefs.GetInt("swerveSounds") == 1;
        _useIntakeSounds = PlayerPrefs.GetInt("intakeSounds") == 1;

        //Resetting static variables on start
        CanBlueRotate = true;
        CanRedRotate = true;

        IsTouchingWallColliderRed = false;
        IsTouchingWallColliderBlue = false;

        IsPinningRed = false;
        IsPinningBlue = false;
        RobotsTouching = false;
        velocity = new Vector3(0f, 0f, 0f);
        IsAmped = false;
        IsRedAmped = false;

        //Initializing starting transforms
        _rb = GetComponent<Rigidbody>();

        _gameManager = GameObject.Find("GameGUI").GetComponent<GameManager>();

        _field = GameObject.Find("DsColliders").GetComponent<Collider>();

        maxSpeed = maxSpeed * transform.localScale.x * 0.3048f;

        accelerationSpeed = accelerationSpeed * transform.localScale.x * 0.3048f;
        
        if (_rb == null)
        {
            _flag = true;
            print("The Rigid Body has not been found on the" + transform.name + "Game Object. addding temporary rigid body");
            _rb = transform.gameObject.AddComponent<Rigidbody>();
            _rb.mass = 100;
            _rb.drag = 3;
            _rb.automaticInertiaTensor = true;
            _rb.useGravity = true;
            _rb.angularDrag = 3;
            _rb.isKinematic = false;
        }

        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        Vector3 autoCg = _rb.centerOfMass;
        _rb.automaticCenterOfMass = false;
        _rb.centerOfMass = new Vector3(0, autoCg.y, 0);
    }

    private void Update()
    {
            if (_flag) return;
            isGrounded = CheckGround();
            areRobotsTouching = RobotsTouching;

            if (!_dontUpdateBeforeVelocity)
            {
                if (!IsTouchingWallColliderBlue && !isRedRobot || !IsTouchingWallColliderRed && isRedRobot)
                {
                    beforeVelocity = _rb.velocity.magnitude;
                }
            }

            if (!isRedRobot)
            {
                if (RobotsTouching && IsTouchingWallColliderBlue)
                {
                    IsPinningBlue = true;
                }
                else
                {
                    IsPinningBlue = false;
                }
            }
            else
            {
                if (RobotsTouching && IsTouchingWallColliderRed)
                {
                    IsPinningRed = true;
                }
                else
                {
                    IsPinningRed = false;
                }
            }

            _dontPlayDriveSounds = !GameManager.canRobotMove;

            velocity = _rb.velocity;

            
            
                var isMovingOrRotating = Math.Abs(Math.Round(_rb.velocity.x)) > 0f ||
                                         Math.Abs(Math.Round(_rb.velocity.z)) > 0f ||
                                         Math.Abs(_rb.angularVelocity.magnitude) > 0f;

                if (isMovingOrRotating && !_dontPlayDriveSounds)
                {
                    PlaySwerveSounds();
                }
                else
                {
                    StopSwerveSounds();
                }
            

            if (!_useSounds) return;
            var touchingWall = (isRedRobot && IsTouchingWallColliderRed) || (!isRedRobot && IsTouchingWallColliderBlue);

            if (touchingWall && !_player.isPlaying && _triggerSound)
            {
                _player.volume = _rb.velocity.magnitude * 0.02f;
                _player.resource = _hitSounds[UnityEngine.Random.Range(0, _hitSounds.Length)];
                _player.Play();
            }

            _triggerSound = !touchingWall;
            
            
    }

    private void FixedUpdate()
    {
        _translateValue = _translateAction.ReadValue<Vector2>();
        _rotateValue = _rotateAction.ReadValue<Vector2>().x;
        
        if (GameManager.canRobotMove && !_flag)
        {
            if (_onAlign && validVision)
            {
                _steer2 = Mathf.Clamp(targetOffset * visionMultiplyer,-1* visionMax, 1* visionMax);
            }
            else
            {
                _steer2 = _rotateValue;
            }


            switch (driveTrain)
            {
                case DriveTrain.Tank:
                {
                    if (!_driveTrainAssigned)
                    {
                        if (_raycastChild.Find("Lf") != null)
                        {
                            LeftFront = _raycastChild.Find("Lf");
                        }
                        else
                        {
                            print("No Lf Raycast Object Detected");
                            _driveTrainError = true;
                        }

                        if (_raycastChild.Find("Rf") != null)
                        {
                            RightFront = _raycastChild.Find("Rf");
                        }
                        else
                        {
                            print("No Rf Raycast Object Detected");
                            _driveTrainError = true;
                        }

                        if (_raycastChild.Find("Lr") != null)
                        {
                            LeftRear = _raycastChild.Find("Lr");
                        }
                        else
                        {
                            print("No Lr Raycast Object Detected");
                            _driveTrainError = true;
                        }

                        if (_raycastChild.Find("Rr") != null)
                        {
                            RightRear = _raycastChild.Find("Rr");
                        }
                        else
                        {
                            print("No Rr Raycast Object Detected");
                            _driveTrainError = true;
                        }

                        if (_raycastChild.Find("Rc") != null)
                        {
                            RightCenter = _raycastChild.Find("Rc");
                        }
                        else
                        {
                            print("No Rc Raycast Object Detected");
                            _driveTrainError = true;
                        }

                        if (_raycastChild.Find("Lc") != null)
                        {
                            LeftCenter = _raycastChild.Find("Lc");
                        }
                        else
                        {
                            print("No Lc Raycast Object Detected");
                            _driveTrainError = true;
                        }




                        if (_wheelChild.Find("Lf").GetComponent<TankWheel>() == null)
                        {
                            _wheelChild.Find("Lf").gameObject.AddComponent<TankWheel>();
                        }

                        if (_wheelChild.Find("Rf").GetComponent<TankWheel>() == null)
                        {
                            _wheelChild.Find("Rf").gameObject.AddComponent<TankWheel>();
                        }

                        if (_wheelChild.Find("Lr").GetComponent<TankWheel>() == null)
                        {
                            _wheelChild.Find("Lr").gameObject.AddComponent<TankWheel>();
                        }

                        if (_wheelChild.Find("Rr").GetComponent<TankWheel>() == null)
                        {
                            _wheelChild.Find("Rr").gameObject.AddComponent<TankWheel>();
                        }

                        if (_wheelChild.Find("Lc").GetComponent<TankWheel>() == null)
                        {
                            _wheelChild.Find("Lc").gameObject.AddComponent<TankWheel>();
                        }

                        if (_wheelChild.Find("Rc").GetComponent<TankWheel>() == null)
                        {
                            _wheelChild.Find("Rc").gameObject.AddComponent<TankWheel>();
                        }

                        _driveTrainAssigned = true;
                    }

                    if (!_driveTrainError)
                    {
                        _rb.maxAngularVelocity = (float)((LeftCenter.position.x - RightCenter.position.x * Math.PI) * (maxSpeed));

                        float leftTankSpeed = (float)(accelerationSpeed * Math.Clamp(_translateValue.y * velocityMp + (_steer2 * steerMultiplyer), -1, 1));
                        float rightTankSpeed = (float)(accelerationSpeed * Math.Clamp(_translateValue.y * velocityMp - (_steer2 * steerMultiplyer), -1, 1));

                        if (Physics.Raycast(LeftFront.position, -transform.up, rayCastDistance))
                        {
                            _rb.AddForceAtPosition(leftTankSpeed * transform.forward, LeftFront.position);
                        }

                        if (Physics.Raycast(LeftCenter.position, -transform.up, rayCastDistance))
                        {
                            _rb.AddForceAtPosition(leftTankSpeed * transform.forward, LeftCenter.position);
                        }

                        if (Physics.Raycast(LeftRear.position, -transform.up, rayCastDistance))
                        {
                            _rb.AddForceAtPosition(leftTankSpeed * transform.forward, LeftRear.position);
                        }

                        if (Physics.Raycast(RightFront.position, -transform.up, rayCastDistance))
                        {
                            _rb.AddForceAtPosition(rightTankSpeed * transform.forward, RightFront.position);
                        }

                        if (Physics.Raycast(RightCenter.position, -transform.up, rayCastDistance))
                        {
                            _rb.AddForceAtPosition(rightTankSpeed * transform.forward, RightCenter.position);
                        }

                        if (Physics.Raycast(RightRear.position, -transform.up, rayCastDistance))
                        {
                            _rb.AddForceAtPosition(rightTankSpeed * transform.forward, RightRear.position);
                        }
                    }
                    else if (driveTrain == DriveTrain.HDrive)
                    {
                        if (!_driveTrainAssigned)
                        {
                            if (_raycastChild.Find("Lf") != null)
                            {
                                LeftFront = _raycastChild.Find("Lf");
                            }
                            else
                            {
                                print("No Lf Raycast Object Detected");
                                _driveTrainError = true;
                            }

                            if (_raycastChild.Find("Rf") != null)
                            {
                                RightFront = _raycastChild.Find("Rf");
                            }
                            else
                            {
                                print("No Rf Raycast Object Detected");
                                _driveTrainError = true;
                            }

                            if (_raycastChild.Find("Lr") != null)
                            {
                                LeftRear = _raycastChild.Find("Lr");
                            }
                            else
                            {
                                print("No Lr Raycast Object Detected");
                                _driveTrainError = true;
                            }

                            if (_raycastChild.Find("Rr") != null)
                            {
                                RightRear = _raycastChild.Find("Rr");
                            }
                            else
                            {
                                print("No Rr Raycast Object Detected");
                                _driveTrainError = true;
                            }

                            if (_raycastChild.Find("Rc") != null)
                            {
                                RightCenter = _raycastChild.Find("Rc");
                            }
                            else
                            {
                                print("No Rc Raycast Object Detected");
                                _driveTrainError = true;
                            }

                            if (_raycastChild.Find("Lc") != null)
                            {
                                LeftCenter = _raycastChild.Find("Lc");
                            }
                            else
                            {
                                print("No Lc Raycast Object Detected");
                                _driveTrainError = true;
                            }

                            if (_raycastChild.Find("Cc") != null)
                            {
                                CenterWheel = _raycastChild.Find("Cc");
                            }
                            else
                            {
                                print("No Cc Raycast Object Detected");
                                _driveTrainError = true;
                            }




                            if (_wheelChild.Find("Lf").GetComponent<TankWheel>() == null)
                            {
                                _wheelChild.Find("Lf").gameObject.AddComponent<TankWheel>();
                            }

                            if (_wheelChild.Find("Rf").GetComponent<TankWheel>() == null)
                            {
                                _wheelChild.Find("Rf").gameObject.AddComponent<TankWheel>();
                            }

                            if (_wheelChild.Find("Lr").GetComponent<TankWheel>() == null)
                            {
                                _wheelChild.Find("Lr").gameObject.AddComponent<TankWheel>();
                            }

                            if (_wheelChild.Find("Rr").GetComponent<TankWheel>() == null)
                            {
                                _wheelChild.Find("Rr").gameObject.AddComponent<TankWheel>();
                            }

                            if (_wheelChild.Find("Lc").GetComponent<TankWheel>() == null)
                            {
                                _wheelChild.Find("Lc").gameObject.AddComponent<TankWheel>();
                            }

                            if (_wheelChild.Find("Rc").GetComponent<TankWheel>() == null)
                            {
                                _wheelChild.Find("Rc").gameObject.AddComponent<TankWheel>();
                            }

                            if (_wheelChild.Find("Cc").GetComponent<HWheel>() == null)
                            {
                                _wheelChild.Find("Cc").gameObject.AddComponent<HWheel>();
                            }

                            _driveTrainAssigned = true;
                        }
                        if (!_driveTrainError)
                        {
                            _rb.maxAngularVelocity = (float)((LeftCenter.position.x - RightCenter.position.x * Math.PI) * (maxSpeed));

                            float leftTankSpeed = (float)(accelerationSpeed * Math.Clamp(_translateValue.y + (_steer2 * steerMultiplyer), -1, 1));
                            float rightTankSpeed = (float)(accelerationSpeed * Math.Clamp(_translateValue.y - (_steer2 * steerMultiplyer), -1, 1));
                            float centerWheelSpeed = (float)(accelerationSpeed * 4 * Math.Clamp(_translateValue.x, -1, 1));

                            if (Physics.Raycast(LeftFront.position, -transform.up, rayCastDistance))
                            {
                                _rb.AddForceAtPosition(leftTankSpeed * transform.forward, LeftFront.position);
                            }

                            if (Physics.Raycast(LeftCenter.position, -transform.up, rayCastDistance))
                            {
                                _rb.AddForceAtPosition(leftTankSpeed * transform.forward, LeftCenter.position);
                            }

                            if (Physics.Raycast(LeftRear.position, -transform.up, rayCastDistance))
                            {
                                _rb.AddForceAtPosition(leftTankSpeed * transform.forward, LeftRear.position);
                            }

                            if (Physics.Raycast(RightFront.position, -transform.up, rayCastDistance))
                            {
                                _rb.AddForceAtPosition(rightTankSpeed * transform.forward, RightFront.position);
                            }

                            if (Physics.Raycast(RightCenter.position, -transform.up, rayCastDistance))
                            {
                                _rb.AddForceAtPosition(rightTankSpeed * transform.forward, RightCenter.position);
                            }

                            if (Physics.Raycast(RightRear.position, -transform.up, rayCastDistance))
                            {
                                _rb.AddForceAtPosition(rightTankSpeed * transform.forward, RightRear.position);
                            }

                            if (Physics.Raycast(CenterWheel.position, -transform.up, rayCastDistance))
                            {
                                _rb.AddForceAtPosition(centerWheelSpeed * transform.right, CenterWheel.position);
                            }
                        }
                    }

                    break;
                }
                case DriveTrain.Swerve:
                {
                    /*
                    A = STR - RCW * L/2
                    B = STR + RCW * L/2
                    C = STR - RCW * W/2
                    D = STR + RCW * W/2
                 */


                    if (!_driveTrainAssigned)
                    {
                        if (_raycastChild.Find("Lf") != null)
                        {
                            LeftFront = _raycastChild.Find("Lf");
                        }
                        else
                        {
                            print("No Lf Raycast Object Detected");
                            _driveTrainError = true;
                        }

                        if (_raycastChild.Find("Rf") != null)
                        {
                            RightFront = _raycastChild.Find("Rf");
                        }
                        else
                        {
                            print("No Rf Raycast Object Detected");
                            _driveTrainError = true;
                        }

                        if (_raycastChild.Find("Lr") != null)
                        {
                            LeftRear = _raycastChild.Find("Lr");
                        }
                        else
                        {
                            print("No Lr Raycast Object Detected");
                            _driveTrainError = true;
                        }

                        if (_raycastChild.Find("Rr") != null)
                        {
                            RightRear = _raycastChild.Find("Rr");
                        }
                        else
                        {
                            print("No Rr Raycast Object Detected");
                            _driveTrainError = true;
                        }


                        if (_wheelChild.Find("Lf") != null)
                        {
                            LeftFrontW = _wheelChild.Find("Lf").GetComponent<SwerveWheel>();
                        }
                        else
                        {
                            print("No Lf Wheel Object Detected");
                            _driveTrainError = true;
                        }

                        if (_raycastChild.Find("Rf") != null)
                        {
                            RightFrontW = _wheelChild.Find("Rf").GetComponent<SwerveWheel>();
                        }
                        else
                        {
                            print("No Rf Wheel Object Detected");
                            _driveTrainError = true;
                        }

                        if (_raycastChild.Find("Lr") != null)
                        {
                            LeftRearW = _wheelChild.Find("Lr").GetComponent<SwerveWheel>();
                        }
                        else
                        {
                            print("No Lr Wheel Object Detected");
                            _driveTrainError = true;
                        }

                        if (_raycastChild.Find("Rr") != null)
                        {
                            RightRearW = _wheelChild.Find("Rr").GetComponent<SwerveWheel>();
                        }
                        else
                        {
                            print("No Rr Wheel Object Detected");
                            _driveTrainError = true;
                        }

                        _driveTrainAssigned = true;
                    }

                    if (!_driveTrainError)
                    {
                        Vector3 driveInput = new Vector3(_translateValue.y, 0, _translateValue.x);

                        float angle;
                        if (!isRedRobot)
                        {
                            angle = transform.localRotation.eulerAngles.y + 270;
                        }
                        else
                        {
                            angle = transform.localRotation.eulerAngles.y + 90;
                        }


                        Vector3 fieldRelativeAngle = Quaternion.AngleAxis(angle, Vector3.up) * driveInput;

                        float fwd, str;

                        if (isFieldCentric)
                        {
                            if (!startingReversed)
                            {
                                fwd = fieldRelativeAngle.x * velocityMp;

                                str = fieldRelativeAngle.z * velocityMp;
                            } else
                            {
                                fwd = -fieldRelativeAngle.x * velocityMp;

                                str = -fieldRelativeAngle.z * velocityMp;
                            }
                        }
                        else
                        {
                            fwd = driveInput.x * velocityMp;

                            str = driveInput.z * velocityMp;
                        }


                        float RCW = -_steer2 * steerMultiplyer;

                        float L = LeftFront.localPosition.z - RightFront.localPosition.z;

                        float W = LeftFront.localPosition.x - RightFront.localPosition.x;

                        float R = Mathf.Sqrt(MathF.Pow(L, 2) + Mathf.Pow(W, 2));

                        float A = str - RCW * (L / R);
                        float B = str + RCW * (L / R);
                        float C = fwd - RCW * (W / R);
                        float D = fwd + RCW * (W / R);

                        float ws1 = Mathf.Sqrt(Mathf.Pow(B, 2) + Mathf.Pow(C, 2));
                        float wa1 = Mathf.Atan2(B, C) * 180 / Mathf.PI;

                        float ws2 = Mathf.Sqrt(Mathf.Pow(B, 2) + Mathf.Pow(D, 2));
                        float wa2 = Mathf.Atan2(B, D) * 180 / Mathf.PI;

                        float ws3 = Mathf.Sqrt(Mathf.Pow(A, 2) + Mathf.Pow(D, 2));
                        float wa3 = Mathf.Atan2(A, D) * 180 / Mathf.PI;

                        float ws4 = Mathf.Sqrt(Mathf.Pow(A, 2) + Mathf.Pow(C, 2));
                        float wa4 = Mathf.Atan2(A, C) * 180 / Mathf.PI;


                        if (Physics.Raycast(LeftFront.position, -transform.up, rayCastDistance))
                        {
                            LeftFront.localEulerAngles = new Vector3(0, wa2, 0);
                            _rb.AddForceAtPosition((LeftFront.forward * (ws2 * accelerationSpeed * _rb.mass) / 4), LeftFront.position);
                        }

                        if (Physics.Raycast(LeftRear.position, -transform.up, rayCastDistance))
                        {
                            LeftRear.localEulerAngles = new Vector3(0, wa3, 0);
                            _rb.AddForceAtPosition((LeftRear.forward * (ws3 * accelerationSpeed * _rb.mass)) / 4, LeftRear.position);
                        }

                        if (Physics.Raycast(RightFront.position, -transform.up, rayCastDistance))
                        {
                            RightFront.localEulerAngles = new Vector3(0, wa1, 0);
                            _rb.AddForceAtPosition((RightFront.forward * (ws1 * accelerationSpeed * _rb.mass)) / 4, RightFront.position);
                        }

                        if (Physics.Raycast(RightRear.position, -transform.up, rayCastDistance))
                        {
                            RightRear.localEulerAngles = new Vector3(0, wa4, 0);
                            _rb.AddForceAtPosition((RightRear.forward * (ws4 * accelerationSpeed * _rb.mass)) / 4, RightRear.position);
                        }

                            LeftFrontW.Wa = LeftFront.localRotation.eulerAngles.y;
                            LeftRearW.Wa = LeftRear.localRotation.eulerAngles.y;
                            RightFrontW.Wa = RightFront.localRotation.eulerAngles.y;
                            RightRearW.Wa = RightRear.localRotation.eulerAngles.y;
                        }

                    break;
                }
                default:
                    break;
            }
        }

        if (_flag) return;
        _rb.maxLinearVelocity = maxSpeed;
        _rb.maxAngularVelocity = maxSpeed / ((Mathf.Abs(LeftFront.transform.localPosition.x) + Mathf.Abs(RightRear.transform.localPosition.x)) * 2 * Mathf.PI) * (2*Mathf.PI);

    }

    private void PlaySwerveSounds()
    {
        float velocityFactor = Mathf.Clamp01(_rb.velocity.magnitude / _rb.maxLinearVelocity);
        float accelerationFactor = Mathf.Clamp(1f + (_rb.velocity.magnitude / _rb.maxLinearVelocity), 1f, 2f);

        float rotationFactor = Mathf.Clamp01(Mathf.Abs(_rb.angularVelocity.magnitude) / _rb.maxAngularVelocity);

        float volume = velocityFactor + (rotationFactor * 10);

        float pitch = Mathf.Max(accelerationFactor, rotationFactor);

        if (driveTrain == DriveTrain.Tank || driveTrain == DriveTrain.HDrive)
        {
            pitch -= 0.35f;
            _gearPlayer.pitch = 0.7f;
        }

        _treadPlayer.volume = volume * 0.8f;
        _treadPlayer.pitch = pitch * 0.7f;
        _gearPlayer.volume = volume * 0.5f;

        if (!_treadPlayer.isPlaying && !_gearPlayer.isPlaying)
        {
            _gearPlayer.Play();
            _treadPlayer.Play();
        }
    }

    private void StopSwerveSounds()
    {
        if (_treadPlayer.isPlaying || _gearPlayer.isPlaying)
        {
            _treadPlayer.Stop();
            _gearPlayer.Stop();
        }
    }

    public IEnumerator GrayOutBumpers(float duration)
    {
        _bumperMat.color = Color.gray;

        yield return new WaitForSeconds(duration);

        _bumperMat.color = _defaultBumperColor;
    }

    public void OnRestart(InputAction.CallbackContext ctx)
    {
        if (_gameManager != null && !_isReseting)
        {
            _isReseting = true;
            _gameManager.Reset();
        }
    }

    public void OnWalk(InputAction.CallbackContext ctx)
    {
        dsMove = ctx.ReadValue<Vector2>();
    }

    public void OnAlign(InputAction.CallbackContext ctx)
    {
        _onAlign = ctx.action.triggered;
    }

    public bool CheckGround()
    {
        float distanceToTheGround = rayCastDistance;
        foreach (Transform rayCastPoint in _raycastChild)
        {
            if (!flipRayCastDir)
            {
                if (Physics.Raycast(rayCastPoint.position, -transform.up, distanceToTheGround))
                {
                    return true;
                }
            }
            else
            {
                if (Physics.Raycast(rayCastPoint.position, transform.up, distanceToTheGround))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isRedRobot)
        {
            if (other.gameObject.CompareTag("RedPlayer"))
            {
                RobotsTouching = true;
            }
            else if (other.gameObject.CompareTag("Field") || other.gameObject.CompareTag("Wall"))
            {
                _dontUpdateBeforeVelocity = true;
                IsTouchingWallColliderBlue = true;
            }
        }
        else
        {
            if (other.gameObject.CompareTag("Player"))
            {
                RobotsTouching = true;
            }
            else if (other.gameObject.CompareTag("Field") || other.gameObject.CompareTag("Wall"))
            {
                _dontUpdateBeforeVelocity = true;
                IsTouchingWallColliderRed = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isRedRobot)
        {
            if (other.gameObject.CompareTag("RedPlayer"))
            {
                RobotsTouching = false;
            }
            else if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Field"))
            {
                _dontUpdateBeforeVelocity = false;
                if (!isRedRobot)
                {
                    IsTouchingWallColliderBlue = false;
                }
                else
                {
                    IsTouchingWallColliderRed = false;
                }
            }
        }
        else
        {
            if (other.gameObject.CompareTag("Player"))
            {
                RobotsTouching = false;
            }
            else if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Field"))
            {
                _dontUpdateBeforeVelocity = false;
                if (!isRedRobot)
                {
                    IsTouchingWallColliderBlue = false;
                }
                else
                {
                    IsTouchingWallColliderRed = false;
                }
            }
        }
    }
}