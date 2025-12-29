using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class SpawnGamePiece : MonoBehaviour
{
    [SerializeField] private Vector3 spawnVelocity;
    
    [SerializeField] private GameObject gamePiece;
    
    [SerializeField] private Transform spawnPoint;
    
    [SerializeField] private SpawnType spawnType;
    
    [SerializeField] private Collider spawnCollider;

    [SerializeField] private int threshold;

    [SerializeField] private float spawnTimer = 2.0f;

    [SerializeField] private bool usePerformanceMode;
    
    private GameObject[] _gamePieces;

    private bool flag;

    private float timer;

    private float _thresholdCounter;
    
    // Start is called before the first frame update
    void Start()
    {
        flag = false;
        
        if (gamePiece == null)
        {
            flag = true;
            print("Game Piece Spawner has no Game Piece selected");
        }

        if (spawnCollider == null)
        {
            flag = true;
            print("Game Piece Spawner has no Collider selected");
        }

        if (spawnPoint == null)
        {
            flag = true;
            print("Game Piece Spawner has no Spawn Point selected");
        }

        if (threshold == 0 && spawnType == SpawnType.PieceThreshold)
        {
            print("Game Piece Spawner threshold of zero will not spawn anything");
            flag = true;
        }

        if (threshold >= 25 && spawnType == SpawnType.PieceThreshold)
        {
            print("Game Piece Spawner thresholds of 25 or greater are recommended against");
        }

        _gamePieces = new GameObject[threshold + 4];

        timer = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (flag) return;

        if (spawnType == SpawnType.RobotDetect)
        {
            Collider[] colliders = Physics.OverlapBox(spawnCollider.transform.position, spawnCollider.bounds.extents/1.5f, spawnCollider.transform.rotation);
            foreach (Collider coll in colliders)
            {
                if (coll.gameObject.layer == LayerMask.NameToLayer("Robot"))
                {
                    if (timer < 0)
                    {
                       var go = Instantiate(gamePiece, spawnPoint.position, spawnPoint.rotation);
                       go.GetComponent<Rigidbody>().velocity = transform.TransformVector(spawnVelocity);
                       
                       if (usePerformanceMode)
                       {
                           go.GetComponent<GamePieceScript>().lowPerformanceMode = true;
                       }
                    }

                    timer = spawnTimer;
                }
                else
                {
                    timer -= Time.deltaTime;
                }
            }
        } else if (spawnType == SpawnType.PieceThreshold)
        {

            _thresholdCounter = 0;
            
            for (int i = 0; i < _gamePieces.Length; i++)
            {
                if (_gamePieces[i] != null)
                {
                    _thresholdCounter += 1;
                }
            }

            if (_thresholdCounter < threshold && timer < 0 )
            {
                timer = spawnTimer;
                
                var go = Instantiate(gamePiece, spawnPoint.position, spawnPoint.rotation);

                if (usePerformanceMode)
                {
                    go.GetComponent<GamePieceScript>().lowPerformanceMode = true;
                }
                
                go.GetComponent<Rigidbody>().velocity = transform.TransformVector(spawnVelocity);
                
                
            } else if (_thresholdCounter == threshold)
            {
                timer = spawnTimer;
            }
            else
            {
                timer -= Time.deltaTime;
            }
            
            for (int i = 0; i < _gamePieces.Length; i++)
            {
                _gamePieces[i] = null;
            }
            
            Collider[] colliders = Physics.OverlapBox(spawnCollider.transform.position, spawnCollider.bounds.extents/2, spawnCollider.transform.rotation);
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
}