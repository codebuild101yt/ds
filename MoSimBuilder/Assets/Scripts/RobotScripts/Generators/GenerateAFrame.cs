using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[ExecuteAlways]
public class GenerateAFrame : MonoBehaviour
{
    private GameObject _AFrameUpright;
    private GameObject _AFrame;
    
    //begin visible section
    [SerializeField] private Units units;
    [SerializeField] private float AFrameHeight = 4;
    [SerializeField] private float TubeHeight = 1;
    [SerializeField] private float AFrameWidth = 1;
    [SerializeField] private float AFrameLength = 4;
    //end visible section
    
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
            if (_AFrameUpright == null)
            {
                _AFrameUpright = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _AFrameUpright.name = "AFrameUpright";
                _AFrameUpright.transform.parent = transform;
                _AFrameUpright.transform.localRotation = new Quaternion();
            }
            _AFrameUpright.transform.localScale = new Vector3(AFrameWidth*_multiplier, AFrameHeight*_multiplier, TubeHeight*_multiplier);
            _AFrameUpright.transform.localPosition = new Vector3(0, (AFrameHeight*_multiplier)/2, 0);
            _AFrameUpright.layer = LayerMask.NameToLayer("Robot");

            if (_AFrame == null)
            {
                _AFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _AFrame.name = "AFrame";
                _AFrame.transform.parent = transform;
            }

            _AFrame.transform.localPosition = new Vector3(0, AFrameHeight*0.5f*_multiplier, ((-TubeHeight* _multiplier)/5) + (AFrameLength*0.5f*_multiplier));
            _AFrame.transform.localRotation = Quaternion.Euler(Mathf.Atan2(AFrameHeight, AFrameLength)*57.2958f, 0, 0);
            _AFrame.transform.localScale = new Vector3(AFrameWidth*_multiplier, TubeHeight*_multiplier, TubeHeight*_multiplier -(_multiplier * Mathf.Sqrt((AFrameHeight*AFrameHeight)+(AFrameLength*AFrameLength))));
            _AFrame.layer = LayerMask.NameToLayer("Robot");
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
        
        if (transform.Find("AFrameUpright"))
        {
            _AFrameUpright = transform.Find("AFrameUpright").gameObject;
        }

        if (transform.Find("AFrame"))
        {
            _AFrame = transform.Find("AFrame").gameObject;
        }
    }
}
