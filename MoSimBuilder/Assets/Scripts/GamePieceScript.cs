using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GamePieceScript : MonoBehaviour
{
    [SerializeField] private GameObject colliderParent;

    [SerializeField] private Rigidbody rb;
    
    public GamePieces gamePiece;

    public bool lowPerformanceMode;
    
    // Start is called before the first frame update
    private void Start()
    {
        if (lowPerformanceMode)
        {
            rb.solverIterations = 1;
            rb.solverVelocityIterations = 1;
        }
        
    }

    public IEnumerator ReleaseToWorld(float vel, float sideSpin, float backSpin, float actionDelay, GenerateOutake outakePoint, Direction direction)
    {
        yield return new WaitForSeconds(actionDelay);
        
        outakePoint.hasObject = false;
        outakePoint.gamePiece = null;
        outakePoint.ejected = false;

        
            rb.useGravity = true;
            rb.isKinematic = false;
            switch (direction)
            {
                case Direction.forward:
                    rb.velocity = transform.forward.normalized * vel;
                    break;
                case Direction.up:
                    rb.velocity = transform.up.normalized * vel;
                    break;
                default:
                    rb.velocity = transform.forward.normalized * vel;
                    break;
            }
            
            rb.angularVelocity = transform.TransformDirection(new Vector3(-backSpin, sideSpin, 0));

            transform.parent = transform.root.parent;

            EnableColliders();
            StartCoroutine(IgnoreRobot());
    }

    public void MoveToPose(Transform pos)
    {
        rb.useGravity = false;
        rb.isKinematic = true;
        DisableColliders();
        gameObject.transform.position = pos.position;
        gameObject.transform.rotation = pos.rotation;
        gameObject.transform.parent = pos;
    }

    public IEnumerator TransferObject(Transform pos, float actionDelay)
    {
        yield return new WaitForSeconds(actionDelay);
        if (transform.parent.GetComponent<GenerateIntake>())
        {
            transform.parent.GetComponent<GenerateIntake>().GetGamePiece();
            
        } else if (transform.parent.GetComponent<GenerateStow>())
        {
            transform.parent.GetComponent<GenerateStow>().hasObject = false;
            transform.parent.GetComponent<GenerateStow>().GamePiece = null;
            transform.parent.GetComponent<GenerateStow>().transfering = false;
        }
        
        rb.useGravity = false;
        rb.isKinematic = true;
        DisableColliders();
        gameObject.transform.position = pos.position;
        gameObject.transform.rotation = pos.rotation;
        gameObject.transform.parent = pos;
        if (pos.gameObject.GetComponent<GenerateStow>())
        {
            pos.gameObject.GetComponent<GenerateStow>().hasObject = true;
            pos.gameObject.GetComponent<GenerateStow>().GamePiece = this;
        } else if (pos.gameObject.GetComponent<GenerateOutake>())
        {
            pos.gameObject.GetComponent<GenerateOutake>().hasObject = true;
            pos.gameObject.GetComponent<GenerateOutake>().gamePiece = this;
        }
    }

    private void DisableColliders()
    {
        colliderParent.SetActive(false);
    }

    private void EnableColliders()
    {
        colliderParent.SetActive(true);
    }

    private IEnumerator IgnoreRobot()
    {
        rb.excludeLayers = LayerMask.GetMask("Robot");
        
        yield return new WaitForSeconds(0.1f);

        rb.excludeLayers = new LayerMask();
    }

    public void Destroy()
    {
        DestroyImmediate(gameObject);
    }

    
    
}
