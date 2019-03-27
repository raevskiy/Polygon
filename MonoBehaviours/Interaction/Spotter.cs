using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spotter : MonoBehaviour {
    [SerializeField]
    private GameObject flowchart;
    [SerializeField]
    private Camera spotterCamera;
    [SerializeField]
    private GameObject toCheck;

    private bool spotted;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == toCheck && !spotted && IsInView())
        {
            spotted = true;
            flowchart.SetActive(true);
        }   
    }

    private bool IsInView()
    {
        Vector3 pointOnScreen = spotterCamera.WorldToScreenPoint(toCheck.GetComponentInChildren<Renderer>().bounds.center);

        //Is in front
        if (pointOnScreen.z < 0)
        {
            Debug.Log("Behind: " + toCheck.name);
            return false;
        }

        //Is in FOV
        if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) ||
                (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
        {
            Debug.Log("OutOfBounds: " + toCheck.name);
            return false;
        }

        RaycastHit hit;
        if (Physics.Linecast(spotterCamera.transform.position, toCheck.GetComponentInChildren<Renderer>().bounds.center, out hit))
        {
            if (hit.transform.name != toCheck.name)
            {
                Debug.Log(toCheck.name + " occluded by " + hit.transform.name);
                return false;
            }
        }
        return true;
    }
}
