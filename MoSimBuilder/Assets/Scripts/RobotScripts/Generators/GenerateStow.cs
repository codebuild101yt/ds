using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[ExecuteAlways]
public class GenerateStow : MonoBehaviour
{

    private GameObject[] _borderVisuals = new GameObject[12];

    [HideInInspector]
    public bool hasObject;
    [HideInInspector]
    public GamePieceScript GamePiece;
    
    [HideInInspector] public GameObject cubeLines;
    
    //visible section
    [Header("Settings")]
    [FormerlySerializedAs("intakeSize")] [SerializeField] private Vector3 stowSize;
    
    [SerializeField] private TransferType transferType;
    
    [SerializeField] private float actionDelay;
    
    [Header("Controls and Interactions")]
    
    [SerializeField] private GameObject[] endpoints;
    
    [SerializeField] private Buttons[] transferButton;
    
    [SerializeField] private int[] pressesToTransfer;
    
    // end visible section
    
    private Buttons[] _buttonBuffer;
    
    private int[] _pressesToTransferBuffer;
    
    private PlayerInput _playerInput;
    
    private InputActionMap _inputMap;
    
    private Buttons[] _lastButton;

    private float _sequenceBuffer;

    [HideInInspector] public bool transfering;
    
    
    // Start is called before the first frame update
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
                    _borderVisuals[0].transform.localPosition = new Vector3(0, stowSize.y/2*0.0254f, stowSize.z/2*0.0254f);
                    _borderVisuals[1].transform.localPosition = new Vector3(0, -stowSize.y/2*0.0254f, stowSize.z/2*0.0254f);
                    _borderVisuals[2].transform.localPosition = new Vector3(0, stowSize.y/2*0.0254f, -stowSize.z/2*0.0254f);
                    _borderVisuals[3].transform.localPosition = new Vector3(0, -stowSize.y/2*0.0254f, -stowSize.z/2*0.0254f);
                
                    _borderVisuals[0].transform.localScale = new Vector3(stowSize.x*0.0254f,0.1f*0.0254f, 0.1f*0.0254f);
                    _borderVisuals[1].transform.localScale = new Vector3(stowSize.x*0.0254f,0.1f*0.0254f, 0.1f*0.0254f);
                    _borderVisuals[2].transform.localScale = new Vector3(stowSize.x*0.0254f,0.1f*0.0254f, 0.1f*0.0254f);
                    _borderVisuals[3].transform.localScale = new Vector3(stowSize.x*0.0254f,0.1f*0.0254f, 0.1f*0.0254f);
                
                    _borderVisuals[4].transform.localPosition = new Vector3(stowSize.x/2*0.0254f, 0, stowSize.z/2*0.0254f);
                    _borderVisuals[5].transform.localPosition = new Vector3(-stowSize.x/2*0.0254f, 0, stowSize.z/2*0.0254f);
                    _borderVisuals[6].transform.localPosition = new Vector3(stowSize.x/2*0.0254f, 0,  -stowSize.z/2*0.0254f);
                    _borderVisuals[7].transform.localPosition = new Vector3(-stowSize.x/2*0.0254f, 0, -stowSize.z/2*0.0254f);
                
                    _borderVisuals[4].transform.localScale = new Vector3(0.1f*0.0254f, stowSize.y*0.0254f,0.1f*0.0254f);
                    _borderVisuals[5].transform.localScale = new Vector3(0.1f*0.0254f, stowSize.y*0.0254f,0.1f*0.0254f);
                    _borderVisuals[6].transform.localScale = new Vector3(0.1f*0.0254f, stowSize.y*0.0254f,0.1f*0.0254f);
                    _borderVisuals[7].transform.localScale = new Vector3(0.1f*0.0254f, stowSize.y*0.0254f,0.1f*0.0254f);
                
                    _borderVisuals[8].transform.localPosition = new Vector3(stowSize.x/2*0.0254f, stowSize.y/2*0.0254f, 0);
                    _borderVisuals[9].transform.localPosition = new Vector3(-stowSize.x/2*0.0254f, stowSize.y/2*0.0254f, 0);
                    _borderVisuals[10].transform.localPosition = new Vector3(stowSize.x/2*0.0254f,  -stowSize.y/2*0.0254f, 0);
                    _borderVisuals[11].transform.localPosition = new Vector3(-stowSize.x/2*0.0254f,  -stowSize.y/2*0.0254f, 0);
                
