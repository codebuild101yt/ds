using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class FieldLoader : MonoBehaviour
{
    private GameObject _field;
    private GameObject loadedField;
    [SerializeField] private GameObject[] fields;
    
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
        if (!EditorApplication.isPlaying)
        {
            if (_field == null)
            {
                _field = new GameObject("field");
                _field.transform.parent = transform;
            }

            if (loadedField.name != fields[0].name && loadedField != null)
            {
                DestroyImmediate(_field);
            }
            
            if (_field == null)
            {
                _field = new GameObject("field");
                _field.transform.parent = transform;
            }

            if (loadedField == null && _field != null)
            {
                loadedField = Instantiate(fields[0], _field.transform, true);
                loadedField.name = (fields[0].name);
            }
        }
    }

    private void Startup()
    {
        if (transform.Find("field"))
        {
            _field = transform.Find("field").gameObject;
        }

        if (_field.transform.Find(fields[0].name))
        {
            loadedField = _field.transform.Find(fields[0].name).gameObject;
        }
    }
}
