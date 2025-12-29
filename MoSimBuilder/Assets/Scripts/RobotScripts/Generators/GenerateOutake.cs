using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class GenerateOutake : MonoBehaviour
{
    private GameObject[] _borderVisuals = new GameObject[12];
    
    //begin visible
    [Header("Settings")]
    [SerializeField] private Vector3 outakeSize;
    
    [SerializeField] private float actionDelay;
    
    [SerializeField] private Direction outakeDirection;

    [Header("Outake Speeds")]
    [SerializeField] private float outakeSpeed;

    [SerializeField] private float sideSpin;

    [SerializeField] private float backSpin;

    
    //end visible

    [HideInInspector] public GameObject cubeLines;

    [HideInInspector] public bool hasObject;

    [HideInInspector] public GamePieceScript gamePiece;
    
    [HideInInspector] public bool ejected;

    private void Start()
    {
        Startup();
    }

    private void Awake()
    {
        Startup();
    }

    // Start is called before the first frame update
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
                    _borderVisuals[0].transform.localPosition = new Vector3(0, outakeSize.y/2*0.0254f, outakeSize.z/2*0.0254f);
                    _borderVisuals[1].transform.localPosition = new Vector3(0, -outakeSize.y/2*0.0254f, outakeSize.z/2*0.0254f);
                    _borderVisuals[2].transform.localPosition = new Vector3(0, outakeSize.y/2*0.0254f, -outakeSize.z/2*0.0254f);
                    _borderVisuals[3].transform.localPosition = new Vector3(0, -outakeSize.y/2*0.0254f, -outakeSize.z/2*0.0254f);
                
                    _borderVisuals[0].transform.localScale = new Vector3(outakeSize.x*0.0254f,0.1f*0.0254f, 0.1f*0.0254f);
                    _borderVisuals[1].transform.localScale = new Vector3(outakeSize.x*0.0254f,0.1f*0.0254f, 0.1f*0.0254f);
                    _borderVisuals[2].transform.localScale = new Vector3(outakeSize.x*0.0254f,0.1f*0.0254f, 0.1f*0.0254f);
                    _borderVisuals[3].transform.localScale = new Vector3(outakeSize.x*0.0254f,0.1f*0.0254f, 0.1f*0.0254f);
                
                    _borderVisuals[4].transform.localPosition = new Vector3(outakeSize.x/2*0.0254f, 0, outakeSize.z/2*0.0254f);
                    _borderVisuals[5].transform.localPosition = new Vector3(-outakeSize.x/2*0.0254f, 0, outakeSize.z/2*0.0254f);
                    _borderVisuals[6].transform.localPosition = new Vector3(outakeSize.x/2*0.0254f, 0,  -outakeSize.z/2*0.0254f);
                    _borderVisuals[7].transform.localPosition = new Vector3(-outakeSize.x/2*0.0254f, 0, -outakeSize.z/2*0.0254f);
                
                    _borderVisuals[4].transform.localScale = new Vector3(0.1f*0.0254f, outakeSize.y*0.0254f,0.1f*0.0254f);
                    _borderVisuals[5].transform.localScale = new Vector3(0.1f*0.0254f, outakeSize.y*0.0254f,0.1f*0.0254f);
                    _borderVisuals[6].transform.localScale = new Vector3(0.1f*0.0254f, outakeSize.y*0.0254f,0.1f*0.0254f);
                    _borderVisuals[7].transform.localScale = new Vector3(0.1f*0.0254f, outakeSize.y*0.0254f,0.1f*0.0254f);
                
                    _borderVisuals[8].transform.localPosition = new Vector3(outakeSize.x/2*0.0254f, outakeSize.y/2*0.0254f, 0);
                    _borderVisuals[9].transform.localPosition = new Vector3(-outakeSize.x/2*0.0254f, outakeSize.y/2*0.0254f, 0);
                    _borderVisuals[10].transform.localPosition = new Vector3(outakeSize.x/2*0.0254f,  -outakeSize.y/2*0.0254f, 0);
                    _borderVisuals[11].transform.localPosition = new Vector3(-outakeSize.x/2*0.0254f,  -outakeSize.y/2*0.0254f, 0);
                
                    _borderVisuals[8].transform.localScale = new Vector3(0.1f*0.0254f, 0.1f*0.0254f, outakeSize.z*0.0254f);
                    _borderVisuals[9].transform.localScale = new Vector3(0.1f*0.0254f, 0.1f*0.0254f, outakeSize.z*0.0254f);
                    _borderVisuals[10].transform.localScale = new Vector3(0.1f*0.0254f, 0.1f*0.0254f, outakeSize.z*0.0254f);
                    _borderVisuals[11].transform.localScale = new Vector3(0.1f*0.0254f, 0.1f*0.0254f, outakeSize.z*0.0254f);
                }
            }
        }
        else
        {
            if (hasObject && !ejected)
            {
                StartCoroutine(gamePiece.ReleaseToWorld(outakeSpeed, sideSpin, backSpin, actionDelay, this, outakeDirection));
                ejected = true;
            }
            else if (hasObject)
            {
                gamePiece.MoveToPose(transform);
            }
        }
    }
    
    private void Startup()
    {
        if (transform.childCount != 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                _borderVisuals[i] = transform.GetChild(i).gameObject;
            }
        }

        ejected = false;
    }
}
