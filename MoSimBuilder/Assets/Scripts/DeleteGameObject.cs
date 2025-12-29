using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class DeleteGameObject : MonoBehaviour
{
    [SerializeField] private Collider intakeCollider;
    
    private GameObject[] _gamePieces = new GameObject[4];

    private bool flag;
    
    // Start is called before the first frame update
    void Start()
    {
        flag = false;
        
        if (intakeCollider == null)
        {
            flag = true;
            print("Game Piece Delete Collider is null");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (flag) return;
        
        foreach (var t in _gamePieces)
        {
            if (t != null)
            {
                t.transform.parent.parent.GetComponent<GamePieceScript>().Destroy();
            }
        }
        
        for (int i = 0; i < _gamePieces.Length; i++)
        {
            _gamePieces[i] = null;
        }
            
        Collider[] colliders = Physics.OverlapBox(intakeCollider.transform.position, intakeCollider.bounds.extents/2, intakeCollider.transform.rotation);
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
