using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class GenerateHook : MonoBehaviour
{
    private GameObject _hookStem;
    private GameObject _hookBridge;
    private GameObject _hookClaw;
    //begin visible section
    [Header("Global Settinsg")]
    [SerializeField] private Units units;
    [SerializeField] private float hookWidth = 0.5f;
    [Header("Stem Settings")]
    [SerializeField] private float hookStemHeight = 3f;
    [SerializeField] private float hookStemDepth = 0.5f;
    [Header("Bridge Settings")]
    [SerializeField] private float hookBridgeLength = 2f;
    [SerializeField] private float hookBridgeHeight = 0.35f;
    [Header("Claw Settings")]
    [SerializeField] private float hookClawHeight = 1f;
    [SerializeField] private float hookClawDepth = 0.5f;
    //end Visible Section

    private float _multiplier;

    private void Awake()
    {
        Startup();
    }

    // Start is called before the first frame update
    void Start()
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
            if (_hookStem == null)
            {
                _hookStem = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _hookStem.name = "HookStem";
                _hookStem.transform.parent = transform;
                _hookStem.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            _hookStem.transform.localRotation = Quaternion.Euler(0, 0, 0);
            _hookStem.layer = LayerMask.NameToLayer("Robot");
            _hookStem.transform.localScale = new Vector3(hookWidth*_multiplier, hookStemHeight*_multiplier, hookStemDepth*_multiplier);
            _hookStem.transform.localPosition = new Vector3(0, hookStemHeight*_multiplier * 0.5f, 0);

            if (_hookBridge == null)
            {
                _hookBridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _hookBridge.name = "HookBridge";
                _hookBridge.transform.parent = transform;
                _hookBridge.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            _hookBridge.transform.localRotation = Quaternion.Euler(0, 0, 0);
            _hookBridge.transform.localScale = new Vector3(hookWidth*_multiplier, hookBridgeHeight*_multiplier, hookBridgeLength*_multiplier);
            _hookBridge.transform.localPosition = new Vector3(0, (hookStemHeight*_multiplier) + (hookBridgeHeight*0.5f*_multiplier), (hookBridgeLength*_multiplier*0.5f)-(hookStemDepth*0.5f*_multiplier));
            _hookBridge.layer = LayerMask.NameToLayer("Robot");
            
            if (_hookClaw == null)
            {
                _hookClaw = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _hookClaw.name = "HookClaw";
                _hookClaw.transform.parent = transform;
                _hookClaw.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            _hookClaw.transform.localRotation = Quaternion.Euler(0, 0, 0);
            _hookClaw.transform.localScale = new Vector3(hookWidth*_multiplier, hookClawHeight*_multiplier, hookClawDepth*_multiplier);
            _hookClaw.transform.localPosition = new Vector3(0, (hookStemHeight*_multiplier)-(hookClawHeight*_multiplier*0.5f), (hookBridgeLength*_multiplier) - (hookClawDepth*_multiplier*0.5f)-(hookStemDepth*0.5f*_multiplier));
            _hookClaw.layer = LayerMask.NameToLayer("Robot");
        }
        else
        {
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
        
        if (transform.Find("HookStem"))
        {
            _hookStem = transform.Find("HookStem").gameObject;
        }

        if (transform.Find("HookBridge"))
        {
            _hookBridge = transform.Find("HookBridge").gameObject;
        }

        if (transform.Find("HookClaw"))
        {
            _hookClaw = transform.Find("HookClaw").gameObject;
        }
    }
}
