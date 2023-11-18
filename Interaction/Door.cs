using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    private GameObject lockObject;

    private void OnJointBreak(float breakForce)
    {
        if (lockObject != null)
        {
            Destroy(lockObject);
        }
    }
}
