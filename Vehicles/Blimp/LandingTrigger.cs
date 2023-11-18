using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingTrigger : MonoBehaviour
{
    [SerializeField]
    private LayerMask groundLayers;

    private bool landed;

    private void OnTriggerEnter(Collider other)
    {
        if (groundLayers == (groundLayers | (1 << other.gameObject.layer)))
        {
            landed = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (groundLayers == (groundLayers | (1 << other.gameObject.layer)))
        {
            landed = false;
        }
    }

    public bool IsLanded()
    {
        return landed;
    }
}
