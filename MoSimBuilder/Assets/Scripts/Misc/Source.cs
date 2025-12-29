using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Source : MonoBehaviour
{
    public Collider collider;
    public Transform spawnPoint;
    private GameObject piece;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Collider[] colliders = Physics.OverlapBox(collider.bounds.center, collider.bounds.extents, Quaternion.identity);
        foreach (Collider collider in colliders)
        {
            if (collider.tag == "BlueCargo" || collider.tag == "RedCargo")
            {
                piece = collider.gameObject;
                piece.transform.position = spawnPoint.position;
                piece.GetComponent<Rigidbody>().velocity = new Vector3();
                piece.GetComponent<Rigidbody>().angularVelocity = new Vector3();
            }
        }
    }
}