                    _borderVisuals[8].transform.localScale = new Vector3(0.1f*0.0254f, 0.1f*0.0254f, stowSize.z*0.0254f);
                    _borderVisuals[9].transform.localScale = new Vector3(0.1f*0.0254f, 0.1f*0.0254f, stowSize.z*0.0254f);
                    _borderVisuals[10].transform.localScale = new Vector3(0.1f*0.0254f, 0.1f*0.0254f, stowSize.z*0.0254f);
                    _borderVisuals[11].transform.localScale = new Vector3(0.1f*0.0254f, 0.1f*0.0254f, stowSize.z*0.0254f);
                }
            }
            
            if (transferButton != null)
            {
                if (transferButton.Length != endpoints.Length)
                {
                    _buttonBuffer = transferButton;

                    transferButton = new Buttons[endpoints.Length];
                    for (int i = 0; i < _buttonBuffer.Length; i++)
                    {
                        if (i < endpoints.Length && i < _buttonBuffer.Length)
                        {
                            transferButton[i] = _buttonBuffer[i];
                        }
                    }
                }
            }
            else
            {
                endpoints = new GameObject[1];
                transferButton = new Buttons[endpoints.Length];
            }
            
            if (pressesToTransfer != null)
            {
                if (pressesToTransfer.Length != endpoints.Length)
                {
                    _pressesToTransferBuffer = pressesToTransfer;

                    pressesToTransfer = new int[endpoints.Length];
                    
                    for (int i = 0; i < pressesToTransfer.Length; i++)
                    {
                        pressesToTransfer[i] = 1;
                    }
                    
                    for (int i = 0; i < _pressesToTransferBuffer.Length; i++)
                    {
                        if (i < pressesToTransfer.Length && i < _pressesToTransferBuffer.Length)
                        {
                            pressesToTransfer[i] = _pressesToTransferBuffer[i];
                        }
                    }
                    
                    
                }
            }
            else
            {
                pressesToTransfer = new int[endpoints.Length];
                for (int i = 0; i < pressesToTransfer.Length; i++)
                {
                    pressesToTransfer[i] = 1;
                }
            }

            for (int i = 0; i < endpoints.Length; i++)
            {
                if (endpoints[i] != null)
                {
                    if (endpoints[i].gameObject.GetComponent<GenerateStow>() == null && endpoints[i].gameObject.GetComponent<GenerateOutake>() == null)
                    {
                        endpoints[i] = null;
                    }
                }
            }
        }
        else
        {
            if (hasObject)
            {
                GamePiece.MoveToPose(transform);
            }

            for (int i = 0; i < endpoints.Length; i++)
            {
                if (_inputMap.FindAction(transferButton[i].ToString()).IsPressed() && transferType == TransferType.button && hasObject && (_sequenceBuffer >= pressesToTransfer[i]) && !transfering)
                { 
                    if (endpoints[i].GetComponent<GenerateStow>())
                    {
                        var target = endpoints[i].GetComponent<GenerateStow>();
                        
                        if (target.hasObject) return;
                        
                        StartCoroutine(GamePiece.TransferObject(endpoints[i].transform, actionDelay));
                        
                        transfering = true;
                        
                    } else if (endpoints[i].GetComponent<GenerateOutake>())
                    {
                        var target = endpoints[i].GetComponent<GenerateOutake>();
                        
                        if (target.hasObject) return;
                        
                        StartCoroutine(GamePiece.TransferObject(endpoints[i].transform, actionDelay));
                        
                        transfering = true;

                    }
                } else if (transferType == TransferType.instant && hasObject && !transfering)
                {
                    if (endpoints[i].GetComponent<GenerateStow>())
                    {
                        var target = endpoints[i].GetComponent<GenerateStow>();
                        
                        if (target.hasObject) return;
                        
                        StartCoroutine(GamePiece.TransferObject(endpoints[i].transform, actionDelay));
                        
                        transfering = true;

                    } else if (endpoints[i].GetComponent<GenerateOutake>())
                    {
                        var target = endpoints[i].GetComponent<GenerateOutake>();
                        
                        if (target.hasObject) return;
                        
                        
                        StartCoroutine(GamePiece.TransferObject(endpoints[i].transform, actionDelay));
                        
                        transfering = true;

                    }
                }
            }

            for (int i = 0; i < transferButton.Length; i++)
            {
                if (_inputMap.FindAction(transferButton[i].ToString()).triggered)
                {
                    if (_lastButton == null)
                    {
                        _lastButton = new Buttons[1];
                        _lastButton[0] = transferButton[i];
                        _sequenceBuffer += 1;
                    }
                    else
                    {
                        if (_lastButton[0] == transferButton[i])
                        {
                            _sequenceBuffer += 1;
                        }
                        else
                        {
                            _sequenceBuffer = 1;
                        }
                    }
                    
                    _lastButton[0] = transferButton[i];
                }

                if (!hasObject)
                {
                    _sequenceBuffer = 0;
                    _lastButton = null;
                }
            }
        }
    }

    private void Startup()
    {
        _sequenceBuffer = 0;

        transfering = false;
        
        if (transform.childCount != 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                _borderVisuals[i] = transform.GetChild(i).gameObject;
            }
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
