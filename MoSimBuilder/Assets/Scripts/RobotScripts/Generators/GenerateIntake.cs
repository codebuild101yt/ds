using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[ExecuteAlways]
public class GenerateIntake : MonoBehaviour
{
    private BoxCollider _intake;

    private Collider intakeCollider;
    
    private GameObject[] _borderVisuals = new GameObject[12];
    // visible section
    
    [Header("settigns")]
    
    [SerializeField] private Vector3 intakeSize;
    
    [SerializeField] private IntakeType intakeType;

    [SerializeField] private float actionDelay;
    
    [Header("Game Piece Settings")]
    [SerializeField] private GamePieces[] intakesGamePieces;

    [Header("Controls and output")]
    
    [SerializeField] private GenerateStow transferToStow;
    
    [SerializeField] private Buttons button;
    
    //end visible section
    
    [HideInInspector] public GameObject cubeLines;
    
    private GameObject[] _gamePieces;
    
    private PlayerInput _playerInput;
    
    private InputActionMap _inputMap;

    private bool _hasGamePiece;
    
    private GameObject _gamePiece;

    
    private void Start()
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
        if (!EditorApplication.isPlaying)
        {
            if (_intake == null)
            {
               _intake = gameObject.AddComponent<BoxCollider>();
               _intake.isTrigger = true;
               _intake.size = intakeSize * 0.0254f;
            }

            _intake.size = intakeSize * 0.0254f;
            
            if (_borderVisuals == null)
            {
                _borderVisuals = new GameObject[12];
            }
            else
            {
                for (int i = 0; i < _borderVisuals.Length; i++)
                {
                    if (_borderVisuals[i] == null)
                    {
                        foreach (var t in _borderVisuals)
                        {
                            if (t != null)
                            {
                                DestroyImmediate(t);
                            }
                        }
                        _borderVisuals = new GameObject[12];
                        for (int t = 0; t < 12; t++)
                        {
                            _borderVisuals[t] = Instantiate(cubeLines, transform.position, transform.rotation, transform);
                        }
                    } 
                }
                
                if (_borderVisuals != null)
                {
                    _borderVisuals[0].transform.localPosition = new Vector3(0, intakeSize.y/2*0.0254f, intakeSize.z/2*0.0254f);
                    _borderVisuals[1].transform.localPosition = new Vector3(0, -intakeSize.y/2*0.0254f, intakeSize.z/2*0.0254f);
                    _borderVisuals[2].transform.localPosition = new Vector3(0, intakeSize.y/2*0.0254f, -intakeSize.z/2*0.0254f);
                    _borderVisuals[3].transform.localPosition = new Vector3(0, -intakeSize.y/2*0.0254f, -intakeSize.z/2*0.0254f);
                
                    _borderVisuals[0].transform.localScale = new Vector3(intakeSize.x*0.0254f,0.1f*0.0254f, 0.1f*0.0254f);
                    _borderVisuals[1].transform.localScale = new Vector3(intakeSize.x*0.0254f,0.1f*0.0254f, 0.1f*0.0254f);
                    _borderVisuals[2].transform.localScale = new Vector3(intakeSize.x*0.0254f,0.1f*0.0254f, 0.1f*0.0254f);
                    _borderVisuals[3].transform.localScale = new Vector3(intakeSize.x*0.0254f,0.1f*0.0254f, 0.1f*0.0254f);
                
                    _borderVisuals[4].transform.localPosition = new Vector3(intakeSize.x/2*0.0254f, 0, intakeSize.z/2*0.0254f);
                    _borderVisuals[5].transform.localPosition = new Vector3(-intakeSize.x/2*0.0254f, 0, intakeSize.z/2*0.0254f);
                    _borderVisuals[6].transform.localPosition = new Vector3(intakeSize.x/2*0.0254f, 0,  -intakeSize.z/2*0.0254f);
                    _borderVisuals[7].transform.localPosition = new Vector3(-intakeSize.x/2*0.0254f, 0, -intakeSize.z/2*0.0254f);
                
                    _borderVisuals[4].transform.localScale = new Vector3(0.1f*0.0254f, intakeSize.y*0.0254f,0.1f*0.0254f);
                    _borderVisuals[5].transform.localScale = new Vector3(0.1f*0.0254f, intakeSize.y*0.0254f,0.1f*0.0254f);
                    _borderVisuals[6].transform.localScale = new Vector3(0.1f*0.0254f, intakeSize.y*0.0254f,0.1f*0.0254f);
                    _borderVisuals[7].transform.localScale = new Vector3(0.1f*0.0254f, intakeSize.y*0.0254f,0.1f*0.0254f);
                
                    _borderVisuals[8].transform.localPosition = new Vector3(intakeSize.x/2*0.0254f, intakeSize.y/2*0.0254f, 0);
                    _borderVisuals[9].transform.localPosition = new Vector3(-intakeSize.x/2*0.0254f, intakeSize.y/2*0.0254f, 0);
                    _borderVisuals[10].transform.localPosition = new Vector3(intakeSize.x/2*0.0254f,  -intakeSize.y/2*0.0254f, 0);
                    _borderVisuals[11].transform.localPosition = new Vector3(-intakeSize.x/2*0.0254f,  -intakeSize.y/2*0.0254f, 0);
                
                    _borderVisuals[8].transform.localScale = new Vector3(0.1f*0.0254f, 0.1f*0.0254f, intakeSize.z*0.0254f);
                    _borderVisuals[9].transform.localScale = new Vector3(0.1f*0.0254f, 0.1f*0.0254f, intakeSize.z*0.0254f);
                    _borderVisuals[10].transform.localScale = new Vector3(0.1f*0.0254f, 0.1f*0.0254f, intakeSize.z*0.0254f);
                    _borderVisuals[11].transform.localScale = new Vector3(0.1f*0.0254f, 0.1f*0.0254f, intakeSize.z*0.0254f);
                }
            }
        }
        else
        {
            bool intakeActive = intakeType == IntakeType.always;
            if (_gamePieces[0] != null && (_inputMap.FindAction(button.ToString()).IsPressed() || intakeActive) && !transferToStow.hasObject && !_hasGamePiece)
            {
                for (var i = 0; i < intakesGamePieces.Length; i++)
                {
                    if (intakesGamePieces[i] ==
                        _gamePieces[0].transform.parent.parent.GetComponent<GamePieceScript>().gamePiece)
                    {
                        _gamePieces[0].transform.parent.parent.GetComponent<GamePieceScript>().MoveToPose(transform);
                        StartCoroutine(_gamePieces[0].transform.parent.parent.GetComponent<GamePieceScript>().TransferObject(transferToStow.transform, actionDelay));
                        _hasGamePiece = true;
                        _gamePiece = _gamePieces[0];
                    }
                }
            }
            else
            {
                if (_hasGamePiece)
                { 
                    _gamePiece.transform.parent.parent.GetComponent<GamePieceScript>().MoveToPose(transform);
                }
            }
            
            for (int i = 0; i < _gamePieces.Length; i++)
            {
                _gamePieces[i] = null;
            }
            
            Collider[] colliders = Physics.OverlapBox(transform.position, (intakeSize*0.0254f)/4, transform.rotation);
            foreach (Collider coll in colliders)
            {
                if (coll.transform.parent.parent != null)
                {
                    if (coll.transform.parent.parent.GetComponent<GamePieceScript>() != null)
                    {

                        for (int i = 0; i < _gamePieces.Length; i++)
                        {
                            if (_gamePieces[i] == null)
                            {
                                _gamePieces[i] = coll.gameObject;
                                return;
                            }
                            else if (_gamePieces[i] == coll.gameObject)
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    public void GetGamePiece()
    {
        _gamePiece = null;
        _hasGamePiece = false;
    }

    private void Startup()
    {
        _gamePieces = new GameObject[4];
        
        intakeCollider = gameObject.GetComponent<Collider>();
        
        if (transform.childCount != 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                _borderVisuals[i] = transform.GetChild(i).gameObject;
            }
        }

        if (transform.GetComponent<BoxCollider>())
        {
            _intake = transform.GetComponent<BoxCollider>();
        }
        
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
    }
}
