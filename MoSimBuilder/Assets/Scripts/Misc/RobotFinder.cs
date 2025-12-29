using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotFinder : MonoBehaviour
{
    public GameObject[] blueRobots;
    public GameObject[] rebRobots;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Player2"))
        {
            if (blueRobots.Length > 0)
            {
                for (var i = 0; i < blueRobots.Length; i++)
                {
                    if (blueRobots[i] != other.gameObject && blueRobots[i] == null)
                    {
                        blueRobots[i] = other.gameObject;
                        return;
                    }
                    else if (blueRobots[i] == other.gameObject)
                    {
                        return;
                    }
                }
            }
            else
            {
                {
                    blueRobots[0] = other.gameObject;
                }
            }
        }

        if (other.gameObject.CompareTag("RedPlayer") || other.gameObject.CompareTag("RedPlayer2"))
        {
            if (rebRobots.Length > 0)
            {
                for (var i = 0; i < rebRobots.Length; i++)
                {
                    if (rebRobots[i] != other.gameObject && rebRobots[i] == null)
                    {
                        rebRobots[i] = other.gameObject;
                        return;
                    }
                    else if (rebRobots[i] == other.gameObject)
                    {
                        return;
                    }
                }
            }
            else
            {
                {
                    rebRobots[0] = other.gameObject;
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Player2"))
        {
            for (var i = 0; i < blueRobots.Length; i++)
            {
                if (blueRobots[i] != other.gameObject && blueRobots[i] == null)
                {
                    blueRobots[i] = other.gameObject;
                    return;
                }
                else if (blueRobots[i] == other.gameObject)
                {
                    return;
                }
            }
        }

        if (other.gameObject.CompareTag("RedPlayer") || other.gameObject.CompareTag("RedPlayer2"))
        {
            for (var i = 0; i < rebRobots.Length; i++)
            {
                if (rebRobots[i] != other.gameObject && rebRobots[i] == null)
                {
                    rebRobots[i] = other.gameObject;
                    return;
                }
                else if (rebRobots[i] == other.gameObject)
                {
                    return;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Player2"))
        {
            for (var i = 0; i < blueRobots.Length; i++)
            {
                if (blueRobots[i] == other.gameObject)
                {
                    blueRobots[i] = null;

                }
            }
        }
        else if (other.gameObject.CompareTag("RedPlayer") || other.gameObject.CompareTag("RedPlayer2"))
        {
            for (var i = 0; i < rebRobots.Length; i++)
            {
                if (rebRobots[i] == other.gameObject)
                {
                    rebRobots[i] = null;

                }
            }
        }
    }
}
